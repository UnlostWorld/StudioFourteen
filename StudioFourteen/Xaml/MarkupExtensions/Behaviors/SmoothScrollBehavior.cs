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

namespace StudioFourteen.Behaviors;

using DependencyPropertyGenerator;
using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Animation;

[AttachedDependencyProperty<bool, ScrollViewer>("SmoothScroll")]
public static partial class SmoothScrollBehavior
{
	public static readonly PropertyInfo? ScrollInfoProperty = typeof(ScrollViewer).GetProperty("ScrollInfo", BindingFlags.NonPublic | BindingFlags.Instance);

	static partial void OnSmoothScrollChanged(ScrollViewer scrollViewer, bool newValue)
	{
		if (newValue != true)
			return;

		if (!scrollViewer.IsLoaded)
		{
			scrollViewer.Loaded += (s, e) =>
			{
				OnSmoothScrollChanged(scrollViewer, newValue);
			};

			return;
		}

		IScrollInfo? scrollInfo = ScrollInfoProperty?.GetValue(scrollViewer) as IScrollInfo;
		if (scrollInfo == null)
			return;

		// don't try to smooth scroll something that is already smooth.
		if (scrollInfo is ISmoothScroll)
			return;

		ScrollInfoProperty?.SetValue(scrollViewer, new ScrollInfoAdapter(scrollInfo));
	}
}

#pragma warning disable
public interface ISmoothScroll
{
	IScrollInfo Original { get; }
}
#pragma warning restore

[DependencyProperty<double>("VerticalScrollActual")]
[DependencyProperty<double>("HorizontalScrollActual")]
public partial class ScrollInfoAdapter : UIElement, IScrollInfo, ISmoothScroll
{
	public const int AnimationDurationMs = 250;

	public const double ScrollLineDelta = 16.0;
	public const double MouseWheelDelta = 160.0;

	public readonly IScrollInfo Original;
	private readonly Storyboard storyboard;
	private readonly DoubleAnimation verticalAnimation;
	private readonly DoubleAnimation horizontalAnimation;

	private bool storyboardIsAttached = false;

	public ScrollInfoAdapter(IScrollInfo original)
	{
		if (original is ScrollInfoAdapter adapter)
			original = adapter.Original;

		this.Original = original;

		this.verticalAnimation = new();
		this.verticalAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(AnimationDurationMs));
		this.verticalAnimation.EasingFunction = new PowerEase()
		{
			EasingMode = EasingMode.EaseOut,
		};

		Storyboard.SetTarget(this.verticalAnimation, this);
		Storyboard.SetTargetProperty(this.verticalAnimation, new PropertyPath(VerticalScrollActualProperty));

		this.horizontalAnimation = new();
		this.horizontalAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(AnimationDurationMs));
		this.horizontalAnimation.EasingFunction = new PowerEase()
		{
			EasingMode = EasingMode.EaseOut,
		};

		Storyboard.SetTarget(this.horizontalAnimation, this);
		Storyboard.SetTargetProperty(this.horizontalAnimation, new PropertyPath(HorizontalScrollActualProperty));

		this.storyboard = new();
		this.storyboard.Children.Add(this.verticalAnimation);
		this.storyboard.Children.Add(this.horizontalAnimation);
	}

	public ScrollViewer ScrollOwner
	{
		get => this.Original.ScrollOwner;
		set => this.Original.ScrollOwner = value;
	}

	public bool CanVerticallyScroll
	{
		get => this.Original.CanVerticallyScroll;
		set => this.Original.CanVerticallyScroll = value;
	}

	public bool CanHorizontallyScroll
	{
		get => this.Original.CanHorizontallyScroll;
		set => this.Original.CanHorizontallyScroll = value;
	}

	public double ExtentWidth => this.Original.ExtentWidth;
	public double ExtentHeight => this.Original.ExtentHeight;
	public double ViewportWidth => this.Original.ViewportWidth;
	public double ViewportHeight => this.Original.ViewportHeight;
	public double HorizontalOffset => this.Original.HorizontalOffset;
	public double VerticalOffset => this.Original.VerticalOffset;

	IScrollInfo ISmoothScroll.Original => this.Original;

	public Rect MakeVisible(Visual visual, Rect rectangle)
	{
		return this.Original.MakeVisible(visual, rectangle);
	}

	public void LineUp() => this.VerticalScroll(-ScrollLineDelta);
	public void LineDown() => this.VerticalScroll(+ScrollLineDelta);
	public void LineLeft() => this.HorizontalScroll(-ScrollLineDelta);
	public void LineRight() => this.HorizontalScroll(+ScrollLineDelta);
	public void MouseWheelUp() => this.VerticalScroll(-MouseWheelDelta);
	public void MouseWheelDown() => this.VerticalScroll(+MouseWheelDelta);
	public void MouseWheelLeft() => this.HorizontalScroll(-MouseWheelDelta);
	public void MouseWheelRight() => this.HorizontalScroll(+MouseWheelDelta);
	public void PageUp() => this.VerticalScroll(-this.ViewportHeight);
	public void PageDown() => this.VerticalScroll(+this.ViewportHeight);
	public void PageLeft() => this.HorizontalScroll(-this.ViewportWidth);
	public void PageRight() => this.HorizontalScroll(+this.ViewportWidth);

	public void SetVerticalOffset(double offset)
	{
		if (offset >= double.PositiveInfinity)
			offset = double.MaxValue;

		if (offset <= double.NegativeInfinity)
			offset = -double.MaxValue;

		this.verticalAnimation.To = offset;
		this.Original.SetVerticalOffset(offset);
	}

	public void SetHorizontalOffset(double offset)
	{
		if (offset >= double.PositiveInfinity)
			offset = double.MaxValue;

		if (offset <= double.NegativeInfinity)
			offset = -double.MaxValue;

		this.verticalAnimation.To = offset;
		this.Original.SetHorizontalOffset(offset);
	}

	private void VerticalScroll(double val)
	{
		double? to = this.verticalAnimation.To;
		if (to == null || (this.storyboardIsAttached && this.storyboard.GetCurrentTime() >= this.verticalAnimation.Duration))
			to = this.Original.VerticalOffset;

		to += val;
		to = Math.Clamp((double)to, 0, this.Original.ScrollOwner.ScrollableHeight);

		this.verticalAnimation.From = this.Original.VerticalOffset;
		this.verticalAnimation.To = to;
		this.storyboard.Begin();
		this.storyboardIsAttached = true;
	}

	private void HorizontalScroll(double val)
	{
		double? to = this.horizontalAnimation.To;
		if (to == null || (this.storyboardIsAttached && this.storyboard.GetCurrentTime() >= this.horizontalAnimation.Duration))
			to = this.Original.HorizontalOffset;

		to += val;
		to = Math.Clamp((double)to, 0, this.Original.ScrollOwner.ScrollableWidth);

		this.horizontalAnimation.From = this.Original.HorizontalOffset;
		this.horizontalAnimation.To = to;
		this.storyboard.Begin();
		this.storyboardIsAttached = true;
	}

	partial void OnVerticalScrollActualChanged(double newValue)
	{
		this.Original.SetVerticalOffset(newValue);
	}

	partial void OnHorizontalScrollActualChanged(double newValue)
	{
		this.Original.SetVerticalOffset(newValue);
	}
}