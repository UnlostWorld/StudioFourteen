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