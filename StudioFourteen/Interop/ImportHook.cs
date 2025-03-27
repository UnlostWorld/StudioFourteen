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

namespace StudioFourteen.Interop;

using Dalamud.Hooking;
using StudioFourteen.Plugin;
using System;
using System.Diagnostics;

public class ImportHook<TDelegate>(ProcessModule? module, string moduleName, string functionName, uint hintOrOrdinal)
	: HookBase<TDelegate>
	where TDelegate : Delegate
{
	protected override Hook<TDelegate>? Create(TDelegate detour)
	{
		if (DalamudServices.InteropProvider == null)
			return null;

		string name = typeof(TDelegate).Name;

		try
		{
			Logging.Shared.Information($"Creating Hook {name} for import {functionName} in {moduleName}");
			return DalamudServices.InteropProvider.HookFromImport(module, moduleName, functionName, hintOrOrdinal, detour);
		}
		catch (Exception ex)
		{
			Logging.ForContext<DalamudServices>().Error(ex, $"Error creating hook {name} from import");
			return null;
		}
	}
}
