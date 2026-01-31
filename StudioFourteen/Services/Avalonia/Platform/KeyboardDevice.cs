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

using global::Avalonia.Win32.Input;
using global::Avalonia.Input;
using global::Dalamud.Game.ClientState.Keys;
using StudioFourteen.Services.Input;
using System;
using global::Avalonia.Input.Raw;
using global::Avalonia;
using System.Reflection;
using global::Avalonia.Controls;

public class StudioKeyboardDevice : KeyboardDevice
{
	private readonly AvaloniaBind bind = new();

	public StudioKeyboardDevice()
	{
		Studio.Input.Keyboard?.ConsumeKey = this.HandleKey;
	}

	public void DisposeKeyboard()
	{
		Studio.Input.Keyboard?.ConsumeKey = null;
	}

	private Bind? HandleKey(VirtualKey virtualKey, bool isDown, long keyData)
	{
		if (this.FocusedElement == null)
			return null;

		IInputRoot? root = this.Field<IInputRoot?>("_focusedRoot");
		if (root == null)
			return null;

		WindowImpl? windowImplementation = null;
		if (root is Window wnd && wnd.PlatformImpl is WindowImpl impl)
			windowImplementation = impl;

		PhysicalKey physicalKey = KeyInterop.PhysicalKeyFromVirtualKey((int)virtualKey, (int)keyData);
		Key key = KeyInterop.KeyFromVirtualKey((int)virtualKey, (int)keyData);
		string? symbol = KeyInterop.GetKeySymbol((int)virtualKey, (int)keyData);

		ulong timeStamp = (ulong)DateTime.UtcNow.Ticks;
		RawKeyEventArgs args = new(
			this,
			timeStamp,
			root,
			isDown ? RawKeyEventType.KeyDown : RawKeyEventType.KeyUp,
			key,
			RawInputModifiers.None,
			physicalKey,
			symbol);

		windowImplementation?.HandleInput(args);

		if (symbol != null && isDown)
		{
			if (key == Key.Space
			|| ((int)key >= 33 && (int)key <= 69)
			|| ((int)key >= 74 && (int)key <= 89)
			|| ((int)key >= 140 && (int)key <= 154))
			{
				RawTextInputEventArgs textArgs = new(this, timeStamp, root, symbol);
				windowImplementation?.HandleInput(textArgs);
			}
		}

		return this.bind;
	}

	private class AvaloniaBind : Bind
	{
	}
}