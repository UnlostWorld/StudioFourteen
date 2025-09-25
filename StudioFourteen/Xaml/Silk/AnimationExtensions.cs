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

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Windows;

public static class AnimationExtensions
{
	public static void PlayAnimation(this UIElement self, string key)
	{
		self.GetAnimator(key).Play();
	}

	public static void StopAnimation(this UIElement self, string key)
	{
		self.GetAnimator(key).Stop();
	}

	public static Animator GetAnimator(this UIElement self, string key)
	{
		List<Animation> allAnimations = self.FindChildren<Animation>();
		List<Animation> animations = new();
		foreach (Animation anim in allAnimations)
		{
			if (anim.Key == key)
			{
				anim.Stop();
				animations.Add(anim);
			}
		}

		return new Animator(animations);
	}
}
