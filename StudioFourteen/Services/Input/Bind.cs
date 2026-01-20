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

namespace StudioFourteen.Services.Input;

using StudioFourteen.Services.Input.Devices;
using System;
using System.Collections.Generic;

public class Bind
{
	public InputAction Action { get; set; }
	public string? PrimaryAxis { get; set; }
	public List<string> ModifierAxes { get; set; } = new();

	public float GetValue()
	{
		if (this.PrimaryAxis == null)
			return 0.0f;

		InputAxis? primaryAxis = Studio.Input.GetAxisId(this.PrimaryAxis);
		if (primaryAxis == null || primaryAxis.IsConsumed)
			return 0.0f;

		float value = primaryAxis.Value;

		foreach (string axisId in this.ModifierAxes)
		{
			InputAxis? modifierAxis = Studio.Input.GetAxisId(axisId);
			if (modifierAxis == null || modifierAxis.IsConsumed)
			{
				value = 0;
			}
			else
			{
				value *= modifierAxis.Value;
			}
		}

		value = Math.Max(value, 0);

		if (value > 0.001f)
		{
			primaryAxis.ConsumedBy = this;
		}

		return value;
	}
}
