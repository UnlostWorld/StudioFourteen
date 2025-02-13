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

namespace StudioFourteen.Services;

using Dalamud.Hooking;
using StudioFourteen.Plugin;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.Linq;
using TerraFX.Interop.Windows;

public class InteropService : ServiceBase
{
	private static readonly List<HookReference> Hooks = new();

	public static Hook<TDelegate>? HookFromSignature<TDelegate>(string sig, TDelegate detour, bool isLongLived = false)
		where TDelegate : Delegate
	{
		if (DalamudServices.SigScanner == null)
			return null;

		string name = typeof(TDelegate).Name;

		try
		{
			nint address = DalamudServices.SigScanner.ScanText(sig);
			return HookFromAddress<TDelegate>(address, detour, isLongLived);
		}
		catch (Exception ex)
		{
			Logging.ForContext<DalamudServices>().Error(ex, $"Error creating hook {name} from signature");
			return null;
		}
	}

	public static Hook<TDelegate>? HookFromAddress<TDelegate>(nint address, TDelegate detour, bool isLongLived = false)
			where TDelegate : Delegate
	{
		if (DalamudServices.InteropProvider == null)
			return null;

		string name = typeof(TDelegate).Name;

		try
		{
			Logging.Shared.Information($"Created Hook {name} for address {address}");

			Hook<TDelegate> hook = DalamudServices.InteropProvider.HookFromAddress<TDelegate>(address, detour);
			Hooks.Add(new(hook, name, isLongLived));
			return hook;
		}
		catch (Exception ex)
		{
			Logging.ForContext<DalamudServices>().Error(ex, $"Error creating hook {name} from address");
			return null;
		}
	}

	public static Hook<TDelegate>? HookFromImport<TDelegate>(ProcessModule? module, string moduleName, string functionName, uint hintOrOrdinal, TDelegate detour, bool isLongLived = false)
		where TDelegate : Delegate
	{
		if (DalamudServices.InteropProvider == null)
			return null;

		string name = typeof(TDelegate).Name;

		try
		{
			Logging.Shared.Information($"Created Hook {name} for import {functionName}");

			Hook<TDelegate> hook = DalamudServices.InteropProvider.HookFromImport<TDelegate>(module, moduleName, functionName, hintOrOrdinal, detour);
			Hooks.Add(new(hook, name, isLongLived));
			return hook;
		}
		catch (Exception ex)
		{
			Logging.ForContext<DalamudServices>().Error(ex, $"Error creating hook {name} from import");
			return null;
		}
	}

	public static void CheckHooks(bool includeLongLived)
	{
		foreach (HookReference reference in Hooks)
		{
			if (reference.IsLongLived && !includeLongLived)
				continue;

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

	private class HookReference(IDalamudHook hook, string name, bool longLived)
	{
		public readonly IDalamudHook Hook = hook;
		public readonly string Name = name;
		public readonly bool IsLongLived = longLived;
	}
}
