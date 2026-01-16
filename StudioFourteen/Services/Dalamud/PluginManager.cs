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
		object pluginManager = DalamudInternalServices.PluginManager;
		Type pluginManagerType = pluginManager.GetType();

		PropertyInfo? installedPluginsProperty = pluginManagerType.GetProperty("InstalledPlugins");
		if (installedPluginsProperty == null)
			throw new Exception("Failed to locate InstalledPlugins property");

		IList? installedPlugins = installedPluginsProperty.GetValue(pluginManager) as IList;
		if (installedPlugins == null)
			throw new Exception("Failed to get installed plugins list");

		foreach (object localPlugin in installedPlugins)
		{
			Type localPluginType = localPlugin.GetType();
			if (localPluginType.Name == "LocalDevPlugin" && localPluginType.BaseType != null)
				localPluginType = localPluginType.BaseType;

			FieldInfo? instanceField = localPluginType.GetField("instance", BindingFlags.NonPublic | BindingFlags.Instance);
			if (instanceField == null)
				throw new Exception("Failed to locate instance field");

			PropertyInfo? dllFileField = localPluginType.GetProperty("DllFile", BindingFlags.Public | BindingFlags.Instance);
			if (dllFileField == null)
				throw new Exception("Failed to locate dllFile property");

			FileInfo? location = dllFileField.GetValue(localPlugin) as FileInfo;
			if (location == null)
				continue;

			IDalamudPlugin? instance = instanceField.GetValue(localPlugin) as IDalamudPlugin;
			if (instance != null && instance == Studio.Instance)
			{
				FieldInfo? loaderFieldInfo = localPluginType.GetField("loader", BindingFlags.NonPublic | BindingFlags.Instance);
				if (loaderFieldInfo == null)
					throw new Exception("Failed to get loader field");

				object? pluginLoader = loaderFieldInfo.GetValue(localPlugin);
				if (pluginLoader == null)
					throw new Exception("Failed to get plugin loader");

				PropertyInfo? loadContextPropertyInfo = pluginLoader.GetType().GetProperty("LoadContext", BindingFlags.Public | BindingFlags.Instance);
				if (loadContextPropertyInfo == null)
					throw new Exception("failed to locate load context property");

				AssemblyLoadContext? loadContext = loadContextPropertyInfo.GetValue(pluginLoader) as AssemblyLoadContext;
				if (loadContext == null)
					throw new Exception("Failed to get load context");

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