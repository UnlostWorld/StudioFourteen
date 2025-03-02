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

namespace StudioFourteen.Scripting.Instance;

using System;
using System.Collections.Generic;

public class OptionsInterface : ScriptServiceBase
{
	internal Dictionary<string, object> Options = new();

	public bool GetCheckBox(string name) => this.Get<bool>(name);
	public bool GetToggle(string name) => this.Get<bool>(name);
	public string GetInput(string name) => this.Get<string>(name);

	private T Get<T>(string name)
	{
		if (this.Options.TryGetValue(name, out object? value))
		{
			if (value is T tValue)
			{
				return tValue;
			}
			else
			{
				throw new Exception($"Option {name} was wrong type");
			}
		}

		throw new Exception($"Option {name} not found (Did you add it to the script header?)");
	}
}