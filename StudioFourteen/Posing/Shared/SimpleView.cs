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
using StudioFourteen.Scene;
using StudioFourteen.Scene.GameObjects.Characters;
using StudioFourteen.Scene.GameObjects.Characters.Skeletons;

[DependencyProperty<string>("LayoutName")]
[DependencyProperty<bool>("FlipSides", DefaultValue = false)]
public partial class SimpleView : PoseViewBase
{
	public const double BackgroundOpacity = 0.25;

	private readonly List<BoneConnection> boneConnections = new();
	private readonly Canvas canvas;
	private SimpleViewLayout? layout;
	private double backgroundAspect;

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
			await this.MainThread();

			this.boneConnections.Clear();
			this.canvas.Children.Clear();

			if (this.LayoutName == null)
				return;

			string? layoutName = this.LayoutName;
			this.layout = new();

			do
			{
				SimpleViewLayout? basedOnLayout = null;
				this.Services.Content.SimplePoseLayouts?.TryGetValue(layoutName, out basedOnLayout);
				layoutName = basedOnLayout?.BasedOn;
				this.layout.MergeBasedOn(basedOnLayout);
			}
			while(!string.IsNullOrEmpty(layoutName));

			if (this.layout == null)
				return;

			if(this.layout.BasedOn != null)
			{
				this.Services.Content.SimplePoseLayouts?.TryGetValue(this.layout.BasedOn, out this.layout);
				if (this.layout == null)
					return;
			}

			// populate bones
			foreach ((string name, Point pos) in this.layout.Bones)
			{
				SkeletonBoneControl target = new();
				target.SelectionName = name;
				this.canvas.Children.Add(target);
				Canvas.SetZIndex(target, -100);

				string? mirrorName = SkeletonService.GetMirrorBoneName(name);
				if (mirrorName != null)
				{
					SkeletonBoneControl mirrorTarget = new();
					mirrorTarget.SelectionName = mirrorName;
					this.canvas.Children.Add(mirrorTarget);
					Canvas.SetZIndex(target, -100);
				}
			}

			this.UpdateBackground();

			await base.UpdateTargetsAsync();

			List<SkeletonBoneControl>? allTargets = this.GetTargets();
			if (allTargets == null)
				return;

			await this.MainThread();

			foreach (SkeletonBoneControl target in allTargets)
			{
				if (target.Selection is SkeletonBone boneSelection && boneSelection.Parent != null)
				{
					List<SkeletonBoneControl>? parentControls = this.GetTargets(boneSelection.Parent.BoneName);
					if (parentControls == null)
						continue;

					foreach(SkeletonBoneControl parentControl in parentControls)
					{
						bool exists = false;
						foreach (BoneConnection otherConnection in this.boneConnections)
						{
							exists |= otherConnection.FromBone == target && otherConnection.ToBone == parentControl;
							exists |= otherConnection.ToBone == target && otherConnection.FromBone == parentControl;

							if (exists)
								break;
						}

						if (!exists)
						{
							BoneConnection connection = new(target, parentControl, this.canvas);
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

	protected override void ClearTargets()
	{
		this.canvas.Children.Clear();
		base.ClearTargets();
	}

	protected void UpdateBackground()
	{
		if (this.layout?.Background == null)
			return;

		BitmapImage bmp = new();
		bmp.BeginInit();
		bmp.UriSource = new($"pack://application:,,,/StudioFourteen;component/{this.layout.Background}");
		bmp.EndInit();

		ImageBrush brush = new();
		brush.ImageSource = bmp;
		brush.Stretch = Stretch.Uniform;
		brush.Opacity = BackgroundOpacity;
		brush.AlignmentX = AlignmentX.Center;
		brush.AlignmentY = AlignmentY.Center;
		this.canvas.Background = brush;

		this.backgroundAspect = bmp.Width / bmp.Height;
	}

	protected override void OnRenderSizeChanged(SizeChangedInfo? sizeInfo)
	{
		if (sizeInfo != null)
			base.OnRenderSizeChanged(sizeInfo);

		List<SkeletonBoneControl>? targets = this.GetTargets();
		if (targets == null)
			return;

		foreach (SkeletonBoneControl target in targets)
		{
			if (target.SafeName == null)
				continue;

			Point pos = this.GetPosition(target.SafeName);
			pos.X = Math.Clamp(pos.X, 0, 1);
			pos.Y = Math.Clamp(pos.Y, 0, 1);

			double areaAspect = this.ActualWidth / this.ActualHeight;

			double height = this.ActualHeight;
			double width = height * this.backgroundAspect;

			if (areaAspect < this.backgroundAspect)
			{
				width = this.ActualWidth;
				height = this.ActualWidth * (1 / this.backgroundAspect);
			}

			double xOffset = (this.ActualWidth - width) / 2;
			double yOffset = (this.ActualHeight - height) / 2;

			Canvas.SetLeft(target, xOffset + (pos.X * width) - (target.Width / 2) - target.Margin.Left);
			Canvas.SetTop(target, yOffset + (pos.Y * height) - (target.Height / 2) - target.Margin.Top);
		}

		foreach (BoneConnection connection in this.boneConnections)
		{
			connection.UpdatePositions();
			connection.OnSelectionChanged();
		}
	}

	protected override void OnSelectionChanged(SceneObjectBase? oldSelection, SceneObjectBase? newSelection, object? source)
	{
		base.OnSelectionChanged(oldSelection, newSelection, source);

		this.Dispatcher.Invoke(() =>
		{
			foreach (BoneConnection connection in this.boneConnections)
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

		if (this.FlipSides && canFlip)
		{
			isFlip = !isFlip;
		}

		Point finalPos = default;
		if (isFlip)
		{
			finalPos.X = 1 - pos.X;
		}
		else
		{
			finalPos.X = pos.X;
		}

		finalPos.Y = pos.Y;
		return finalPos;
	}

	partial void OnFlipSidesChanged()
	{
		this.OnRenderSizeChanged(null);
		this.UpdateBackground();
	}

	public class BoneConnection
	{
		public readonly SkeletonBoneControl FromBone;
		public readonly SkeletonBoneControl ToBone;

		private readonly Line line;

		public BoneConnection(SkeletonBoneControl fromBone, SkeletonBoneControl toBone, Canvas parent)
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
	public string? BasedOn { get; set; }

	public void MergeBasedOn(SimpleViewLayout? parent)
	{
		if (parent == null)
			return;

		if (this.Background == null)
			this.Background = parent.Background;

		foreach((string key, Point pos) in parent.Bones)
		{
			if (this.Bones.ContainsKey(key))
				continue;

			this.Bones.Add(key, pos);
		}
	}
}