namespace ScreenshotStudio.Plugin;

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
