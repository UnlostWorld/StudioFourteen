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

using System;
using System.Reflection;
using Dalamud.Plugin;

public static class DalamudInternalServices
{
	public static object PluginManager => Get("Dalamud.Plugin.Internal.PluginManager");

	private static object Get(string typeName)
	{
		Assembly dalamudAssembly = typeof(IDalamudPluginInterface).Assembly;
		Type? pluginManagerType = dalamudAssembly.GetType(typeName);
		if (pluginManagerType == null)
			throw new Exception($"Unable to locate type: {typeName}");

		Type? serviceType = dalamudAssembly.GetType("Dalamud.Service`1");
		if (serviceType == null)
			throw new Exception("Unable to locate service type");

		Type pluginManagerServiceType = serviceType.MakeGenericType(pluginManagerType);
		MethodInfo? getMethod = pluginManagerServiceType.GetMethod("Get");
		if (getMethod == null)
			throw new Exception("Unable to locate service Get method");

		object? result = getMethod.Invoke(null, BindingFlags.Default, null, [], null);
		if (result == null)
			throw new Exception($"Failed to get service instance: {typeName}");

		return result;
	}
}