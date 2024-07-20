namespace ScreenshotStudio.Plugin;
using System;
using System.Reflection;

public static class InterfaceManager
{
	private static readonly object? DalamudInterfaceManager;
	private static readonly MethodInfo? RunBeforeImGuiRenderMethod;

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
	}

	public static void RunBeforeImGuiRender(Action action)
	{
		if (RunBeforeImGuiRenderMethod == null)
			return;

		RunBeforeImGuiRenderMethod.Invoke(DalamudInterfaceManager, [action]);
	}
}
