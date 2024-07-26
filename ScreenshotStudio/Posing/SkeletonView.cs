namespace ScreenshotStudio.Posing;

using FFXIVClientStructs.FFXIV.Client.Game.Character;
using ScreenshotStudio.Utilities;
using ScreenshotStudio.Windows;
using Serilog;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

public class SkeletonView : Canvas
{
	public static readonly DependencyProperty ViewDefinitionProperty = DependencyProperty.Register(
		nameof(SkeletonView.ViewDefinition),
		typeof(PoseViewDefinition),
		typeof(SkeletonView),
		new(null, OnViewDefinitionChanged));

	protected readonly ILogger Log = Logging.ForContext<SkeletonView>();

	private const double BackgroundOpacity = 0.25;
	private const double MouseOverDistance = 20;

	private readonly List<BoneButton> boneButtons = new();
	private readonly List<BoneConnection> boneConnections = new();
	private int backgroundWidth;
	private int backgroundHeight;
	private BoneButton? mouseOver;

	public SkeletonView()
	{
		this.Loaded += (s, e) => this.OnLoaded();
		this.Unloaded += (s, e) => this.OnUnloaded();
		this.Dispatcher.ShutdownStarted += (s, e) => this.OnUnloaded();
	}

	public PoseViewDefinition? ViewDefinition
	{
		get => (PoseViewDefinition?)this.GetValue(ViewDefinitionProperty);
		set => this.SetValue(ViewDefinitionProperty, value);
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
			foreach(BoneId buttonBoneId in button.Selection.BoneIds)
			{
				if (buttonBoneId == boneId)
				{
					return button;
				}
			}
		}

		return null;
	}

	protected unsafe void Load()
	{
		this.boneButtons.Clear();
		this.boneConnections.Clear();

		this.Children.Clear();

		if (this.ViewDefinition == null)
			return;

		this.Margin = this.ViewDefinition.Padding;

		// set background
		if (this.ViewDefinition.Background != null)
		{
			BitmapImage bmp = new();
			bmp.BeginInit();
			bmp.UriSource = new($"pack://application:,,,/ScreenshotStudio;component/{this.ViewDefinition.Background}");
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

		PoseViewDefinition definition = this.ViewDefinition;
		Threads.RunOnFrameworkThread(() =>
		{
			List<BoneSelection> selections = new();

			try
			{
				Character* character = CharacterWindow.GetTarget();
				if (character == null)
					return;

				// populate bones
				foreach ((string name, Point pos) in definition.Bones)
				{
					BoneSelection? selection = ServiceManager.Instance.Pose.FindBone(ref character, name);
					if (selection != null)
						selections.Add(selection);

					if (name.EndsWith("_l"))
					{
						string rName = name.Substring(0, name.Length - 2) + "_r";

						selection = ServiceManager.Instance.Pose.FindBone(ref character, rName);
						if (selection != null)
						{
							selections.Add(selection);
						}
					}
				}
			}
			catch(Exception ex)
			{
				this.Log.Error(ex, "Error getting bone selections");
				return;
			}

			this.Dispatcher.Invoke(() =>
			{
				try
				{
					if (selections.Count <= 0)
					{
						this.Visibility = Visibility.Collapsed;
					}
					else
					{
						this.Visibility = Visibility.Visible;
						Dictionary<BoneId, BoneButton> buttonLookup = new();
						foreach (BoneSelection selection in selections)
						{
							BoneButton button = new(selection, this);
							this.boneButtons.Add(button);

							foreach (BoneId boneId in selection.BoneIds)
							{
								if (!buttonLookup.ContainsKey(boneId))
								{
									buttonLookup.Add(boneId, button);
								}
							}
						}

						foreach(BoneButton button in this.boneButtons)
						{
							foreach (BoneId parentBoneId in button.Selection.ParentBoneIds)
							{
								BoneButton? parentButton;
								if (buttonLookup.TryGetValue(parentBoneId, out parentButton))
								{
									BoneConnection connection = new(button, parentButton, this);
									this.boneConnections.Add(connection);
								}
							}
						}

						this.OnRenderSizeChanged(null);
						this.OnSelectionChanged(ServiceManager.Instance.Pose.Selection);
					}
				}
				catch (Exception ex)
				{
					this.Log.Error(ex, "Error applying bone selections");
				}
			});
		});

		if (this.ViewDefinition.Size.Width > 0 && this.ViewDefinition.Size.Height > 0)
		{
			this.backgroundWidth = (int)this.ViewDefinition.Size.Width;
			this.backgroundHeight = (int)this.ViewDefinition.Size.Height;
		}

		this.Width = this.backgroundWidth;
		this.Height = this.backgroundHeight;
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
			link.Position = this.GetPosition(link.Selection.BoneName);
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

	protected override void OnMouseDown(MouseButtonEventArgs e)
	{
		base.OnMouseDown(e);

		if (this.MouseOver != null)
		{
			ServiceManager.Instance.Pose.Selection = this.MouseOver.Selection;
			e.Handled = true;
		}
	}

	private static void OnViewDefinitionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is SkeletonView view)
		{
			view.Load();
		}
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
		if (lookupName.EndsWith("_r"))
		{
			isFlip = true;
			lookupName = lookupName.Substring(0, lookupName.Length - 2) + "_l";
		}

		Point pos;
		if (this.ViewDefinition == null || !this.ViewDefinition.Bones.TryGetValue(lookupName, out pos))
			return default;

		double scaleY = this.ActualHeight / (double)this.backgroundHeight;
		double scaleX = this.ActualWidth / (double)this.backgroundWidth;
		double scale = Math.Min(scaleX, scaleY);

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

	private void OnLoaded()
	{
		ServiceManager.Instance.Pose.SelectionChanged += this.OnSelectionChanged;
	}

	private void OnUnloaded()
	{
		ServiceManager.Instance.Pose.SelectionChanged -= this.OnSelectionChanged;
	}

	private void OnSelectionChanged(object? newSelection)
	{
		if (newSelection is SelectionBase selection)
		{
			this.Dispatcher.Invoke(() =>
			{
				foreach (var link in this.boneButtons)
				{
					link.IsSelected = link.Selection == selection;
				}
			});
		}
	}

	public class BoneButton
	{
		public const double Radius = 7;
		public const double InnerRadius = 3;

		public readonly BoneSelection Selection;

		private readonly Ellipse outer;
		private readonly Ellipse inner;
		private Point position;

		private bool isMouseHover = false;
		private bool isSelected = false;

		public BoneButton(BoneSelection selection, Canvas parent)
		{
			this.Selection = selection;

			this.outer = new();
			this.outer.Width = Radius * 2;
			this.outer.Height = Radius * 2;
			this.outer.SetResourceReference(Ellipse.FillProperty, "ControlBackgroundBrush");
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

public class PoseViewDefinition
{
	public string? Category { get; set; }
	public string? Background { get; set; }
	public Dictionary<string, Point> Bones { get; set; } = new();
	public Thickness Padding { get; set; }
	public Size Size { get; set; }
}