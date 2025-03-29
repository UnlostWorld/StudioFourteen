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

using FFXIVClientStructs.FFXIV.Client.System.Input;
using StudioFourteen.Interop;
using System;
using System.Collections.Generic;
using System.Reflection;

using ClientButtons = FFXIVClientStructs.FFXIV.Client.System.Input.GamepadButtonsFlags;

public class GamepadDevice : InputDeviceBase
{
	private readonly Dictionary<Buttons, InputAxis> buttonAxes = new();
	private readonly Queue<Buttons> sendButtons = new();

	public GamepadDevice()
	{
		foreach (Buttons button in Enum.GetValues<Buttons>())
		{
			InputAxis axis = new(GetAxisId(button), this, true);
			this.buttonAxes.Add(button, axis);
			this.Axes.Add(axis);
		}
	}

	public enum Buttons
	{
		None = ClientButtons.None,
		DpadUp = ClientButtons.DPadUp,
		DpadDown = ClientButtons.DPadDown,
		DpadLeft = ClientButtons.DPadLeft,
		DpadRight = ClientButtons.DPadRight,
		FaceUp = ClientButtons.Triangle,
		FaceDown = ClientButtons.Cross,
		FaceLeft = ClientButtons.Square,
		FaceRight = ClientButtons.Circle,
		LeftShoulder = ClientButtons.L1,
		LeftTrigger = ClientButtons.L2,
		LeftStick = ClientButtons.L3,
		RightShoulder = ClientButtons.R1,
		RightTrigger = ClientButtons.R2,
		RightStick = ClientButtons.R3,
		Start = ClientButtons.Start,
		Select = ClientButtons.Select,
	}

	public static string GetAxisId(Buttons button) => $"Gamepad:{button}";

	public void SendButton(Buttons button)
	{
		this.sendButtons.Enqueue(button);
	}

	public unsafe override void Attach()
	{
		Hooks.PadDevicePoll.Enable(this.GamepadPollDetour);
	}

	public override void Detach()
	{
		Hooks.PadDevicePoll.Disable();
	}

	public override void Activate()
	{
		Type type = typeof(System.Windows.Input.KeyboardNavigation);
		PropertyInfo? showFocusVisual = type.GetProperty("AlwaysShowFocusVisual", BindingFlags.NonPublic | BindingFlags.Static);
		showFocusVisual?.SetValue(null, true);

		base.Activate();
	}

	public override void Deactivate()
	{
		Type type = typeof(System.Windows.Input.KeyboardNavigation);
		PropertyInfo? showFocusVisual = type.GetProperty("AlwaysShowFocusVisual", BindingFlags.NonPublic | BindingFlags.Static);
		showFocusVisual?.SetValue(null, false);

		base.Deactivate();
	}

	private unsafe nint GamepadPollDetour(PadDevice* pPadDevice)
	{
		nint ret = Hooks.PadDevicePoll.Original(pPadDevice);

		Buttons buttonValues = (Buttons)pPadDevice->GamepadInputData.Buttons;

		foreach ((Buttons button, InputAxis axis) in this.buttonAxes)
		{
			bool value = buttonValues.HasFlag(button);

			// first press, pre-consume
			bool pressed = value && axis.Value < 0.001f;

			// Was this axis consumed in the last tick, or is this a fresh
			// input?
			if (pressed || axis.IsConsumed)
			{
				pPadDevice->GamepadInputData.Buttons &= (ClientButtons)~button;
				pPadDevice->GamepadInputData.ButtonsPressed &= (ClientButtons)~button;
				pPadDevice->GamepadInputData.ButtonsReleased &= (ClientButtons)~button;
				pPadDevice->GamepadInputData.ButtonsRepeat &= (ClientButtons)~button;
			}

			axis.Value = value ? 1 : 0;
			axis.ConsumedBy = null;
		}

		while (this.sendButtons.Count > 0)
		{
			Buttons button = this.sendButtons.Dequeue();

			pPadDevice->GamepadInputData.Buttons |= (ClientButtons)button;
			pPadDevice->GamepadInputData.ButtonsPressed |= (ClientButtons)button;
		}

		return ret;
	}
}
