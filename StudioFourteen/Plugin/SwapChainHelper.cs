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

namespace StudioFourteen.Plugin;

using Dalamud.Plugin;
using System;
using System.Reflection;

public static class SwapChainHelper
{
	private static readonly PropertyInfo? ReshadeOnPresentProperty;

	static SwapChainHelper()
	{
		Type? type = typeof(IDalamudPluginInterface).Assembly.GetType("Dalamud.Interface.Internal.SwapChainHelper");
		ReshadeOnPresentProperty = type?.GetProperty("ReshadeOnPresent", BindingFlags.Public | BindingFlags.Static);
	}

	public static bool IsReshade => ReshadeOnPresent != 0;

	public static nint ReshadeOnPresent
	{
		get
		{
			if (ReshadeOnPresentProperty == null)
				return 0;

			object? obj = ReshadeOnPresentProperty.GetValue(null);
			if (obj == null)
				return 0;

			return (nint)obj;
		}
	}
}
