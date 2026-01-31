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

using global::Avalonia.Input;
using global::Dalamud.Game.ClientState.Keys;
using StudioFourteen.Services.Input;

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

	private Bind? HandleKey(VirtualKey key)
	{
		if (this.FocusedElement == null)
			return null;

		return this.bind;
	}

	private void OnUiTick()
	{
		/*ulong timeStamp = (ulong)DateTime.UtcNow.Ticks;
		RawKeyEventArgs args = new(this, timeStamp, null, RawKeyEventType.KeyDown, Key.A, RawInputModifiers.None, PhysicalKey.A, "a");
		this.ProcessRawEvent(args);*/
	}

	private class AvaloniaBind : Bind
	{
	}
}