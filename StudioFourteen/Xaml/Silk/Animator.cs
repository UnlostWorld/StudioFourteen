namespace StudioFourteen.Silk;

using System.Collections.Generic;

public readonly struct Animator
{
	private readonly List<Animation> animations;

	public Animator(List<Animation> animations)
	{
		this.animations = animations;
	}

	public bool IsPlaying
	{
		get
		{
			foreach (Animation anim in this.animations)
			{
				if (anim.IsPlaying)
				{
					return true;
				}
			}

			return false;
		}
	}

	public void Stop()
	{
		foreach (Animation anim in this.animations)
		{
			anim.Stop();
		}
	}

	public void Play()
	{
		foreach (Animation anim in this.animations)
		{
			anim.Play();
		}
	}
}