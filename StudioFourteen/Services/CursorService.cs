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

using System;
using System.Runtime.InteropServices;
using System.Windows.Input;
using Dalamud.Hooking;
using FFXIVClientStructs.FFXIV.Client.UI;
using StudioFourteen.Services;

public class CursorService : ServiceBase
{
	private Hook<UpdateGameCursorDelegate>? updateCursorHook;
	private Hook<SetUser32CursorDelegate>? setCursorHook;

	private unsafe delegate nint UpdateGameCursorDelegate(RaptureAtkModule* module);

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	private delegate IntPtr SetUser32CursorDelegate(IntPtr hCursor);

	public unsafe override void Attach()
	{
		base.Attach();

		// https://github.com/goatcorp/Dalamud/blob/master/Dalamud/Game/Addon/Events/AddonEventManagerAddressResolver.cs
		this.updateCursorHook = InteropService.HookFromSignature<UpdateGameCursorDelegate>("48 89 74 24 ?? 48 89 7C 24 ?? 41 56 48 83 EC 20 4C 8B F1 E8 ?? ?? ?? ?? 49 8B CE", this.UpdateCursorDetour);
		this.updateCursorHook?.Enable();

		this.setCursorHook = InteropService.HookFromImport<SetUser32CursorDelegate>(null, "user32.dll", "SetCursor", 0, this.SetCursorDetour);
		this.setCursorHook?.Enable();
	}

	public override void Detach()
	{
		base.Detach();
		this.updateCursorHook?.Dispose();
		this.setCursorHook?.Dispose();
	}

	private unsafe nint UpdateCursorDetour(RaptureAtkModule* module)
    {
		if (this.updateCursorHook == null)
			return nint.Zero;

		return this.updateCursorHook.Original(module);
    }

	private IntPtr SetCursorDetour(IntPtr hCursor)
	{
		if (this.setCursorHook == null)
			return IntPtr.Zero;

		if (this.Services.Windows.IsMouseOverWindow())
			return IntPtr.Zero;

		return this.setCursorHook.Original(hCursor);
	}
}