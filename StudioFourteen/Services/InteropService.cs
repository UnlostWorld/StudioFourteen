namespace StudioFourteen.Services;

using Dalamud.Hooking;
using StudioFourteen.Plugin;
using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TerraFX.Interop.Windows;

public class InteropService : ServiceBase
{
	private static readonly List<HookReference> Hooks = new();

	public static Hook<TDelegate>? HookFromSignature<TDelegate>(string sig, TDelegate detour)
		where TDelegate : System.Delegate
	{
		if (DalamudServices.SigScanner == null)
			return null;

		string name = typeof(TDelegate).Name;

		try
		{
			nint address = DalamudServices.SigScanner.ScanText(sig);
			return HookFromAddress<TDelegate>(address, detour);
		}
		catch (Exception ex)
		{
			Logging.ForContext<DalamudServices>().Error(ex, $"Error creating hook {name} from signature");
			return null;
		}
	}

	public static Hook<TDelegate>? HookFromAddress<TDelegate>(nint address, TDelegate detour)
			where TDelegate : System.Delegate
	{
		if (DalamudServices.InteropProvider == null)
			return null;

		string name = typeof(TDelegate).Name;

		try
		{
			Logging.Shared.Information($"Created Hook {name} for address {address}");

			Hook<TDelegate> hook = DalamudServices.InteropProvider.HookFromAddress<TDelegate>(address, detour);
			Hooks.Add(new(hook, name));
			return hook;
		}
		catch (Exception ex)
		{
			Logging.ForContext<DalamudServices>().Error(ex, $"Error creating hook {name} from address");
			return null;
		}
	}

	public static void CheckHooks()
	{
		foreach (HookReference reference in Hooks)
		{
			if (!reference.Hook.IsDisposed)
			{
				Logging.Shared.Error($"Hook {reference.Name} was not disposed!");
				reference.Hook.Dispose();
			}
			else
			{
				Logging.Shared.Information($"Disposed Hook {reference.Name}");
			}
		}

		Hooks.Clear();
	}

	private class HookReference(IDalamudHook hook, string name)
	{
		public readonly IDalamudHook Hook = hook;
		public readonly string Name = name;
	}
}
