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

namespace StudioFourteen.Services.Dalamud;

using System;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using global::Dalamud.Plugin;

// ⚠️ WARNING: REFLECTION BASED CRIMES ⚠️
// Dalamud doesn't provide a way to get our own LocalPlugin info for things like the AssemblyLoadContext,
// so we're going hacking through reflection to get it ourselves.
// https://github.com/goatcorp/Dalamud/blob/master/Dalamud/Plugin/Internal/PluginManager.cs
public static class PluginManager
{
	public static LocalPlugin GetLocalPlugin()
	{
		IList installedPlugins = DalamudInternalServices.PluginManager.Property<IList>("InstalledPlugins");

		foreach (object localPlugin in installedPlugins)
		{
			FileInfo location = localPlugin.Property<FileInfo>("DllFile");
			IDalamudPlugin? instance = localPlugin.Field<IDalamudPlugin>("instance");

			if (instance == Studio.Instance)
			{
				AssemblyLoadContext loadContext = localPlugin.Field("loader").Property<AssemblyLoadContext>("LoadContext");

				LocalPlugin result = default;
				result.Interface = instance;
				result.LoadContext = loadContext;
				result.Location = location;
				return result;
			}
		}

		throw new Exception("Failed to get local plugin");
	}

	public struct LocalPlugin
	{
		public IDalamudPlugin Interface;
		public AssemblyLoadContext LoadContext;
		public FileInfo Location;
	}
}