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

namespace StudioFourteen.Environment;

using StudioFourteen.Controls;
using DependencyPropertyGenerator;
using System;
using StudioFourteen.Utilities;

[DependencyProperty<double>("SunsetAlpha")]
[DependencyProperty<double>("SunriseAlpha")]
[DependencyProperty<double>("NoonAlpha")]
[DependencyProperty<double>("NightAlpha")]
public partial class TimeControl : Slider
{
	protected override void OnValueChanged(double oldValue, double newValue)
	{
		base.OnValueChanged(oldValue, newValue);

		float p = MathUtility.InverseLerp(0, 1440, (float)newValue);

		if (p > 0 && p < 0.25f)
		{
			this.NightAlpha = MathUtility.InverseLerp(0.25f, 0, p);
			this.SunriseAlpha = MathUtility.InverseLerp(0, 0.25f, p);
		}

		if (p > 0.25f && p < 0.5f)
		{
			this.SunriseAlpha = MathUtility.InverseLerp(0.5f, 0.25f, p);
			this.NoonAlpha = MathUtility.InverseLerp(0.25f, 0.5f, p);
		}

		if (p > 0.5f && p < 0.75f)
		{
			this.NoonAlpha = MathUtility.InverseLerp(0.75f, 0.5f, p);
			this.SunsetAlpha = MathUtility.InverseLerp(0.5f, 0.75f, p);
		}

		if (p > 0.75 && p < 1.0f)
		{
			this.SunsetAlpha = MathUtility.InverseLerp(1.0f, 0.75f, p);
			this.NightAlpha = MathUtility.InverseLerp(0.75f, 1.0f, p);
		}
	}
}