// .                    @@             _____ _______ _    _ _____ _____ ____
//          @       @@@@@             / ____|__   __| |  | |  __ \_   _/ __ \
//         @@@  @@@@                 | (___    | |  | |  | | |  | || || |  | |
//         @@@@@@@@@  @    @          \___ \   | |  | |  | | |  | || || |  | |
//        @@@@       @@@@@@@          ____) |  | |  | |__| | |__| || || |__| |
//    @@@@@             @@@          |_____/   |_|   \____/|_____/_____\____/
//     @@@      @@@      @@        ___     _    _   _  __   _____  ___  ___  _  _
//      @@    @@@@@@@    @@       |  _|  / _ \ | | | || _ \|_   _|| __|| __|| \| |
//      @@    @@@@@@@    @   @    | __| | (_) || |_| ||   /  | |  | _| | _| | .` |
//    @@@@      @@@      @@@@     |_|    \___/  \___/ |_|_\  |_|  |___||___||_|\_|
//     @@@@             @@@        https://github.com/UnlostWorld/StudioFourteen
//       @@@@@      @@@@@
//        @@@@@@@@@@@@@@                This software is licensed under the
//            @@@@  @                  GNU AFFERO GENERAL PUBLIC LICENSE v3

namespace StudioFourteen.Posing;

using DependencyPropertyGenerator;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using StudioFourteen.Posing.Shared;
using System.Threading.Tasks;
using StudioFourteen.Selection;

[DependencyProperty<SkeletonViewDefinition>("ViewDefinition")]
[DependencyProperty<bool>("FlipSides", DefaultValue = false)]
public partial class SimpleView : PoseViewBase
{
	public const double BackgroundOpacity = 0.25;

	private readonly List<BoneConnection> boneConnections = new();
	private readonly Canvas canvas;
	private int backgroundWidth;
	private int backgroundHeight;

	public SimpleView()
	{
		this.canvas = new();
		this.canvas.IsHitTestVisible = false;
		this.Content = this.canvas;
	}

	protected override async Task UpdateTargetsAsync()
	{
		try
		{
			this.boneConnections.Clear();
			this.canvas.Children.Clear();

			if (this.ViewDefinition == null)
				return;

			// populate bones
			foreach ((string name, Point pos) in this.ViewDefinition.Bones)
			{
				PoseSelectionControl target = new();
				target.SelectionName = name;
				this.canvas.Children.Add(target);
				Canvas.SetZIndex(target, -100);

				string? mirrorName = PoseService.GetMirrorBoneName(name);
				if (mirrorName != null)
				{
					PoseSelectionControl mirrorTarget = new();
					mirrorTarget.SelectionName = mirrorName;
					this.canvas.Children.Add(mirrorTarget);
					Canvas.SetZIndex(target, 100);
				}
			}

			this.UpdateBackground();

			if (this.ViewDefinition.Size.Width > 0 && this.ViewDefinition.Size.Height > 0)
			{
				this.backgroundWidth = (int)this.ViewDefinition.Size.Width;
				this.backgroundHeight = (int)this.ViewDefinition.Size.Height;
			}

			await base.UpdateTargetsAsync();

			List<PoseSelectionControl>? allTargets = this.GetTargets();
			if (allTargets == null)
				return;

			foreach (PoseSelectionControl target in allTargets)
			{
				if (target.Selection is BoneSelection boneSelection)
				{
					foreach ((BoneId boneId, List<BoneId> pathToRoot) in boneSelection.BonePaths)
					{
						if (pathToRoot.Count <= 0)
							continue;

						List<PoseSelectionControl>? parents = this.GetTargets(pathToRoot[0]);
						if (parents == null)
							continue;

						foreach(PoseSelectionControl parent in parents)
						{
							BoneConnection connection = new(target, parent, this.canvas);
							this.boneConnections.Add(connection);
						}
					}
				}
			}

			this.OnRenderSizeChanged(null);
		}
		catch (Exception ex)
		{
			this.Log.Error(ex, "Error getting bone selections");
		}
	}

	protected void UpdateBackground()
	{
		SkeletonViewDefinition? definition = this.ViewDefinition;
		if (definition == null)
			return;

		if (definition.Background != null)
		{
			BitmapImage bmp = new();
			bmp.BeginInit();
			bmp.UriSource = new($"pack://application:,,,/StudioFourteen;component/{definition.Background}");

			if (definition.BackgroundFlipped != null && this.FlipSides)
				bmp.UriSource = new($"pack://application:,,,/StudioFourteen;component/{definition.BackgroundFlipped}");

			bmp.EndInit();

			this.backgroundWidth = bmp.PixelWidth;
			this.backgroundHeight = bmp.PixelHeight;

			ImageBrush brush = new();
			brush.ImageSource = bmp;
			brush.Stretch = Stretch.Uniform;
			brush.Opacity = BackgroundOpacity;
			brush.AlignmentX = AlignmentX.Left;
			brush.AlignmentY = AlignmentY.Top;
			this.canvas.Background = brush;
		}
	}

	protected override void OnRenderSizeChanged(SizeChangedInfo? sizeInfo)
	{
		if (sizeInfo != null)
			base.OnRenderSizeChanged(sizeInfo);

		List<PoseSelectionControl>? targets = this.GetTargets();
		if (targets == null)
			return;

		foreach (PoseSelectionControl target in targets)
		{
			if (target.SafeName == null)
				continue;

			Point pos = this.GetPosition(target.SafeName);
			Canvas.SetLeft(target, pos.X - (target.Width / 2) - target.Margin.Left);
			Canvas.SetTop(target, pos.Y - (target.Height / 2) - target.Margin.Top);
		}

		foreach (BoneConnection connection in this.boneConnections)
		{
			connection.UpdatePositions();
			connection.OnSelectionChanged();
		}
	}

	protected override void OnSelectionChanged(SelectionBase? oldSelection, SelectionBase? newSelection)
	{
		base.OnSelectionChanged(oldSelection, newSelection);

		foreach(BoneConnection connection in this.boneConnections)
		{
			connection.OnSelectionChanged();
		}
	}

	partial void OnViewDefinitionChanged()
	{
		this.UpdateTargets();
	}

	private bool HasBone(string boneName)
	{
		if (this.ViewDefinition == null)
			return false;

		if (boneName.EndsWith("_r"))
			boneName = boneName.Substring(0, boneName.Length - 2) + "_l";

		return this.ViewDefinition.Bones.ContainsKey(boneName);
	}

	private Point GetPosition(string boneName)
	{
		string lookupName = boneName;
		bool isFlip = false;
		bool canFlip = false;
		if (lookupName.EndsWith("_r"))
		{
			isFlip = true;
			lookupName = lookupName.Substring(0, lookupName.Length - 2) + "_l";
			canFlip = true;
		}
		else if (lookupName.EndsWith("_l"))
		{
			canFlip = true;
		}

		Point pos;
		if (this.ViewDefinition == null || !this.ViewDefinition.Bones.TryGetValue(lookupName, out pos))
			return default;

		double scaleY = this.ActualHeight / (double)this.backgroundHeight;
		double scaleX = this.ActualWidth / (double)this.backgroundWidth;
		double scale = Math.Min(scaleX, scaleY);

		if (this.FlipSides && canFlip)
		{
			isFlip = !isFlip;
		}

		Point finalPos = default;
		if (isFlip)
		{
			finalPos.X = (this.backgroundWidth - pos.X) * scale;
		}
		else
		{
			finalPos.X = pos.X * scale;
		}

		finalPos.Y = pos.Y * scale;
		return finalPos;
	}

	partial void OnFlipSidesChanged()
	{
		this.OnRenderSizeChanged(null);
		this.UpdateBackground();
	}

	public class BoneConnection
	{
		public readonly PoseSelectionControl FromBone;
		public readonly PoseSelectionControl ToBone;

		private readonly Line line;

		public BoneConnection(PoseSelectionControl fromBone, PoseSelectionControl toBone, Canvas parent)
		{
			this.FromBone = fromBone;
			this.ToBone = toBone;

			this.line = new();
			this.line.SetResourceReference(Line.StrokeProperty, "ForegroundLightBrush");
			this.line.StrokeThickness = 1;
			this.line.Opacity = 0.15;

			Canvas.SetZIndex(this.line, 0);

			parent.Children.Add(this.line);
		}

		public void UpdatePositions()
		{
			double fromX = Canvas.GetLeft(this.FromBone) + (this.FromBone.Width / 2) + this.FromBone.Margin.Left;
			double fromY = Canvas.GetTop(this.FromBone) + (this.FromBone.Height / 2) + this.FromBone.Margin.Top;
			double toX = Canvas.GetLeft(this.ToBone) + (this.ToBone.Width / 2) + this.ToBone.Margin.Left;
			double toY = Canvas.GetTop(this.ToBone) + (this.ToBone.Height / 2) + this.ToBone.Margin.Top;

			if (double.IsNaN(fromX) || double.IsNaN(fromY) || double.IsNaN(toX) || double.IsNaN(toY))
				return;

			Point from = new Point(fromX, fromY);
			Point to = new Point(toX, toY);
			Vector vector = to - from;
			vector.Normalize();

			from += vector * 6;
			to -= vector * 6;

			if (double.IsNaN(from.X) || double.IsNaN(from.Y) || double.IsNaN(to.X) || double.IsNaN(to.Y))
				return;

			this.line.X1 = from.X;
			this.line.Y1 = from.Y;
			this.line.X2 = to.X;
			this.line.Y2 = to.Y;
		}

		public void OnSelectionChanged()
		{
			if (this.ToBone.IsSelected)
			{
				this.line.SetResourceReference(Line.StrokeProperty, "TrimBrush");
				this.line.Opacity = 0.5;
			}
			else if (this.ToBone.IsParentSelected)
			{
				this.line.SetResourceReference(Line.StrokeProperty, "TrimBrush");
				this.line.Opacity = 0.15;
			}
			else
			{
				this.line.SetResourceReference(Line.StrokeProperty, "ForegroundLightBrush");
				this.line.Opacity = 0.15;
			}
		}
	}
}

public class SkeletonViewDefinition
{
	public string? Background { get; set; }
	public string? BackgroundFlipped { get; set; }
	public Dictionary<string, Point> Bones { get; set; } = new();
	public Size Size { get; set; }
}