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

using StudioFourteen.Utilities;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Windows.Input;

public class MouseDevice : InputDeviceBase
{
	private readonly Dictionary<MouseButton, InputAxis> buttonAxes = new();

	private readonly InputAxisSigned wheel = new(MouseDevice.WheelPos, MouseDevice.WheelNeg);
	private readonly InputAxisSigned x = new(MouseDevice.MoveRight, MouseDevice.MoveLeft);
	private readonly InputAxisSigned y = new(MouseDevice.MoveUp, MouseDevice.MoveDown);

	private Vector2 lastMousePosition = Vector2.Zero;

	public MouseDevice()
	{
		this.AddAxis(this.wheel);
		this.AddAxis(this.x);
		this.AddAxis(this.y);

		foreach(MouseButton button in Enum.GetValues<MouseButton>())
		{
			this.buttonAxes.Add(button, new(MouseDevice.GetAxisId(button)));
		}

		foreach((MouseButton button, InputAxis axis) in this.buttonAxes)
		{
			this.Axes.Add(axis);
		}
	}

	public static string WheelPos => "Mouse:Wheel+";
	public static string WheelNeg => "Mouse:Wheel-";
	public static string MoveRight => "Mouse:MoveRight";
	public static string MoveLeft => "Mouse:MoveLeft";
	public static string MoveUp => "Mouse:MoveUp";
	public static string MoveDown => "Mouse:MoveDown";

	public bool IsMouseDragging { get; private set; }

	public static string GetAxisId(MouseButton button) => $"Mouse:{button}";

	public override void Attach()
	{
	}

	public override void Detach()
	{
	}

	public override void PreUpdate()
	{
		this.wheel.IsConsumed = false;
		this.x.IsConsumed = false;
		this.y.IsConsumed = false;

		foreach ((MouseButton button, InputAxis axis) in this.buttonAxes)
		{
			axis.IsConsumed = false;
		}
	}

	public override void PostUpdate()
	{
		base.PostUpdate();

		this.wheel.Value = 0;
		this.x.Value = 0;
		this.y.Value = 0;
	}

	public void HandleMouse(MouseButtonEventArgs e, bool down)
	{
		this.buttonAxes[e.ChangedButton].Value = down ? 1.0f : 0.0f;

		if (!down)
		{
			CursorUtility.SetCursorVisible(true);
		}
	}

	public void HandleMouseMove(Vector2 newPos)
	{
		Vector2 delta = newPos - this.lastMousePosition;

		this.x.Value += delta.X;
		this.y.Value += delta.Y;

		bool holdPosition = false;
		foreach ((MouseButton button, InputAxis axis) in this.buttonAxes)
		{
			if (axis.Value > 0.5f)
			{
				holdPosition = true;
				this.Services.Windows.SetCursorPosition(new((int)this.lastMousePosition.X, (int)this.lastMousePosition.Y));
				this.IsMouseDragging = true;
			}
		}

		if (holdPosition)
		{
			CursorUtility.SetCursorVisible(false);
		}
		else
		{
			this.lastMousePosition = newPos;
		}
	}

	public void HandleMouseLeave()
	{
		foreach ((MouseButton button, InputAxis axis) in this.buttonAxes)
		{
			axis.Value = 0.0f;
		}

		CursorUtility.SetCursorVisible(true);
	}

	public void HandleMouseWheel(float delta)
	{
		this.wheel.Value += delta;
	}
}
