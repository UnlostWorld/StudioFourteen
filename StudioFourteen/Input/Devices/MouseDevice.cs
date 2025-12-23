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

using System;
using System.Collections.Generic;
using System.Numerics;
using System.Windows;
using System.Windows.Input;
using FFXIVClientStructs.FFXIV.Client.System.Input;
using StudioFourteen.Rendering;
using StudioFourteen.Utilities;

public class MouseDevice : InputDeviceBase
{
	private const float MinDragDistance = 1.0f;

	private readonly Dictionary<MouseButtonFlags, Vector2> dragStarts = new();
	private readonly HashSet<MouseButtonFlags> draggingButtons = new();
	private readonly Dictionary<MouseButtonFlags, InputAxis> buttonAxes = new();
	private readonly Dictionary<MouseButtonFlags, (InputAxisSigned X, InputAxisSigned Y)> dragAxis = new();

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

		foreach (MouseButtonFlags button in Enum.GetValues<MouseButtonFlags>())
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

		foreach ((MouseButtonFlags button, InputAxis axis) in this.buttonAxes)
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

	public static string GetAxisId(MouseButtonFlags button) => $"Mouse:{button}";
	public static string GetDragAxisId(MouseButtonFlags button, DragDirections direction) => $"Mouse:{button}Drag:{direction}";

	public Vector2 GetPosition() => new(this.positionX.Value, this.positionY.Value);
	public bool GetButton(MouseButtonFlags button) => this.buttonAxes[button].Value > 0.05f;

	public override void Attach()
	{
		base.Attach();

		this.buttonAxes[MouseButtonFlags.LBUTTON].UtcLastInput = DateTime.UtcNow;
	}

	public unsafe override void PreUpdate()
	{
		this.wheel.ConsumedBy = null;

		foreach ((MouseButtonFlags button, InputAxis axis) in this.buttonAxes)
		{
			axis.ConsumedBy = null;
		}

		foreach ((MouseButtonFlags button, (InputAxisSigned xAxis, InputAxisSigned yAxis)) in this.dragAxis)
		{
			xAxis.ConsumedBy = null;
			yAxis.ConsumedBy = null;
		}

		if (!this.ShouldHandleMouse())
			return;

		this.UpdateMousePosition();
	}

	public unsafe override void PostUpdate()
	{
		base.PostUpdate();

		this.wheel.Value = 0;

		foreach ((MouseButtonFlags button, (InputAxisSigned xAxis, InputAxisSigned yAxis)) in this.dragAxis)
		{
			xAxis.Value = 0;
			yAxis.Value = 0;
		}

		/*foreach ((MouseButton button, InputAxis axis) in this.buttonAxes)
		{
			axis.Value = 0;
		}*/
	}

	public bool HandleMouseButton(MouseButtonFlags button, bool down)
	{
		if (!down)
		{
			/*if (this.dragStarts.ContainsKey(button) && !this.draggingButtons.Contains(button))
			{
				this.Log.Information($"Click {button}");
			}*/

			this.draggingButtons.Remove(button);
			this.dragStarts.Remove(button);
			CursorUtility.SetCursorVisible(true);
			this.buttonAxes[button].Value = 0.0f;
		}

		Vector2? mousePoint = CursorUtility.GetPosition();
		if (mousePoint == null)
			return false;

		if (down)
		{
			this.dragAxis[button].X.Value = 0;
			this.dragAxis[button].Y.Value = 0;
			this.dragStarts[button] = mousePoint.Value;
			this.buttonAxes[button].Value = 1.0f;
		}

		return this.ShouldHandleMouse();
	}

	public bool HandleMouseWheel(float delta)
	{
		if (!this.ShouldHandleMouse())
			return false;

		this.wheel.Value += delta;
		return true;
	}

	public void HandleMouseLeave()
	{
		foreach ((MouseButtonFlags button, (InputAxisSigned xAxis, InputAxisSigned yAxis)) in this.dragAxis)
		{
			xAxis.Value = 0;
			yAxis.Value = 0;
		}

		foreach ((MouseButtonFlags button, InputAxis axis) in this.buttonAxes)
		{
			axis.Value = 0.0f;
		}

		foreach (MouseButtonFlags button in this.draggingButtons)
		{
			this.dragAxis[button].X.Value = 0;
			this.dragAxis[button].Y.Value = 0;
		}

		this.draggingButtons.Clear();
		this.dragStarts.Clear();

		CursorUtility.SetCursorVisible(true);
	}

	private bool ShouldHandleMouse()
	{
		// If the user has disabled the overlay system globally,
		// never capture mouse inputs.
		if (!this.Services.Settings.Current.AllowMouseCapture)
			return false;

		// Capture the mouse if a studio window or gizmo handle is under
		// ths cursor.
		if (RendererInput.IsCursorOverHandle)
			return true;

		// If the reshade overlay is open, let it do its cursor things.
		if (this.Services.Reshade.IsReshadeOverlayOpen)
			return false;

		return true;
	}

	private void UpdateMousePosition()
	{
		Vector2? mousePoint = null; ////this.Services.Windows.GetCursorPosition();
		Vector2 clientSize = Vector2.Zero; ////this.Services.Windows.GetXivWindowClientSize();

		if (mousePoint == null)
		{
			this.HandleMouseLeave();
			return;
		}

		foreach ((MouseButtonFlags button, Vector2 dragStart) in this.dragStarts)
		{
			if (this.draggingButtons.Contains(button))
				continue;

			Vector2 totalDelta = mousePoint.Value - dragStart;
			if (Math.Abs(totalDelta.X) > MinDragDistance || Math.Abs(totalDelta.Y) > MinDragDistance)
			{
				this.draggingButtons.Add(button);
			}
		}

		foreach (MouseButtonFlags button in this.draggingButtons)
		{
			Vector2 delta = mousePoint.Value - this.lastMousePosition;
			this.dragAxis[button].X.Value += (float)delta.X / 8; // Sensitivity
			this.dragAxis[button].Y.Value += (float)delta.Y / 8;
		}

		if (this.IsAnyDragging)
		{
			foreach ((MouseButtonFlags button, Vector2 dragStart) in this.dragStarts)
			{
				CursorUtility.SetPosition(dragStart);
			}

			CursorUtility.SetCursorVisible(false);
		}
		else
		{
			this.positionX.Value = (float)(mousePoint.Value.X / clientSize.X);
			this.positionY.Value = (float)(mousePoint.Value.Y / clientSize.Y);
		}

		mousePoint = CursorUtility.GetPosition();
		if (mousePoint == null)
			return;

		this.lastMousePosition = mousePoint.Value;
	}
}
