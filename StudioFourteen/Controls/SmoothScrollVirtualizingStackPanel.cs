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

namespace StudioFourteen.Controls;

using DependencyPropertyGenerator;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Animation;
using StudioFourteen.Behaviors;

[DependencyProperty<double>("VerticalScrollActual")]
[DependencyProperty<double>("HorizontalScrollActual")]
public partial class SmoothScrollVirtualizingStackPanel : VirtualizingStackPanel, ISmoothScroll
{
	private readonly Storyboard storyboard;
	private readonly DoubleAnimation verticalAnimation;
	private readonly DoubleAnimation horizontalAnimation;

	public SmoothScrollVirtualizingStackPanel()
	{
		this.verticalAnimation = new();
		this.verticalAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(ScrollInfoAdapter.AnimationDurationMs));
		this.verticalAnimation.EasingFunction = new PowerEase()
		{
			EasingMode = EasingMode.EaseOut,
		};

		Storyboard.SetTarget(this.verticalAnimation, this);
		Storyboard.SetTargetProperty(this.verticalAnimation, new PropertyPath(VerticalScrollActualProperty));

		this.horizontalAnimation = new();
		this.horizontalAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(ScrollInfoAdapter.AnimationDurationMs));
		this.horizontalAnimation.EasingFunction = new PowerEase()
		{
			EasingMode = EasingMode.EaseOut,
		};

		Storyboard.SetTarget(this.horizontalAnimation, this);
		Storyboard.SetTargetProperty(this.horizontalAnimation, new PropertyPath(HorizontalScrollActualProperty));

		this.storyboard = new();
		this.storyboard.Children.Add(this.verticalAnimation);
		this.storyboard.Children.Add(this.horizontalAnimation);
		this.storyboard.Begin();
	}

	public IScrollInfo Original => this;

	public override void LineUp() => this.VerticalScroll(-ScrollInfoAdapter.ScrollLineDelta);
	public override void LineDown() => this.VerticalScroll(+ScrollInfoAdapter.ScrollLineDelta);
	public override void LineLeft() => this.HorizontalScroll(-ScrollInfoAdapter.ScrollLineDelta);
	public override void LineRight() => this.HorizontalScroll(+ScrollInfoAdapter.ScrollLineDelta);
	public override void MouseWheelUp() => this.VerticalScroll(-ScrollInfoAdapter.MouseWheelDelta);
	public override void MouseWheelDown() => this.VerticalScroll(+ScrollInfoAdapter.MouseWheelDelta);
	public override void MouseWheelLeft() => this.HorizontalScroll(-ScrollInfoAdapter.MouseWheelDelta);
	public override void MouseWheelRight() => this.HorizontalScroll(+ScrollInfoAdapter.MouseWheelDelta);
	public override void PageUp() => this.VerticalScroll(-this.ViewportHeight);
	public override void PageDown() => this.VerticalScroll(+this.ViewportHeight);
	public override void PageLeft() => this.HorizontalScroll(-this.ViewportWidth);
	public override void PageRight() => this.HorizontalScroll(+this.ViewportWidth);

	private void VerticalScroll(double val)
	{
		double? to = this.verticalAnimation.To;
		if (to == null || this.storyboard.GetCurrentTime() >= this.verticalAnimation.Duration)
			to = this.Original.VerticalOffset;

		to += val;
		to = Math.Clamp((double)to, 0, this.Original.ScrollOwner.ScrollableHeight);

		this.verticalAnimation.From = this.Original.VerticalOffset;
		this.verticalAnimation.To = to;
		this.storyboard.Begin();
	}

	private void HorizontalScroll(double val)
	{
		double? to = this.horizontalAnimation.To;
		if (to == null || this.storyboard.GetCurrentTime() >= this.horizontalAnimation.Duration)
			to = this.Original.HorizontalOffset;

		to += val;
		to = Math.Clamp((double)to, 0, this.Original.ScrollOwner.ScrollableWidth);

		this.horizontalAnimation.From = this.Original.HorizontalOffset;
		this.horizontalAnimation.To = to;
		this.storyboard.Begin();
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
