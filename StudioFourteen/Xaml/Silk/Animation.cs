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

namespace StudioFourteen.Xaml.Silk;

using System;
using System.Windows;
using System.Windows.Media.Animation;

public abstract class Animation : UIElement
{
	private Storyboard? storyboard;

	public string? Key { get; set; }
	public ushort Delay { get; set; } = 0;

	public bool IsPlaying { get; private set; }

	public void Play(DependencyObject? target = null)
	{
		this.IsPlaying = true;

		this.Dispatcher.Invoke(() =>
		{
			if (target == null)
				target = this.VisualParent;

			if (this.storyboard == null)
			{
				this.storyboard = new();
				this.storyboard.Completed += this.OnStoryboardCompleted;
				Storyboard.SetTarget(this.storyboard, target);

				AnimationTimeline timeline = this.GetTimeline();
				timeline.BeginTime = AnimationUtils.GetTimeSpan(this.Delay);
				this.storyboard.Children.Add(timeline);
			}

			this.storyboard.Begin();
		});
	}

	public void Stop()
	{
		if (this.storyboard == null)
			return;

		this.storyboard.SkipToFill();
		this.storyboard.Stop();
		this.IsPlaying = false;
	}

	protected abstract AnimationTimeline GetTimeline();

	private void OnStoryboardCompleted(object? sender, EventArgs e)
	{
		this.IsPlaying = false;
	}
}

public partial class DoubleAnimation : Animation
{
	public enum Easings
	{
		None,
		SineInOut,
		SineIn,
		SineOut,
	}

	public ushort Duration { get; set; } = 0;
	public PropertyPath? Property { get; set; }
	public Easings Easing { get; set; } = Easings.None;
	public double To { get; set; }
	public double From { get; set; }

	protected override AnimationTimeline GetTimeline()
	{
		System.Windows.Media.Animation.DoubleAnimation timeline = new(this.To, AnimationUtils.GetDuration(this.Duration));

		if (this.Easing == Easings.SineIn)
		{
			timeline.EasingFunction = new SineEase() { EasingMode = EasingMode.EaseIn, };
		}
		else if (this.Easing == Easings.SineOut)
		{
			timeline.EasingFunction = new SineEase() { EasingMode = EasingMode.EaseOut, };
		}
		else if (this.Easing == Easings.SineInOut)
		{
			timeline.EasingFunction = new SineEase() { EasingMode = EasingMode.EaseInOut, };
		}

		Storyboard.SetTargetProperty(timeline, this.Property);
		return timeline;
	}
}

public partial class VisibilityAnimation : Animation
{
	public Visibility To { get; set; }

	protected override AnimationTimeline GetTimeline()
	{
		ObjectAnimationUsingKeyFrames kf = new ObjectAnimationUsingKeyFrames();
		kf.KeyFrames.Add(new DiscreteObjectKeyFrame(this.To, KeyTime.FromPercent(1)));

		Storyboard.SetTargetProperty(kf, new(UIElement.VisibilityProperty));
		return kf;
	}
}