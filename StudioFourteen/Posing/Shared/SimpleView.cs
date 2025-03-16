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
using WpfUtils;
using System.Windows.Input;
using StudioFourteen.Serialization;
using System.IO;

[DependencyProperty<string>("LayoutName")]
[DependencyProperty<bool>("FlipSides", DefaultValue = false)]
public partial class SimpleView : PoseViewBase
{
	public const double BackgroundOpacity = 0.25;

	private readonly List<BoneConnection> boneConnections = new();
	private readonly Canvas canvas;
	private int backgroundWidth;
	private int backgroundHeight;
	private SimpleViewLayout? layout;

	public SimpleView()
	{
		this.canvas = new();
		this.canvas.IsHitTestVisible = false;
		this.Content = this.canvas;
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		base.OnMouseMove(e);

		if (this.layout == null)
			return;

		Point mousePos = e.GetPosition(this.canvas);
		if (e.RightButton == MouseButtonState.Pressed)
		{
			if (this.Services.Selection.Current is BoneSelection boneSelection)
			{
				foreach ((BoneId boneId, _) in boneSelection.BonePaths)
				{
					List<PoseSelectionControl>? targets = this.GetTargets(boneId);
					if (targets == null)
						continue;

					foreach(PoseSelectionControl target in targets)
					{
						string? name = target.SafeName;
						if (name == null)
							continue;

						Point pos = this.layout.Bones[name];
						pos.X = mousePos.X / this.canvas.Width;
						pos.Y = mousePos.Y / this.canvas.Height;
						this.layout.Bones[name] = pos;
					}
				}

				this.OnRenderSizeChanged(null);
			}
		}
	}

	protected override void OnMouseUp(MouseButtonEventArgs e)
	{
		base.OnMouseUp(e);

		if (e.ChangedButton == MouseButton.Middle && this.layout != null)
		{
			string json = Serializer.Serialize(this.layout);
			File.WriteAllText("Output.jsonc", json);
		}
	}

	protected override async Task UpdateTargetsAsync()
	{
		try
		{
			await this.MainThread();

			this.boneConnections.Clear();
			this.canvas.Children.Clear();

			if (this.LayoutName == null
			|| !this.Services.Data.SimplePoseLayouts?.TryGetValue(this.LayoutName, out this.layout) == true
			|| this.layout == null)
				return;

			// populate bones
			foreach ((string name, Point pos) in this.layout.Bones)
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
					Canvas.SetZIndex(target, -100);
				}
			}

			this.UpdateBackground();

			if (this.layout.Size.Width > 0 && this.layout.Size.Height > 0)
			{
				this.backgroundWidth = (int)this.layout.Size.Width;
				this.backgroundHeight = (int)this.layout.Size.Height;
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
		if (this.layout?.Background == null)
			return;

		BitmapImage bmp = new();
		bmp.BeginInit();
		bmp.UriSource = new($"pack://application:,,,/StudioFourteen;component/{this.layout.Background}");
		bmp.EndInit();

		this.backgroundWidth = bmp.PixelWidth;
		this.backgroundHeight = bmp.PixelHeight;

		ImageBrush brush = new();
		brush.ImageSource = bmp;
		brush.Stretch = Stretch.None;
		brush.Opacity = BackgroundOpacity;
		////brush.AlignmentX = AlignmentX.Left;
		////brush.AlignmentY = AlignmentY.Top;
		this.canvas.Background = brush;

		this.canvas.Width = bmp.PixelWidth;
		this.canvas.Height = bmp.PixelHeight;
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
			pos.X = Math.Clamp(pos.X, 0, 1);
			pos.Y = Math.Clamp(pos.Y, 0, 1);

			Canvas.SetLeft(target, (pos.X * this.canvas.Width) - (target.Width / 2) - target.Margin.Left);
			Canvas.SetTop(target, (pos.Y * this.canvas.Height) - (target.Height / 2) - target.Margin.Top);
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

		this.Dispatcher.Invoke(() =>
		{
			foreach(BoneConnection connection in this.boneConnections)
			{
				connection.OnSelectionChanged();
			}
		});
	}

	partial void OnLayoutNameChanged()
	{
		this.UpdateTargets();
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
		if (this.layout == null || !this.layout.Bones.TryGetValue(lookupName, out pos))
			return default;

		/*double scaleY = this.ActualHeight / (double)this.backgroundHeight;
		double scaleX = this.ActualWidth / (double)this.backgroundWidth;
		double scale = Math.Min(scaleX, scaleY);*/

		double scale = 1.0;

		if (this.FlipSides && canFlip)
		{
			isFlip = !isFlip;
		}

		Point finalPos = default;
		if (isFlip)
		{
			finalPos.X = (1 - pos.X) * scale;
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

public class SimpleViewLayout
{
	public string? Background { get; set; }
	public Dictionary<string, Point> Bones { get; set; } = new();
	public Size Size { get; set; }
}