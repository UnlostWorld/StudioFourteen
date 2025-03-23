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

namespace StudioFourteen.Icons;

using System;

public abstract class IconDefinitionBase
{
	public static IconDefinitionBase? Parse(string? value)
	{
		if (value == null)
			return null;

		string typekey = value.Split("-")[0];
		Type type = GetType(typekey);

		IconDefinitionBase? icon = Activator.CreateInstance(type) as IconDefinitionBase;
		if (icon == null)
			return null;

		string iconString = value.Substring(typekey.Length + 1);
		icon.ParseIcon(iconString);
		return icon;
	}

	public override string ToString() => $"{GetTypeKey(this.GetType())}-{this.IconToString()}";
	protected abstract string? IconToString();
	protected abstract void ParseIcon(string icon);

	private static string GetTypeKey(Type type)
	{
		if (type == typeof(Svg))
			return "svg";

		if (type == typeof(Fa))
			return "fa";

		return string.Empty;
	}

	private static Type GetType(string key)
	{
		switch(key)
		{
			case "svg": return typeof(Svg);
			case "fa": return typeof(Fa);
		}

		throw new NotSupportedException();
	}
}
