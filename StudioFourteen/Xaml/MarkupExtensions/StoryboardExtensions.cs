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

namespace System.Windows.Media.Animation;

using System;
using System.Reflection;

public static class StoryboardExtensions
{
	public static T CreateAnimation<T>(
		this Storyboard self,
		DependencyObject target,
		DependencyProperty targetProperty,
		int durationMs,
		IEasingFunction? easing = null)
		where T : AnimationTimeline, new()
	{
		return self.CreateAnimation<T>(
			target,
			new PropertyPath(targetProperty),
			new Duration(TimeSpan.FromMilliseconds(durationMs)),
			easing);
	}

	public static T CreateAnimation<T>(
		this Storyboard self,
		DependencyObject target,
		PropertyPath targetProperty,
		Duration duration,
		IEasingFunction? easing = null)
		where T : AnimationTimeline, new()
	{
		T animation = new T();
		animation.Duration = duration;
		Storyboard.SetTarget(animation, target);
		Storyboard.SetTargetProperty(animation, targetProperty);
		self.Children.Add(animation);

		if (easing != null)
		{
			PropertyInfo? propertyInfo = typeof(T).GetProperty("EasingFunction");
			if (propertyInfo == null)
				throw new Exception($"No Easing Fucntion on animation type: {typeof(T)}");

			propertyInfo.SetValue(animation, easing);
		}

		return animation;
	}
}

public static class Easing
{
	public static IEasingFunction SineOut => new SineEase() { EasingMode = EasingMode.EaseOut, };
	public static IEasingFunction SineIn => new SineEase() { EasingMode = EasingMode.EaseIn, };
	public static IEasingFunction SineInOut => new SineEase() { EasingMode = EasingMode.EaseInOut, };

	public static IEasingFunction QuadraticOut => new QuadraticEase() { EasingMode = EasingMode.EaseOut, };
	public static IEasingFunction QuadraticIn => new QuadraticEase() { EasingMode = EasingMode.EaseIn, };
	public static IEasingFunction QuadraticInOut => new QuadraticEase() { EasingMode = EasingMode.EaseInOut, };

	public static IEasingFunction ExponentialOut => new ExponentialEase() { EasingMode = EasingMode.EaseOut, };
	public static IEasingFunction ExponentialIn => new ExponentialEase() { EasingMode = EasingMode.EaseIn, };
	public static IEasingFunction ExponentialInOut => new ExponentialEase() { EasingMode = EasingMode.EaseInOut, };
}