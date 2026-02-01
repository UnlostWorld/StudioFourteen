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
using System.Drawing;
using System.Numerics;
using FFXIVClientStructs.FFXIV.Client.System.Input;
using global::Dalamud.Game.Addon.Events;
using Windows.Win32;

public enum MouseButtons
{
	Left = MouseButtonFlags.LBUTTON,
	Middle = MouseButtonFlags.MBUTTON,
	Right = MouseButtonFlags.RBUTTON,
	XButton1 = MouseButtonFlags.XBUTTON1,
	XButton2 = MouseButtonFlags.XBUTTON2,
}

public enum Cursors
{
	Arrow = AddonCursorType.Arrow,
	Boot = AddonCursorType.Boot,
	Search = AddonCursorType.Search,
	ChatPointer = AddonCursorType.ChatPointer,
	Interact = AddonCursorType.Interact,
	Attack = AddonCursorType.Attack,
	Hand = AddonCursorType.Hand,
	ResizeWE = AddonCursorType.ResizeWE,
	ResizeNS = AddonCursorType.ResizeNS,
	ResizeNWSE = AddonCursorType.ResizeNWSR,
	ResizeNESW = AddonCursorType.ResizeNESW,
	Clickable = AddonCursorType.Clickable,
	TextInput = AddonCursorType.TextInput,
	TextClick = AddonCursorType.TextClick,
	Grab = AddonCursorType.Grab,
	ChatBubble = AddonCursorType.ChatBubble,
	NoAccess = AddonCursorType.NoAccess,
	Hidden = AddonCursorType.Hidden,
}

#pragma warning disable

public class MouseDevice : InputDeviceBase
{
	private const float MinDragDistance = 3.0f / 1920.0f;   // 3px at 1920 res.

	private readonly Dictionary<MouseButtons, Vector2> dragStarts = new();
	private readonly HashSet<MouseButtons> draggingButtons = new();
	private readonly Dictionary<MouseButtons, InputAxis> buttonAxes = new();
	private readonly Dictionary<MouseButtons, (InputAxisSigned X, InputAxisSigned Y)> dragAxis = new();

	private readonly InputAxis positionX;
	private readonly InputAxis positionY;

	private readonly InputAxisSigned wheel;

	private Cursors cursor = Cursors.Arrow;
	private bool isOverridingCursor = false;
	private Vector2 lastMousePosition;
	private bool lockCursor = false;
	private Point lockCursorPoint;

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

	public void LockCursor(bool enableLock)
	{
		if (enableLock && this.lockCursor)
			throw new Exception("Attempt to lock mouse cursor that is already locked");

		if (enableLock)
		{
			PInvoke.GetCursorPos(out this.lockCursorPoint);
			this.cursor = Cursors.Hidden;
		}
		else
		{
			PInvoke.ClipCursor(null);
			this.cursor = Cursors.Arrow;
		}

		this.lockCursor = enableLock;
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

		PInvoke.GetCursorPos(out Point p);
		Vector2 clientSize = Studio.Window.GetClientSize();
		Vector2 position = new(p.X / clientSize.X, p.Y / clientSize.Y);

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
			this.dragAxis[button].X.Value += (float)delta.X;
			this.dragAxis[button].Y.Value += (float)delta.Y;
		}

		if (this.lockCursor)
		{
			PInvoke.SetCursorPos(this.lockCursorPoint.X, this.lockCursorPoint.Y);

			PInvoke.GetCursorPos(out Point p2);
			position = new(p2.X / clientSize.X, p2.Y / clientSize.Y);
		}

		this.positionX.Value = position.X;
		this.positionY.Value = position.Y;
		this.lastMousePosition = position;
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

		bool shouldConsume = this.ShouldConsumeMouse();
		if (!this.isOverridingCursor && shouldConsume)
		{
			this.isOverridingCursor = true;
		}
		else if (this.isOverridingCursor && shouldConsume)
		{
			Studio.AddonEventManager.SetCursor((AddonCursorType)this.cursor);
		}
		else if (this.isOverridingCursor && !shouldConsume)
		{
			this.isOverridingCursor = false;
			Studio.AddonEventManager.SetCursor(AddonCursorType.Arrow);
			Studio.AddonEventManager.ResetCursor();
		}
	}

	public bool HandleMouseButton(MouseButtons button, bool down)
	{
		if (!down)
		{
			this.draggingButtons.Remove(button);
			this.dragStarts.Remove(button);
			this.buttonAxes[button].Value = 0.0f;
		}

		Vector2 mousePoint = this.GetPosition();
		if (down)
		{
			this.dragAxis[button].X.Value = 0;
			this.dragAxis[button].Y.Value = 0;
			this.dragStarts[button] = mousePoint;
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
	}

	public bool ShouldConsumeMouse()
	{
		if (Studio.IsDisposed || !Studio.IsInitialized)
			return false;

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

		if (Studio.Avalonia.IsWindowUnderCursor)
			return true;

		// TODO: Needs to be true when the cursor is over a handle.
		return false;
	}
}
