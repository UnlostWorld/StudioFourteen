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

namespace StudioFourteen.Services.Interop;

using global::Dalamud.Hooking;
using System;

public class SignatureHook<TDelegate>(string signature)
	: HookBase<TDelegate>
	where TDelegate : Delegate
{
	protected override Hook<TDelegate>? Create(TDelegate detour)
	{
		if (Studio.SigScanner == null || Studio.InteropProvider == null)
			return null;

		string name = typeof(TDelegate).Name;

		try
		{
			Studio.Log.Information($"Creating Hook {name} for signature {signature}");
			nint address = Studio.SigScanner.ScanText(signature);
			return Studio.InteropProvider.HookFromAddress(address, detour);
		}
		catch (Exception ex)
		{
			Studio.Log.Error(ex, $"Error creating hook {name} from signature");
			return null;
		}
	}
}