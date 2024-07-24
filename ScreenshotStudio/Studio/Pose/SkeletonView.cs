namespace ScreenshotStudio.Studio.Pose;

using FFXIVClientStructs.FFXIV.Client.Game.Character;
using ScreenshotStudio.Services;
using ScreenshotStudio.Utilities;
using ScreenshotStudio.Windows;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml.Linq;

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

	private readonly List<BoneLink> boneLinks = new();
	private int backgroundWidth;
	private int backgroundHeight;
	private BoneLink? mouseOver;

	public SkeletonView()
	{
		this.Loaded += this.OnLoaded;

		ServiceManager.Instance.Pose.SelectionChanged += this.OnSelectionChanged;
	}

	public PoseViewDefinition? ViewDefinition
	{
		get => (PoseViewDefinition?)this.GetValue(ViewDefinitionProperty);
		set => this.SetValue(ViewDefinitionProperty, value);
	}

	public BoneLink? MouseOver
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

	protected unsafe void Load()
	{
		this.boneLinks.Clear();
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
			Character* character = CharacterWindow.GetTarget();
			if (character == null)
				return;

			// populate bones
			List<BoneSelection> selections = new();
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

			this.Dispatcher.Invoke(() =>
			{
				if (selections.Count <= 0)
				{
					this.Visibility = Visibility.Collapsed;
				}
				else
				{
					this.Visibility = Visibility.Visible;
					foreach (BoneSelection selection in selections)
					{
						this.boneLinks.Add(new(selection, this));
					}

					this.OnRenderSizeChanged(null);
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

	protected override void OnRenderSizeChanged(SizeChangedInfo? sizeInfo)
	{
		if (sizeInfo != null)
			base.OnRenderSizeChanged(sizeInfo);

		if (this.ViewDefinition == null)
			return;

		double scaleY = this.ActualHeight / (double)this.backgroundHeight;
		double scaleX = this.ActualWidth / (double)this.backgroundWidth;
		double scale = Math.Min(scaleX, scaleY);

		foreach(BoneLink link in this.boneLinks)
		{
			string lookupName = link.Selection.BoneName;
			bool isFlip = false;
			if (lookupName.EndsWith("_r"))
			{
				isFlip = true;
				lookupName = lookupName.Substring(0, lookupName.Length - 2) + "_l";
			}

			Point pos;
			if (!this.ViewDefinition.Bones.TryGetValue(lookupName, out pos))
				continue;

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
			link.Position = finalPos;
		}
	}

	private static void OnViewDefinitionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is SkeletonView view)
		{
			view.Load();
		}
	}

	private void OnLoaded(object sender, RoutedEventArgs e)
	{
		Window? wnd = this.FindParent<Window>();
		if (wnd != null)
		{
			wnd.MouseMove += (s, e) => this.OnWindowMouseMove();
			wnd.MouseDown += (s, e) => this.OnWindowMouseDown(e);
		}
	}

	private void OnSelectionChanged(object? newSelection)
	{
		if (newSelection is SelectionBase selection)
		{
			foreach(var link in this.boneLinks)
			{
				link.IsSelected = link.Selection == selection;
			}
		}
	}

	private void OnWindowMouseMove()
	{
		Point mousePos = Mouse.GetPosition(this);

		double closestDist = double.MaxValue;
		BoneLink? closestLink = null;
		foreach (BoneLink link in this.boneLinks)
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

	private void OnWindowMouseDown(MouseButtonEventArgs e)
	{
		if (this.MouseOver != null)
		{
			ServiceManager.Instance.Pose.Selection = this.MouseOver.Selection;
			e.Handled = true;
		}
	}

	public class BoneLink
	{
		public const double Radius = 7;
		public const double InnerRadius = 3;

		public readonly BoneSelection Selection;

		private readonly Ellipse outer;
		private readonly Ellipse inner;
		private Point position;

		private bool isMouseHover = false;
		private bool isSelected = false;

		public BoneLink(BoneSelection selection, Canvas parent)
		{
			this.Selection = selection;

			this.outer = new();
			this.outer.Width = Radius * 2;
			this.outer.Height = Radius * 2;
			this.outer.SetResourceReference(Ellipse.FillProperty, "ControlBackgroundBrush");
			parent.Children.Add(this.outer);

			this.inner = new();
			this.inner.Width = InnerRadius * 2;
			this.inner.Height = InnerRadius * 2;
			this.inner.SetResourceReference(Ellipse.FillProperty, "ForegroundLightBrush");
			this.inner.IsHitTestVisible = false;
			parent.Children.Add(this.inner);
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
}

public class PoseViewDefinition
{
	public string? Category { get; set; }
	public string? Background { get; set; }
	public Dictionary<string, Point> Bones { get; set; } = new();
	public Thickness Padding { get; set; }
	public Size Size { get; set; }
}