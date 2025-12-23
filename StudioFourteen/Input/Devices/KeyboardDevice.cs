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

namespace StudioFourteen.Input.Devices;

using Dalamud.Game.ClientState.Keys;
using StudioFourteen.Plugin;
using System.Collections.Generic;

public class KeyboardDevice : InputDeviceBase
{
	private readonly Dictionary<VirtualKey, InputAxis> axisLookup = new();
	private readonly HashSet<VirtualKey> keysSentToXiv = new();

	public KeyboardDevice()
	{
		if (DalamudServices.KeyState == null)
			return;

		foreach (VirtualKey key in DalamudServices.KeyState.GetValidVirtualKeys())
		{
			if (this.axisLookup.ContainsKey(key))
				continue;

			InputAxis axis = new(KeyboardDevice.GetAxisId(key), this, true);
			this.axisLookup.Add(key, axis);
			this.AddAxis(axis);
		}
	}

	public static object? FocusedElement => null;

	public static string GetAxisId(VirtualKey key) => $"Keyboard:{key}";

	public override void PreUpdate()
	{
		if (DalamudServices.KeyState == null)
			return;

		if (this.Services.Input.IsXivTextInputActive)
			return;

		foreach (InputAxis axis in this.Axes)
		{
			axis.ConsumedBy = null;
		}

		// Read XIV key states into Studio
		/*foreach ((VirtualKey key, InputAxis axis) in this.axisLookup)
		{
			bool isDown = DalamudServices.KeyState[key];
			axis.Value = isDown ? 1.0f : 0.0f;
		}*/
	}

	public override void PostUpdate()
	{
		if (DalamudServices.KeyState == null)
			return;

		if (this.Services.Input.IsXivTextInputActive)
			return;

		// block XIV keys that Studio consumed
		foreach ((VirtualKey key, InputAxis axis) in this.axisLookup)
		{
			if (axis.IsConsumed)
			{
				DalamudServices.KeyState[key] = false;
			}
		}
	}

	public IEnumerable<VirtualKey> GetValidKeys()
	{
		if (DalamudServices.KeyState == null)
			return new List<VirtualKey>();

		return DalamudServices.KeyState.GetValidVirtualKeys();
	}

	public bool HandleChar(uint charId)
	{
		return false;
	}

	public bool HandleKey(int keyId, bool down)
	{
		VirtualKey vKey = (VirtualKey)keyId;
		if (vKey == VirtualKey.NO_KEY)
			return false;

		return this.HandleKey(vKey, down);
	}

	public bool HandleKey(VirtualKey vKey, bool down)
	{
		if (DalamudServices.KeyState == null)
			return false;

		if (vKey == VirtualKey.LSHIFT || vKey == VirtualKey.RSHIFT)
			vKey = VirtualKey.SHIFT;

		if (vKey == VirtualKey.LMENU || vKey == VirtualKey.RMENU)
			vKey = VirtualKey.MENU;

		if (vKey == VirtualKey.LCONTROL || vKey == VirtualKey.RCONTROL)
			vKey = VirtualKey.CONTROL;

		if (!DalamudServices.KeyState.IsVirtualKeyValid(vKey))
			return false;

		if (!this.axisLookup.ContainsKey(vKey))
			return false;

		this.axisLookup[vKey].Value = down ? 1.0f : 0.0f;

		return false;
	}
}