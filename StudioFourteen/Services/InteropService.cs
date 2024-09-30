namespace StudioFourteen.Services;

using Dalamud.Game;
using Dalamud.Hooking;
using FFXIVClientStructs;
using Lumina.Text.ReadOnly;
using StudioFourteen.Plugin;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class InteropService : ServiceBase
{
	private static readonly List<HookReference> Hooks = new();

	public static Hook<TDelegate>? HookFromAddress<TDelegate>(nint address, TDelegate detour)
			where TDelegate : System.Delegate
	{
		if (DalamudServices.InteropProvider == null)
			return null;

		try
		{
			string name = typeof(TDelegate).Name;

			Hook<TDelegate> hook = DalamudServices.InteropProvider.HookFromAddress<TDelegate>(address, detour);
			Hooks.Add(new(hook, name));
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
		foreach (HookReference reference in Hooks)
		{
			if (!reference.Hook.IsDisposed)
			{
				this.Log.Warning($"Hook {reference.Name} was not disposed!");
				reference.Hook.Dispose();
			}
		}

		Hooks.Clear();

		return base.Shutdown();
	}

	private class HookReference(IDalamudHook hook, string name)
	{
		public readonly IDalamudHook Hook = hook;
		public readonly string Name = name;
	}
}
