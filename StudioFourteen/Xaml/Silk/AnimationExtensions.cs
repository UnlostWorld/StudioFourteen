namespace StudioFourteen.Silk;

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
