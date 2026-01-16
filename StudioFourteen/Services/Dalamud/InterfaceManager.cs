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
using System.Reflection;
using System.Runtime.InteropServices;

// ⚠️ WARNING: REFLECTION BASED CRIMES ⚠️
public static class InterfaceManager
{
	private static readonly object? DalamudInterfaceManager;
	private static readonly MethodInfo? RunBeforeImGuiRenderMethod;
	private static readonly MethodInfo? RunAfterImGuiRenderMethod;

	static InterfaceManager()
	{
		Type? textureManagerType = Studio.TextureProvider?.GetType();
		PropertyInfo? managerProperty = textureManagerType?.GetProperty("ManagerOrThrow", BindingFlags.NonPublic | BindingFlags.Instance);
		object? textureManager = managerProperty?.GetValue(Studio.TextureProvider);
		FieldInfo? interfaceManagerField = textureManager?.GetType().GetField("interfaceManager", BindingFlags.NonPublic | BindingFlags.Instance);

		DalamudInterfaceManager = interfaceManagerField?.GetValue(textureManager);

		RunBeforeImGuiRenderMethod = DalamudInterfaceManager?.GetType().GetMethod(
			"RunBeforeImGuiRender",
			BindingFlags.Public | BindingFlags.Instance,
			[typeof(Action)]);

		RunAfterImGuiRenderMethod = DalamudInterfaceManager?.GetType().GetMethod(
			"RunAfterImGuiRender",
			BindingFlags.Public | BindingFlags.Instance,
			[typeof(Action)]);
	}

	[UnmanagedFunctionPointer(CallingConvention.ThisCall)]
	public delegate void ReshadeOnPresentDelegate(nint swapChain, uint flags, nint presentParams);

	public static void RunBeforeImGuiRender(Action action)
	{
		if (RunBeforeImGuiRenderMethod == null)
			return;

		RunBeforeImGuiRenderMethod.Invoke(DalamudInterfaceManager, [action]);
	}

	public static void RunAfterImGuiRender(Action action)
	{
		if (RunAfterImGuiRenderMethod == null)
			return;

		RunAfterImGuiRenderMethod.Invoke(DalamudInterfaceManager, [action]);
	}
}
