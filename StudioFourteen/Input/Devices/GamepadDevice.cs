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
	private readonly Dictionary<Sticks, (InputAxisSigned X, InputAxisSigned Y)> stickAxes = new();

	public GamepadDevice()
	{
		foreach (Buttons button in Enum.GetValues<Buttons>())
		{
			InputAxis axis = new(GetAxisId(button), this, true);
			this.buttonAxes.Add(button, axis);
			this.Axes.Add(axis);
		}

		foreach (Sticks stick in Enum.GetValues<Sticks>())
		{
			InputAxisSigned xAxis = new(
				GetStickAxis(stick, StickDirections.Right),
				GetStickAxis(stick, StickDirections.Left),
				this,
				false);

			InputAxisSigned yAxis = new(
				GetStickAxis(stick, StickDirections.Up),
				GetStickAxis(stick, StickDirections.Down),
				this,
				false);

			this.Axes.Add(xAxis.Positive);
			this.Axes.Add(xAxis.Negative);
			this.Axes.Add(yAxis.Positive);
			this.Axes.Add(yAxis.Negative);
			this.stickAxes.Add(stick, (xAxis, yAxis));
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

	public enum Sticks
	{
		LeftStick,
		RightStick,
	}

	public enum StickDirections
	{
		Up,
		Down,
		Left,
		Right,
	}

	public static string GetAxisId(Buttons button) => $"Gamepad:{button}";
	public static string GetStickAxis(Sticks stick, StickDirections direction) => $"Gamepad:{stick}:{direction}";

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
		base.Activate();
	}

	public override void Deactivate()
	{
		base.Deactivate();
	}

	private unsafe nint GamepadPollDetour(PadDevice* pPadDevice)
	{
		nint ret = Hooks.PadDevicePoll.Original(pPadDevice);

		// TODO: Consume stick inputs.
		this.stickAxes[Sticks.LeftStick].X.Value = pPadDevice->GamepadInputData.LeftStickX / 100.0f;
		this.stickAxes[Sticks.LeftStick].Y.Value = pPadDevice->GamepadInputData.LeftStickY / 100.0f;
		this.stickAxes[Sticks.RightStick].X.Value = pPadDevice->GamepadInputData.RightStickX / 100.0f;
		this.stickAxes[Sticks.RightStick].Y.Value = pPadDevice->GamepadInputData.RightStickY / 100.0f;

		if (this.stickAxes[Sticks.LeftStick].X.ConsumedBy != null)
		{
			pPadDevice->GamepadInputData.LeftStickX = 0;
			this.stickAxes[Sticks.LeftStick].X.ConsumedBy = null;
		}

		if (this.stickAxes[Sticks.LeftStick].Y.ConsumedBy != null)
		{
			pPadDevice->GamepadInputData.LeftStickY = 0;
			this.stickAxes[Sticks.LeftStick].Y.ConsumedBy = null;
		}

		if (this.stickAxes[Sticks.RightStick].X.ConsumedBy != null)
		{
			pPadDevice->GamepadInputData.RightStickX = 0;
			this.stickAxes[Sticks.RightStick].X.ConsumedBy = null;
		}

		if (this.stickAxes[Sticks.RightStick].Y.ConsumedBy != null)
		{
			pPadDevice->GamepadInputData.RightStickY = 0;
			this.stickAxes[Sticks.RightStick].Y.ConsumedBy = null;
		}

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
