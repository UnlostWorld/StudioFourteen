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

namespace StudioFourteen.Services.Input.Devices;

using System;
using System.Collections.Generic;
using System.Numerics;
using FFXIVClientStructs.FFXIV.Client.System.Input;

public enum MouseButtons
{
	Left = MouseButtonFlags.LBUTTON,
	Middle = MouseButtonFlags.MBUTTON,
	Right = MouseButtonFlags.RBUTTON,
	XButton1 = MouseButtonFlags.XBUTTON1,
	XButton2 = MouseButtonFlags.XBUTTON2,
}

#pragma warning disable

public class MouseDevice : InputDeviceBase
{
	private const float MinDragDistance = 1.0f;

	private readonly Dictionary<MouseButtons, Vector2> dragStarts = new();
	private readonly HashSet<MouseButtons> draggingButtons = new();
	private readonly Dictionary<MouseButtons, InputAxis> buttonAxes = new();
	private readonly Dictionary<MouseButtons, (InputAxisSigned X, InputAxisSigned Y)> dragAxis = new();

	private readonly InputAxis positionX;
	private readonly InputAxis positionY;

	private readonly InputAxisSigned wheel;

	private Vector2 lastMousePosition;

	public MouseDevice()
	{
		this.wheel = new(MouseDevice.WheelPos, MouseDevice.WheelNeg, this, true);
		this.AddAxis(this.wheel);

		this.positionX = new(MouseDevice.PositionX, this, false);
		this.AddAxis(this.positionX);

		this.positionY = new(MouseDevice.PositionY, this, false);
		this.AddAxis(this.positionY);

		foreach (MouseButtons button in Enum.GetValues<MouseButtons>())
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

		foreach ((MouseButtons button, InputAxis axis) in this.buttonAxes)
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
	public static string PositionX => $"Mouse:Position:X";
	public static string PositionY => $"Mouse:Position:Y";

	public bool IsAnyDragging => this.draggingButtons.Count > 0;

	public static string GetAxisId(MouseButtons button) => $"Mouse:{button}";
	public static string GetDragAxisId(MouseButtons button, DragDirections direction) => $"Mouse:{button}Drag:{direction}";

	public Vector2 GetPosition() => new(this.positionX.Value, this.positionY.Value);
	public bool GetButton(MouseButtons button) => this.buttonAxes[button].Value > 0.05f;

	public void SetCursorVisible(bool visible)
	{
	}

	public override void Attach()
	{
		base.Attach();

		this.buttonAxes[MouseButtons.Left].UtcLastInput = DateTime.UtcNow;
	}

	public unsafe override void PreUpdate()
	{
		this.wheel.ConsumedBy = null;

		foreach ((MouseButtons button, InputAxis axis) in this.buttonAxes)
		{
			axis.ConsumedBy = null;
		}

		foreach ((MouseButtons button, (InputAxisSigned xAxis, InputAxisSigned yAxis)) in this.dragAxis)
		{
			xAxis.ConsumedBy = null;
			yAxis.ConsumedBy = null;
		}
	}

	public unsafe override void PostUpdate()
	{
		base.PostUpdate();

		this.wheel.Value = 0;

		foreach ((MouseButtons button, (InputAxisSigned xAxis, InputAxisSigned yAxis)) in this.dragAxis)
		{
			xAxis.Value = 0;
			yAxis.Value = 0;
		}

		/*foreach ((MouseButton button, InputAxis axis) in this.buttonAxes)
		{
			axis.Value = 0;
		}*/
	}

	public bool HandleMouseMove(Vector2 position)
	{
		foreach ((MouseButtons button, Vector2 dragStart) in this.dragStarts)
		{
			if (this.draggingButtons.Contains(button))
				continue;

			Vector2 totalDelta = position - dragStart;
			if (Math.Abs(totalDelta.X) > MinDragDistance || Math.Abs(totalDelta.Y) > MinDragDistance)
			{
				this.draggingButtons.Add(button);
			}
		}

		foreach (MouseButtons button in this.draggingButtons)
		{
			Vector2 delta = position - this.lastMousePosition;
			this.dragAxis[button].X.Value += (float)delta.X / 8; // Sensitivity
			this.dragAxis[button].Y.Value += (float)delta.Y / 8;
		}

		if (this.IsAnyDragging)
		{
			foreach ((MouseButtons button, Vector2 dragStart) in this.dragStarts)
			{
				// 🤔
				////this.SetPosition(dragStart);
			}

			this.SetCursorVisible(false);
		}
		else
		{
			this.positionX.Value = position.X;
			this.positionY.Value = position.Y;
		}

		this.lastMousePosition = position;

		return this.ShouldConsumeMouse();
	}

	public bool HandleMouseButton(MouseButtons button, bool down)
	{
		if (!down)
		{
			this.draggingButtons.Remove(button);
			this.dragStarts.Remove(button);
			this.SetCursorVisible(true);
			this.buttonAxes[button].Value = 0.0f;
		}

		Vector2? mousePoint = this.GetPosition();
		if (mousePoint == null)
			return false;

		if (down)
		{
			this.dragAxis[button].X.Value = 0;
			this.dragAxis[button].Y.Value = 0;
			this.dragStarts[button] = mousePoint.Value;
			this.buttonAxes[button].Value = 1.0f;
		}

		return this.ShouldConsumeMouse();
	}

	public bool HandleMouseWheel(float delta)
	{
		if (!this.ShouldConsumeMouse())
			return false;

		this.wheel.Value += delta;
		return true;
	}

	// TODO: Replace this with WindowService SetCapture / ReleaseCapture or TrackMouseEvent
	public void HandleMouseLeave()
	{
		foreach ((MouseButtons button, (InputAxisSigned xAxis, InputAxisSigned yAxis)) in this.dragAxis)
		{
			xAxis.Value = 0;
			yAxis.Value = 0;
		}

		foreach ((MouseButtons button, InputAxis axis) in this.buttonAxes)
		{
			axis.Value = 0.0f;
		}

		foreach (MouseButtons button in this.draggingButtons)
		{
			this.dragAxis[button].X.Value = 0;
			this.dragAxis[button].Y.Value = 0;
		}

		this.draggingButtons.Clear();
		this.dragStarts.Clear();

		this.SetCursorVisible(true);
	}

	private bool ShouldConsumeMouse()
	{
		// If the user has disabled the overlay system globally,
		// never capture mouse inputs.
		////if (!Studio.Settings.Current.AllowMouseCapture)
		////	return false;

		// Capture the mouse if a studio window or gizmo handle is under
		// ths cursor.
		////if (RendererInput.IsCursorOverHandle)
		////	return true;

		// If the reshade overlay is open, let it do its cursor things.
		////if (Studio.Reshade.IsReshadeOverlayOpen)
		////	return false;

		// TODO: Needs to be true when the cursor is over a Studio window or handle.
		return false;
	}
}
