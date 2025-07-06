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
using StudioFourteen.Utilities;

using Vector = System.Windows.Vector;

public class MouseDevice : InputDeviceBase
{
	private readonly Dictionary<MouseButton, Point> dragStarts = new();
	private readonly HashSet<MouseButton> draggingButtons = new();
	private readonly Dictionary<MouseButton, InputAxis> buttonAxes = new();
	private readonly Dictionary<MouseButton, (InputAxisSigned X, InputAxisSigned Y)> dragAxis = new();

	private readonly InputAxis positionX;
	private readonly InputAxis positionY;

	private readonly InputAxisSigned wheel;

	private Point lastMousePosition;

	public MouseDevice()
	{
		this.wheel = new(MouseDevice.WheelPos, MouseDevice.WheelNeg, this, true);
		this.AddAxis(this.wheel);

		this.positionX = new(MouseDevice.PositionX, this, false);
		this.AddAxis(this.positionX);

		this.positionY = new(MouseDevice.PositionY, this, false);
		this.AddAxis(this.positionY);

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
	public static string PositionX => $"Mouse:Position:X";
	public static string PositionY => $"Mouse:Position:Y";

	public bool IsAnyDragging => this.draggingButtons.Count > 0;

	public static string GetAxisId(MouseButton button) => $"Mouse:{button}";
	public static string GetDragAxisId(MouseButton button, DragDirections direction) => $"Mouse:{button}Drag:{direction}";

	public static MouseButtonFlags GetEngineFlags(MouseButton button)
	{
		switch (button)
		{
			case MouseButton.Left: return MouseButtonFlags.LBUTTON;
			case MouseButton.Middle: return MouseButtonFlags.MBUTTON;
			case MouseButton.Right: return MouseButtonFlags.RBUTTON;
			case MouseButton.XButton1: return MouseButtonFlags.XBUTTON1;
			case MouseButton.XButton2: return MouseButtonFlags.XBUTTON2;
		}

		throw new NotImplementedException();
	}

	public Vector2 GetPosition() => new(this.positionX.Value, this.positionY.Value);
	public bool GetButton(MouseButton button) => this.buttonAxes[button].Value > 0.05f;

	public override void Attach()
	{
		base.Attach();

		// Listen for mouse down on any UI element
		// and update the last input time to force focus mode to switch
		// correctly, even when we're not capturing mouse inputs.
		EventManager.RegisterClassHandler(
			typeof(UIElement),
			FrameworkElement.MouseDownEvent,
			new RoutedEventHandler((s, e) =>
			{
				if (ServiceManager.ShutdownRequested)
					return;

				this.buttonAxes[MouseButton.Left].UtcLastInput = DateTime.UtcNow;
			}));

		this.buttonAxes[MouseButton.Left].UtcLastInput = DateTime.UtcNow;
	}

	public unsafe override void PreUpdate()
	{
		this.wheel.ConsumedBy = null;

		foreach ((MouseButton button, InputAxis axis) in this.buttonAxes)
		{
			axis.ConsumedBy = null;
		}

		foreach ((MouseButton button, (InputAxisSigned xAxis, InputAxisSigned yAxis)) in this.dragAxis)
		{
			xAxis.ConsumedBy = null;
			yAxis.ConsumedBy = null;
		}

		this.UpdateMousePosition();
	}

	public unsafe override void PostUpdate()
	{
		base.PostUpdate();

		this.wheel.Value = 0;

		foreach ((MouseButton button, (InputAxisSigned xAxis, InputAxisSigned yAxis)) in this.dragAxis)
		{
			xAxis.Value = 0;
			yAxis.Value = 0;
		}

		/*foreach ((MouseButton button, InputAxis axis) in this.buttonAxes)
		{
			axis.Value = 0;
		}*/
	}

	public bool HandleMouseButton(MouseButton button, bool down)
	{
		// If we are clicking into xiv, ensure any studio windows have lost focus correctly.
		if (!this.Services.Windows.IsCursorOverStudio)
			this.Services.Windows.Activate(null);

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

		Point? mousePoint = this.Services.Windows.GetCursorPosition();
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
		if (this.Services.Windows.IsCursorOverStudio
			|| this.Services.Handles.IsCursorOverHandle)
			return true;

		// Don't process mouse if the cursor is over a in-game UI element
		if (this.Services.Windows.IsCursorOverAtkUnit)
			return false;

		// Don't process mouse if the cursor is over a Dalamud ImGUI element.
		if (this.Services.Windows.IsCursorOverImGui)
			return false;

		// If the reshade overlay is open, let it do its cursor things.
		if (this.Services.Reshade.IsReshadeOverlayOpen)
			return false;

		// in group pose, capture all scene mouse inputs.
		return this.Services.GroupPose.IsGroupPosing;
	}

	private void UpdateMousePosition()
	{
		Point? mousePoint = this.Services.Windows.GetCursorPosition();
		Rect clientSize = this.Services.Windows.GetXivWindowClientSize();

		if (mousePoint == null)
		{
			this.HandleMouseLeave();
			return;
		}

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
				this.Services.Windows.SetCursorPosition(new Point((int)dragStart.X, (int)dragStart.Y));
			}

			CursorUtility.SetCursorVisible(false);
		}
		else
		{
			this.positionX.Value = (float)(mousePoint.Value.X / clientSize.Width);
			this.positionY.Value = (float)(mousePoint.Value.Y / clientSize.Height);
		}

		mousePoint = this.Services.Windows.GetCursorPosition();
		if (mousePoint == null)
			return;

		this.lastMousePosition = mousePoint.Value;
	}
}
