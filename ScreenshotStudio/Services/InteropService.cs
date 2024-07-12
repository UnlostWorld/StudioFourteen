namespace ScreenshotStudio.Services;

using Dalamud.Game;
using Dalamud.Hooking;
using FFXIVClientStructs;
using ScreenshotStudio.Plugin;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class InteropService : ServiceBase
{
	private static readonly List<IDalamudHook> Hooks = new();

	public static Hook<TDelegate>? HookFromAddress<TDelegate>(nint address, TDelegate detour)
			where TDelegate : System.Delegate
	{
		if (DalamudServices.InteropProvider == null)
			return null;

		try
		{
			Hook<TDelegate> hook = DalamudServices.InteropProvider.HookFromAddress<TDelegate>(address, detour);
			Hooks.Add(hook);
			return hook;
		}
		catch (Exception ex)
		{
			Logging.ForContext<DalamudServices>().Error(ex, "Error creating hook from address");
			return null;
		}
	}

	public static Hook<TDelegate>? HookFromSignature<TDelegate>(string sig, TDelegate detour)
		where TDelegate : System.Delegate
	{
		if (DalamudServices.SigScanner == null)
			return null;

		try
		{
			nint address = DalamudServices.SigScanner.ScanText(sig);
			return HookFromAddress<TDelegate>(address, detour);
		}
		catch (Exception ex)
		{
			Logging.ForContext<DalamudServices>().Error(ex, "Error creating hook from signature");
			return null;
		}
	}

	public override Task Initialize()
	{
		return base.Initialize();
	}

	public override Task Shutdown()
	{
		int leaks = 0;
		foreach (IDalamudHook hook in Hooks)
		{
			if (!hook.IsDisposed)
			{
				this.Log.Warning($"Hook {hook.Address} was not disposed!");
				leaks++;
			}
		}

		if (leaks <= 0)
		{
			this.Log.Information($"No hooks leaked during shutdown");
		}

		return base.Shutdown();
	}
}
