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
namespace StudioFourteen.Input.Devices;

using Dalamud.Game.ClientState.GamePad;
using Dalamud.Hooking;
using StudioFourteen.Services;
using System;

public class GamepadDevice : InputDeviceBase
{
	private Hook<ControllerPoll>? gamepadPoll;
	private ushort buttons;

	public GamepadDevice()
	{
		this.Axes.Add(new(DpadUp));
		this.Axes.Add(new(DpadDown));
		this.Axes.Add(new(DpadLeft));
		this.Axes.Add(new(DpadRight));
		this.Axes.Add(new(FaceUp));
		this.Axes.Add(new(FaceDown));
		this.Axes.Add(new(FaceLeft));
		this.Axes.Add(new(FaceRight));
		this.Axes.Add(new(LeftShoulder));
		this.Axes.Add(new(LeftTrigger));
		this.Axes.Add(new(LeftStick));
		this.Axes.Add(new(RightShoulder));
		this.Axes.Add(new(RightTrigger));
		this.Axes.Add(new(RightStick));
		this.Axes.Add(new(Start));
		this.Axes.Add(new(Select));
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

	public static string DpadUp => "Gamepad:DpadUp";
	public static string DpadDown => "Gamepad:DpadDown";
	public static string DpadLeft => "Gamepad:DpadLeft";
	public static string DpadRight => "Gamepad:DpadRight";
	public static string FaceUp => "Gamepad:FaceUp";
	public static string FaceDown => "Gamepad:FaceDown";
	public static string FaceLeft => "Gamepad:FaceLeft";
	public static string FaceRight => "Gamepad:FaceRight";
	public static string LeftShoulder => "Gamepad:LeftShoulder";
	public static string LeftTrigger => "Gamepad:LeftTrigger";
	public static string LeftStick => "Gamepad:LeftStick";
	public static string RightShoulder => "Gamepad:RightShoulder";
	public static string RightTrigger => "Gamepad:RightTrigger";
	public static string RightStick => "Gamepad:RightStick";
	public static string Start => "Gamepad:Start";
	public static string Select => "Gamepad:Select";

	public override void Attach()
	{
		this.gamepadPoll = InteropService.HookFromSignature<ControllerPoll>("40 55 53 57 41 54 41 57 48 8D AC 24 ?? ?? ?? ?? 48 81 EC ?? ?? ?? ?? 44 0F 29 B4 24", this.GamepadPollDetour);
		this.gamepadPoll?.Enable();
	}

	public override void Detach()
	{
		this.gamepadPoll?.Dispose();
	}

	public unsafe override void PreUpdate()
	{
	}

	private unsafe int GamepadPollDetour(IntPtr gamepadInput)
	{
		if (this.gamepadPoll == null)
			throw new InvalidOperationException();

		int ret = this.gamepadPoll.Original(gamepadInput);

		GamepadInput* input = (GamepadInput*)gamepadInput;
		this.buttons = input->ButtonsRaw;

		input->ButtonsRaw = 0;
		input->ButtonsPressed = 0;
		input->ButtonsReleased = 0;
		input->ButtonsRepeat = 0;

		return ret;
	}
}
