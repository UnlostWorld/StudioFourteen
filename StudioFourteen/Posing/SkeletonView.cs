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
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using StudioFourteen.Plugin;
using StudioFourteen.Utilities;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WpfUtils;
using StudioFourteen.Selection;

[DependencyProperty<SkeletonViewDefinition>("ViewDefinition")]
[DependencyProperty<int>("ObjectTableIndex", DefaultValue = 1)]
[DependencyProperty<bool>("FlipSides", DefaultValue = false)]
[DependencyProperty<bool>("Hide", DefaultValue = false)]
public partial class SkeletonView : Canvas
{
	public const double BackgroundOpacity = 0.25;
	public const double MouseOverDistance = 20;

	protected readonly ILogger Log = Logging.ForContext<SkeletonView>();

	private readonly List<BoneButton> boneButtons = new();
	private readonly List<BoneConnection> boneConnections = new();
	private int backgroundWidth;
	private int backgroundHeight;
	private BoneButton? mouseOver;

	public SkeletonView()
	{
		if (DesignerProperties.GetIsInDesignMode(this))
			return;

		this.Loaded += (s, e) => this.OnLoaded();
		this.Unloaded += (s, e) => this.OnUnloaded();
		this.Dispatcher.ShutdownStarted += (s, e) => this.OnUnloaded();
	}

	public BoneButton? MouseOver
	{
		get => this.mouseOver;
		set
		{
			if (this.mouseOver == value)
				return;

			if (this.mouseOver != null)
				this.mouseOver.IsMouseHover = false;

			this.mouseOver = value;

			if (this.mouseOver != null)
				this.mouseOver.IsMouseHover = true;
		}
	}

	public BoneButton? GetButton(BoneId boneId)
	{
		foreach(BoneButton button in this.boneButtons)
		{
			if (button.Selection is BoneSelection boneSelection)
			{
				foreach (BoneId buttonBoneId in boneSelection.BoneIds)
				{
					if (buttonBoneId == boneId)
					{
						return button;
					}
				}
			}
		}

		return null;
	}

	protected unsafe void Load()
	{
		if (DesignerProperties.GetIsInDesignMode(this))
			return;

		if (this.Hide)
		{
			this.Visibility = Visibility.Collapsed;
			return;
		}

		this.boneButtons.Clear();
		this.boneConnections.Clear();

		this.Children.Clear();

		SkeletonViewDefinition? definition = this.ViewDefinition;
		if (definition == null)
			return;

		this.UpdateBackground();

		if (definition.Size.Width > 0 && definition.Size.Height > 0)
		{
			this.backgroundWidth = (int)definition.Size.Width;
			this.backgroundHeight = (int)definition.Size.Height;
		}

		////this.Width = this.backgroundWidth;
		////this.Height = this.backgroundHeight;

		int objectTableIndex = this.ObjectTableIndex;
		if (this.ObjectTableIndex < 0)
			return;

		Task.Run(() => this.LoadFromTableOrChildren(objectTableIndex, definition));
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
			this.Background = brush;
		}
	}

	protected async Task LoadFromTableOrChildren(int objectTableIndex, SkeletonViewDefinition definition)
	{
		if (DalamudServices.ObjectTable == null)
			return;

		await Threads.FrameworkThread();

		// Check our object table index
		bool result = await this.LoadFromTable(objectTableIndex, definition);

		// check for ornaments
		if (!result)
		{
			int ornamentTableIndex = -1;
			unsafe
			{
				Character* character = (Character*)DalamudServices.ObjectTable.GetObjectAddress(objectTableIndex);
				Ornament* ornament = character->OrnamentData.OrnamentObject;

				if (ornament != null)
				{
					ornamentTableIndex = ornament->ObjectIndex;
				}
			}

			result = await this.LoadFromTable(ornamentTableIndex, definition);
		}

		// TODO: check for mounts?
		await this.Dispatcher.MainThread();
		this.Visibility = result ? Visibility.Visible : Visibility.Collapsed;

		this.OnRenderSizeChanged(null);
	}

	protected async Task<bool> LoadFromTable(int objectTableIndex, SkeletonViewDefinition definition)
	{
		await Threads.FrameworkThread();

		List<SelectionBase> selections = new();

		try
		{
			if (DalamudServices.ObjectTable == null)
				return false;

			unsafe
			{
				Character* character = (Character*)DalamudServices.ObjectTable.GetObjectAddress(objectTableIndex);
				if (character == null)
					return false;

				// populate bones
				foreach ((string name, Point pos) in definition.Bones)
				{
					if (name == "character")
					{
						selections.Add(new GameObjectSelection(objectTableIndex));
						continue;
					}

					BoneSelection? selection = ServiceManager.Instance.Pose.FindBone(character, name);
					if (selection != null)
						selections.Add(selection);

					string? mirrorName = PoseService.GetMirrorBoneName(name);
					if (mirrorName != null)
					{
						selection = ServiceManager.Instance.Pose.FindBone(character, mirrorName);
						if (selection != null)
						{
							selections.Add(selection);
						}
					}
				}
			}
		}
		catch(Exception ex)
		{
			this.Log.Error(ex, "Error getting bone selections");
			return false;
		}

		await this.Dispatcher.MainThread();

		try
		{
			if (selections.Count <= 0)
			{
				return false;
			}
			else
			{
				Dictionary<BoneId, BoneButton> buttonLookup = new();
				foreach (SelectionBase selection in selections)
				{
					BoneButton button = new(selection, this);
					this.boneButtons.Add(button);

					if (selection is BoneSelection boneSelection)
					{
						foreach (BoneId boneId in boneSelection.BoneIds)
						{
							if (!buttonLookup.ContainsKey(boneId))
							{
								buttonLookup.Add(boneId, button);
							}
						}
					}
				}

				foreach(BoneButton button in this.boneButtons)
				{
					if (button.Selection is BoneSelection boneSelection)
					{
						foreach (BoneId parentBoneId in boneSelection.ParentBoneIds)
						{
							BoneButton? parentButton;
							if (buttonLookup.TryGetValue(parentBoneId, out parentButton))
							{
								BoneConnection connection = new(button, parentButton, this);
								this.boneConnections.Add(connection);
							}
						}
					}
				}

				this.OnRenderSizeChanged(null);
				this.OnSelectionChanged(ServiceManager.Instance.Selection.Current);
			}
		}
		catch (Exception ex)
		{
			this.Log.Error(ex, "Error applying bone selections");
		}

		return true;
	}

	protected override HitTestResult? HitTestCore(PointHitTestParameters hitTestParameters)
	{
		double closestDist = double.MaxValue;
		BoneButton? closestLink = null;
		foreach (BoneButton link in this.boneButtons)
		{
			double distance = Point.Subtract(hitTestParameters.HitPoint, link.Position).Length;
			if (distance < closestDist)
			{
				closestDist = distance;
				closestLink = link;
			}
		}

		if (closestLink != null && closestDist < MouseOverDistance)
		{
			return base.HitTestCore(hitTestParameters);
		}
		else
		{
			return null;
		}
	}

	protected override void OnRenderSizeChanged(SizeChangedInfo? sizeInfo)
	{
		if (sizeInfo != null)
			base.OnRenderSizeChanged(sizeInfo);

		if (this.ViewDefinition == null)
			return;

		foreach (BoneButton link in this.boneButtons)
		{
			if (link.Selection is BoneSelection boneSelection)
			{
				link.Position = this.GetPosition(boneSelection.BoneName);
			}
			else if (link.Selection is GameObjectSelection objectSelection)
			{
				link.Position = this.GetPosition("character");
			}
		}

		foreach(BoneConnection connection in this.boneConnections)
		{
			connection.From = connection.FromBone.Position;
			connection.To = connection.ToBone.Position;
		}
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		base.OnMouseMove(e);

		Point mousePos = Mouse.GetPosition(this);

		double closestDist = double.MaxValue;
		BoneButton? closestLink = null;
		foreach (BoneButton link in this.boneButtons)
		{
			double distance = Point.Subtract(mousePos, link.Position).Length;
			if (distance < closestDist)
			{
				closestDist = distance;
				closestLink = link;
			}
		}

		if (closestLink != null && closestDist < MouseOverDistance)
		{
			this.MouseOver = closestLink;
		}
		else
		{
			this.MouseOver = null;
		}
	}

	protected override void OnMouseLeave(MouseEventArgs e)
	{
		base.OnMouseLeave(e);
		this.MouseOver = null;
	}

	protected override void OnMouseUp(MouseButtonEventArgs e)
	{
		base.OnMouseUp(e);

		if (this.MouseOver != null)
		{
			ServiceManager.Instance.Selection.Current = this.MouseOver.Selection;
			e.Handled = true;
		}
	}

	partial void OnViewDefinitionChanged()
	{
		this.Load();
	}

	partial void OnObjectTableIndexChanged()
	{
		this.Load();
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

	private void OnLoaded()
	{
		ServiceManager.Instance.Selection.SelectionChanged += this.OnSelectionChanged;
	}

	private void OnUnloaded()
	{
		if (ServiceManager.ShutdownRequested)
			return;

		ServiceManager.Instance.Selection.SelectionChanged -= this.OnSelectionChanged;
	}

	private void OnSelectionChanged(object? newSelection)
	{
		if (newSelection is SelectionBase selection)
		{
			this.Dispatcher.Invoke(() =>
			{
				foreach (var link in this.boneButtons)
				{
					link.IsSelected = link.Selection.Equals(selection);
				}
			});
		}
	}

	public class BoneButton
	{
		public const double Radius = 7;
		public const double InnerRadius = 3;

		public readonly SelectionBase Selection;

		private readonly Ellipse outer;
		private readonly Ellipse inner;
		private Point position;

		private bool isMouseHover = false;
		private bool isSelected = false;

		public BoneButton(SelectionBase selection, Canvas parent)
		{
			this.Selection = selection;

			this.outer = new();
			this.outer.Width = Radius * 2;
			this.outer.Height = Radius * 2;
			this.outer.SetResourceReference(Ellipse.FillProperty, "ControlBackgroundBrush");
			this.outer.ToolTip = selection.Name;
			parent.Children.Add(this.outer);

			Canvas.SetZIndex(this.outer, 0);

			this.inner = new();
			this.inner.Width = InnerRadius * 2;
			this.inner.Height = InnerRadius * 2;
			this.inner.SetResourceReference(Ellipse.FillProperty, "ForegroundLightBrush");
			this.inner.IsHitTestVisible = false;
			parent.Children.Add(this.inner);

			Canvas.SetZIndex(this.inner, 200);
		}

		public Point Position
		{
			get => this.position;
			set
			{
				this.position = value;
				Canvas.SetLeft(this.outer, value.X - Radius);
				Canvas.SetTop(this.outer, value.Y - Radius);

				Canvas.SetLeft(this.inner, value.X - InnerRadius);
				Canvas.SetTop(this.inner, value.Y - InnerRadius);
			}
		}

		public bool IsMouseHover
		{
			get => this.isMouseHover;
			set
			{
				this.outer.SetResourceReference(Ellipse.FillProperty, value ? "ControlHighlightBrush" : "ControlBackgroundBrush");
				this.isMouseHover = value;
			}
		}

		public bool IsSelected
		{
			get => this.isSelected;
			set
			{
				this.inner.SetResourceReference(Ellipse.FillProperty, value ? "TrimBrush" : "ForegroundLightBrush");
				this.isSelected = value;
			}
		}
	}

	public class BoneConnection
	{
		public readonly BoneButton FromBone;
		public readonly BoneButton ToBone;

		private readonly Line line;

		public BoneConnection(BoneButton fromBone, BoneButton toBone, Canvas parent)
		{
			this.FromBone = fromBone;
			this.ToBone = toBone;

			this.line = new();
			this.line.SetResourceReference(Line.StrokeProperty, "ForegroundLightBrush");
			this.line.StrokeThickness = 1;
			this.line.Opacity = 0.15;

			Canvas.SetZIndex(this.line, 100);

			parent.Children.Add(this.line);
		}

		public Point From
		{
			get => new Point(this.line.X1, this.line.Y1);
			set
			{
				this.line.X1 = value.X;
				this.line.Y1 = value.Y;
			}
		}

		public Point To
		{
			get => new Point(this.line.X2, this.line.Y2);
			set
			{
				this.line.X2 = value.X;
				this.line.Y2 = value.Y;
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