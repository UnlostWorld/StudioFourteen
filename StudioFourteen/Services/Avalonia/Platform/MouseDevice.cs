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

namespace StudioFourteen.Services.Avalonia.Platform;

using System;
using global::Avalonia;
using global::Avalonia.Controls;
using global::Avalonia.Input;

public class WindowsMouseDevice : MouseDevice
{
	private readonly IPointer pointer;

	public WindowsMouseDevice()
		: base(WindowsMousePointer.CreatePointer(out var pointer))
	{
		this.pointer = pointer;
	}

	// Normally user should use IPointer.Capture instead of MouseDevice.Capture,
	// But on Windows we need to handle WM_MOUSE capture manually without having access to the Pointer.
	internal void Capture(IInputElement? control)
	{
		this.pointer.Capture(control);
	}

	internal class WindowsMousePointer : Pointer
	{
		private WindowsMousePointer()
			: base(GetNextFreeId(), PointerType.Mouse, true)
		{
		}

		public static WindowsMousePointer CreatePointer(out WindowsMousePointer pointer)
		{
			return pointer = new WindowsMousePointer();
		}

		protected override void PlatformCapture(IInputElement? element)
		{
			throw new NotSupportedException();
		}
	}
}