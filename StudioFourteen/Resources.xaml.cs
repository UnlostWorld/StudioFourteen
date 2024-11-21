// .                      @@             _____ _______ _    _ _____ _____ ____
//            @       @@@@@             / ____|__   __| |  | |  __ \_   _/ __ \
//           @@@  @@@@                 | (___    | |  | |  | | |  | || || |  | |
//           @@@@@@@@@  @    @          \___ \   | |  | |  | | |  | || || |  | |
//          @@@@       @@@@@@@          ____) |  | |  | |__| | |__| || || |__| |
//      @@@@@             @@@          |_____/   |_|   \____/|_____/_____\____/
//       @@@      @@@      @@        ___     _    _   _  __   _____  ___  ___  _  _
//        @@    @@@@@@@    @@       |  _|  / _ \ | | | || _ \|_   _|| __|| __|| \| |
//        @@    @@@@@@@    @   @    | __| | (_) || |_| ||   /  | |  | _| | _| | .` |
//      @@@@      @@@      @@@@     |_|    \___/  \___/ |_|_\  |_|  |___||___||_|\_|
//       @@@@             @@@
//         @@@@@      @@@@@               This software is licensed under the
//          @@@@@@@@@@@@@@                 GNU AFFERO GENERAL PUBLIC LICENSE
//              @@@@  @                       Version 3, 19 November 2007

namespace StudioFourteen;

using System;
using System.Windows;

public partial class Resources : ResourceDictionary
{
	public static Resources? Shared { get; private set; }

	public static Resources Load()
	{
		LoadShared();

		Resources resources = new();
		resources.Source = new("pack://application:,,,/StudioFourteen;component/Resources.xaml");
		return resources;
	}

	public static void LoadShared()
	{
		if (Shared != null)
			return;

		try
		{
			Resources resources = new();
			resources.Source = new("pack://application:,,,/StudioFourteen;component/Resources.xaml");
			Shared = resources;
		}
		catch (Exception ex)
		{
			Logging.Shared.Error(ex, "Error loading resources");
		}
	}

	public static string Find(object key, string fallback)
	{
		object? val = Find(key, (object?)fallback);

		if (val is string valStr)
			return valStr;

		return fallback;
	}

	public static object? Find(object key, object? fallback = null)
	{
		if (Shared == null)
		{
			Logging.Shared.Warning($"No shared resources for lookup: {key}, using fallback {fallback}");
			return fallback;
		}

		if (Shared.Contains(key))
			return Shared[key];

		return fallback;
	}

	public static string Format(object key, params object?[] args)
	{
		return string.Format(Find(key, string.Empty), args);
	}
}
