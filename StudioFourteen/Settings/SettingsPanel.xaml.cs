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

namespace StudioFourteen.Settings;

using StudioFourteen.Input;
using StudioFourteen.Mvm;
using StudioFourteen.Panels;
using System.Collections.Generic;

public partial class SettingsPanel : Panel
{
	[AutoNotify] public SettingsService.Configuration Settings => this.Services.Settings.Current;

	public Dictionary<KeyBindEvents, KeyBind> Keys
	{
		get
		{
			Dictionary<KeyBindEvents, KeyBind> value = new(this.Services.Input.DefaultKeys);
			foreach((KeyBindEvents evt, KeyBind key) in this.Settings.CustomKeyBinds)
			{
				if (!value.ContainsKey(evt))
					continue;

				value[evt] = key;
			}

			return value;
		}
	}

	private void OnKeyBindChanged(KeyBindEditor sender, KeyBind bind)
	{
		if (sender.Tag is KeyBindEvents evt)
		{
			this.Settings.CustomKeyBinds[evt] = bind;
		}
	}
}
