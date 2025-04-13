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

using System;
using System.Windows;
using FFXIVClientStructs.FFXIV.Client.UI;
using StudioFourteen.Interop;

public class CursorService : ServiceBase
{
	public unsafe override void Attach()
	{
		base.Attach();

		Hooks.UpdateGameCursor.Enable(this.UpdateCursorDetour);
		Hooks.SetCursor.Enable(this.SetCursorDetour);
	}

	public override void Detach()
	{
		base.Detach();
		Hooks.UpdateGameCursor.Disable();
		Hooks.SetCursor.Disable();
	}

	private unsafe nint UpdateCursorDetour(RaptureAtkModule* module)
    {
		return Hooks.UpdateGameCursor.Original(module);
    }

	private IntPtr SetCursorDetour(IntPtr hCursor)
	{
		if (this.Services.Windows.IsMouseOverWindow() || this.Services.DragAndDrop.IsDragging)
			return IntPtr.Zero;

		return Hooks.SetCursor.Original(hCursor);
	}
}