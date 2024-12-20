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

// Dalamud
// https://github.com/goatcorp/Dalamud/blob/master/Dalamud/Game/ClientState/ClientStateAddressResolver.cs
// https://github.com/goatcorp/Dalamud/blob/master/Dalamud/Game/ClientState/GamePad/GamepadInput.cs
// https://github.com/goatcorp/Dalamud/blob/master/Dalamud/Game/ClientState/GamePad/GamepadState.cs
namespace StudioFourteen.Input.Devices;

using Dalamud.Game.ClientState.GamePad;
using Dalamud.Hooking;
using StudioFourteen.Services;
using System;
using System.Collections.Generic;
using System.Reflection;

public class GamepadDevice : InputDeviceBase
{
	private readonly Dictionary<Buttons, InputAxis> buttonAxes = new();
	private readonly Queue<Buttons> sendButtons = new();
	private Hook<ControllerPoll>? gamepadPoll;

	public GamepadDevice()
	{
		foreach(Buttons button in Enum.GetValues<Buttons>())
		{
			InputAxis axis = new(GetAxisId(button), this, true);
			this.buttonAxes.Add(button, axis);
			this.Axes.Add(axis);
		}
	}

	private delegate int ControllerPoll(IntPtr controllerInput);

	public enum Buttons
	{
		None = 0,
		DpadUp = 1,
		DpadDown = 2,
		DpadLeft = 4,
		DpadRight = 8,
		FaceUp = 0x10,
		FaceDown = 0x20,
		FaceLeft = 0x40,
		FaceRight = 0x80,
		LeftShoulder = 0x100,
		LeftTrigger = 0x200,
		LeftStick = 0x400,
		RightShoulder = 0x800,
		RightTrigger = 0x1000,
		RightStick = 0x2000,
		Start = 0x8000,
		Select = 0x4000,
	}

	public static string GetAxisId(Buttons button) => $"Gamepad:{button}";

	public void SendButton(Buttons button)
	{
		this.sendButtons.Enqueue(button);
	}

	public override void Attach()
	{
		this.gamepadPoll = InteropService.HookFromSignature<ControllerPoll>("40 55 53 57 41 54 41 57 48 8D AC 24 ?? ?? ?? ?? 48 81 EC ?? ?? ?? ?? 44 0F 29 B4 24", this.GamepadPollDetour);
		this.gamepadPoll?.Enable();
	}

	public override void Detach()
	{
		this.gamepadPoll?.Dispose();
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

	private unsafe int GamepadPollDetour(IntPtr gamepadInput)
	{
		if (this.gamepadPoll == null)
			throw new InvalidOperationException();

		int ret = this.gamepadPoll.Original(gamepadInput);

		GamepadInput* input = (GamepadInput*)gamepadInput;
		ushort buttonValues = input->ButtonsRaw;

		foreach ((Buttons button, InputAxis axis) in this.buttonAxes)
		{
			bool value = (buttonValues & (ushort)button) > 0;

			// first press, pre-consume
			bool pressed = value && axis.Value < 0.001f;

			// Was this axis consumed in the last tick, or is this a fresh
			// input?
			if (pressed || axis.IsConsumed)
			{
				input->ButtonsRaw &= (ushort)~button;
				input->ButtonsPressed &= (ushort)~button;
				input->ButtonsReleased &= (ushort)~button;
				input->ButtonsRepeat &= (ushort)~button;
			}

			axis.Value = value ? 1 : 0;
			axis.IsConsumed = false;
		}

		while(this.sendButtons.Count > 0)
		{
			Buttons button = this.sendButtons.Dequeue();

			input->ButtonsRaw |= (ushort)button;
			input->ButtonsPressed |= (ushort)button;
		}

		return ret;
	}
}
