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
using System.Windows;
using System.Windows.Input;

using Vector = System.Windows.Vector;

public class MouseDevice : InputDeviceBase
{
	private readonly Dictionary<MouseButton, Point> dragStarts = new();
	private readonly HashSet<MouseButton> draggingButtons = new();
	private readonly Dictionary<MouseButton, InputAxis> buttonAxes = new();
	private readonly Dictionary<MouseButton, (InputAxisSigned X, InputAxisSigned Y)> dragAxis = new();

	private readonly InputAxisSigned wheel;

	private Point lastMousePosition;

	public MouseDevice()
	{
		this.wheel = new(MouseDevice.WheelPos, MouseDevice.WheelNeg, this, true);
		this.AddAxis(this.wheel);

		foreach (MouseButton button in Enum.GetValues<MouseButton>())
		{
			this.buttonAxes.Add(button, new(MouseDevice.GetAxisId(button), this, true));

			InputAxisSigned x = new(
				GetDragAxisId(button, DragDirections.Left),
				GetDragAxisId(button, DragDirections.Right),
				this,
				false);

			InputAxisSigned y = new(
				GetDragAxisId(button, DragDirections.Up),
				GetDragAxisId(button, DragDirections.Down),
				this,
				false);

			this.dragAxis.Add(button, (x, y));
			this.AddAxis(x);
			this.AddAxis(y);
		}

		foreach ((MouseButton button, InputAxis axis) in this.buttonAxes)
		{
			this.AddAxis(axis);
		}
	}

	public enum DragDirections
	{
		Up,
		Down,
		Left,
		Right,
	}

	public static string WheelPos => "Mouse:Wheel+";
	public static string WheelNeg => "Mouse:Wheel-";

	public bool IsAnyDragging => this.draggingButtons.Count > 0;

	public static string GetAxisId(MouseButton button) => $"Mouse:{button}";
	public static string GetDragAxisId(MouseButton button, DragDirections direction) => $"Mouse:{button}Drag{direction}";

	public override void Attach()
	{
	}

	public override void Detach()
	{
	}

	public override void PreUpdate()
	{
		this.wheel.IsConsumed = false;

		foreach ((MouseButton button, InputAxis axis) in this.buttonAxes)
		{
			axis.IsConsumed = false;
		}

		foreach ((MouseButton button, (InputAxisSigned xAxis, InputAxisSigned yAxis)) in this.dragAxis)
		{
			xAxis.IsConsumed = false;
			yAxis.IsConsumed = false;
		}
	}

	public override void PostUpdate()
	{
		base.PostUpdate();

		this.wheel.Value = 0;

		foreach ((MouseButton button, (InputAxisSigned xAxis, InputAxisSigned yAxis)) in this.dragAxis)
		{
			xAxis.Value = 0;
			yAxis.Value = 0;
		}
	}

	public void HandleMouse(MouseButton button, bool down)
	{
		Point? mousePoint = this.Services.Windows.GetCursorPosition();

		if (mousePoint == null)
			return;

		this.buttonAxes[button].Value = down ? 1.0f : 0.0f;

		if (down)
		{
			this.dragAxis[button].X.Value = 0;
			this.dragAxis[button].Y.Value = 0;
			this.dragStarts[button] = mousePoint.Value;
		}
		else
		{
			this.draggingButtons.Remove(button);
			this.dragStarts.Remove(button);
			CursorUtility.SetCursorVisible(true);
		}
	}

	public void HandleMouseMove()
	{
		Point? mousePoint = this.Services.Windows.GetCursorPosition();
		if (mousePoint == null)
			return;

		foreach ((MouseButton button, Point dragStart) in this.dragStarts)
		{
			if (this.draggingButtons.Contains(button))
				continue;

			Vector totalDelta = mousePoint.Value - dragStart;
			if (Math.Abs(totalDelta.X) > 5.0f || Math.Abs(totalDelta.Y) > 5.0f)
			{
				this.draggingButtons.Add(button);
			}
		}

		foreach (MouseButton button in this.draggingButtons)
		{
			Vector delta = mousePoint.Value - this.lastMousePosition;
			this.dragAxis[button].X.Value += (float)delta.X / 8; // Sensitivity
			this.dragAxis[button].Y.Value += (float)delta.Y / 8;
		}

		if (this.IsAnyDragging)
		{
			foreach ((MouseButton button, Point dragStart) in this.dragStarts)
			{
				this.Services.Windows.SetCursorPosition(new((int)dragStart.X, (int)dragStart.Y));
			}

			CursorUtility.SetCursorVisible(false);
		}

		mousePoint = this.Services.Windows.GetCursorPosition();
		if (mousePoint == null)
			return;

		this.lastMousePosition = mousePoint.Value;
	}

	public void HandleMouseLeave()
	{
		foreach ((MouseButton button, (InputAxisSigned xAxis, InputAxisSigned yAxis)) in this.dragAxis)
		{
			xAxis.Value = 0;
			yAxis.Value = 0;
		}

		foreach ((MouseButton button, InputAxis axis) in this.buttonAxes)
		{
			axis.Value = 0.0f;
		}

		foreach (MouseButton button in this.draggingButtons)
		{
			this.dragAxis[button].X.Value = 0;
			this.dragAxis[button].Y.Value = 0;
		}

		CursorUtility.SetCursorVisible(true);
	}

	public void HandleMouseWheel(float delta)
	{
		this.wheel.Value += delta;
	}
}
