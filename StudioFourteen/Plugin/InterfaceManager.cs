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

using Dalamud.Hooking;
using System;
using System.Reflection;
using System.Runtime.InteropServices;

public static class InterfaceManager
{
	private static readonly object? DalamudInterfaceManager;
	private static readonly MethodInfo? RunBeforeImGuiRenderMethod;
	private static readonly MethodInfo? RunAfterImGuiRenderMethod;
	private static readonly FieldInfo? ReshadeOnPresentHookField;
	private static readonly MethodInfo? ReshadeOnPresentDetourMethod;

	static InterfaceManager()
	{
		Type? textureManagerType = DalamudServices.TextureProvider?.GetType();
		PropertyInfo? managerProperty = textureManagerType?.GetProperty("ManagerOrThrow", BindingFlags.NonPublic | BindingFlags.Instance);
		object? textureManager = managerProperty?.GetValue(DalamudServices.TextureProvider);
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

		ReshadeOnPresentHookField = DalamudInterfaceManager?.GetType().GetField(
			"reshadeOnPresentHook",
			BindingFlags.NonPublic | BindingFlags.Instance);

		ReshadeOnPresentDetourMethod = DalamudInterfaceManager?.GetType().GetMethod(
			"ReshadeOnPresentDetour",
			BindingFlags.NonPublic | BindingFlags.Instance);
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

	public static void DisableReshadePresent()
	{
		if (ReshadeOnPresentHookField == null)
			return;

		object? hook = ReshadeOnPresentHookField.GetValue(DalamudInterfaceManager);
		MethodInfo? method = hook?.GetType().GetMethod("Disable");
		method?.Invoke(hook, null);
	}

	public static void EnableReshadePresent()
	{
		if (ReshadeOnPresentHookField == null)
			return;

		object? hook = ReshadeOnPresentHookField.GetValue(DalamudInterfaceManager);
		MethodInfo? method = hook?.GetType().GetMethod("Enable");
		method?.Invoke(hook, null);
	}

	public static void ReshadeOnPresentDetour(nint swapChain, uint flags, nint presentParams)
	{
		if (ReshadeOnPresentDetourMethod == null)
			return;

		ReshadeOnPresentDetourMethod?.Invoke(DalamudInterfaceManager, [swapChain, flags, presentParams]);
	}
}
