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
using StudioFourteen.Utilities;

[DependencyProperty<double>("DownOpacity")]
[DependencyProperty<double>("LeftOpacity")]
[DependencyProperty<double>("TopOpacity")]
[DependencyProperty<double>("RightOpacity")]
[DependencyProperty<object>("DownIcon")]
[DependencyProperty<object>("LeftIcon")]
[DependencyProperty<object>("TopIcon")]
[DependencyProperty<object>("RightIcon")]
public partial class IconSlider : Slider
{
	protected override void OnValueChanged(double oldValue, double newValue)
	{
		base.OnValueChanged(oldValue, newValue);

		float p = MathUtility.InverseLerp(0, (float)this.Maximum, (float)newValue);

		this.DownOpacity = 0;
		this.LeftOpacity = 0;
		this.TopOpacity = 0;
		this.RightOpacity = 0;

		if (p >= 0 && p <= 0.25f)
		{
			this.DownOpacity = MathUtility.InverseLerp(0.25f, 0, p);
			this.LeftOpacity = MathUtility.InverseLerp(0, 0.25f, p);
		}

		if (p >= 0.25f && p <= 0.5f)
		{
			this.LeftOpacity = MathUtility.InverseLerp(0.5f, 0.25f, p);
			this.TopOpacity = MathUtility.InverseLerp(0.25f, 0.5f, p);
		}

		if (p >= 0.5f && p <= 0.75f)
		{
			this.TopOpacity = MathUtility.InverseLerp(0.75f, 0.5f, p);
			this.RightOpacity = MathUtility.InverseLerp(0.5f, 0.75f, p);
		}

		if (p >= 0.75 && p <= 1.0f)
		{
			this.RightOpacity = MathUtility.InverseLerp(1.0f, 0.75f, p);
			this.DownOpacity = MathUtility.InverseLerp(0.75f, 1.0f, p);
		}
	}
}