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

namespace StudioFourteen.Services.Avalonia.Platform;

using System;
using System.Numerics;
using global::Avalonia;
using global::Avalonia.Controls;
using global::Avalonia.Input;
using global::Avalonia.Input.Raw;
using StudioFourteen.Services.Input;
using StudioFourteen.Services.Tick;

public class StudioMouseDevice : MouseDevice
{
	private readonly StudioPointer pointer;

	private readonly Input0DListener mousePrimaryClickListener = new(InputAction.UI_Primary_Click);
	private readonly Input0DListener mouseSecondaryClickListener = new(InputAction.UI_Secondary_Click);
	private readonly Input0DListener mouseMiddleClickListener = new(InputAction.UI_Middle_Click);

	private Window? windowUnderCursor = null;
	private Window? capturedWindow = null;

	public StudioMouseDevice()
		: base(StudioPointer.CreatePointer(out var pointer))
	{
		this.pointer = pointer;
		Studio.Tick.Add(TickChannels.Ui, this.OnUiTick);
	}

	public Window? WindowUnderCursor
	{
		get => this.windowUnderCursor;
		private set
		{
			if (this.windowUnderCursor == value)
				return;

			if (this.windowUnderCursor?.PlatformImpl is WindowImpl leftWindow)
			{
				ulong ts = (ulong)DateTime.UtcNow.Ticks;
				RawPointerEventType type = RawPointerEventType.LeaveWindow;
				RawInputModifiers modifiers = RawInputModifiers.None;
				RawPointerEventArgs args = new(
					this,
					ts,
					this.windowUnderCursor,
					type,
					new Point(0, 0),
					modifiers);
				leftWindow.HandleInput(args);
			}

			this.windowUnderCursor = value;

			if (this.windowUnderCursor == null)
			{
				this.mousePrimaryClickListener.Disable();
				this.mouseSecondaryClickListener.Disable();
				this.mouseMiddleClickListener.Disable();
			}
			else
			{
				this.mousePrimaryClickListener.Enable();
				this.mouseSecondaryClickListener.Enable();
				this.mouseMiddleClickListener.Enable();
			}
		}
	}

	public void DisposeMouse()
	{
		Studio.Tick.Remove(TickChannels.Ui, this.OnUiTick);
	}

	// Normally user should use IPointer.Capture instead of MouseDevice.Capture,
	// But on Windows we need to handle WM_MOUSE capture manually without having access to the Pointer.
	internal void Capture(IInputElement? control)
	{
		this.pointer.Capture(control);
	}

	internal void HandleCapture(IInputElement? control)
	{
		if (control != null)
		{
			this.capturedWindow = this.WindowUnderCursor;
		}
		else
		{
			this.capturedWindow = null;
		}
	}

	private void OnUiTick()
	{
		if (Studio.Input.Mouse == null)
			return;

		Vector2 mousePosition = Studio.Input.Mouse.GetPosition();
		PixelPoint mousePoint = new(
			(int)(mousePosition.X * Studio.Avalonia.Windowing.Screens.RendererScreen.Bounds.Width),
			(int)(mousePosition.Y * Studio.Avalonia.Windowing.Screens.RendererScreen.Bounds.Height));

		WindowImpl? bestWindow = null;
		if (this.capturedWindow == null)
		{
			foreach (WindowImpl testWindowImpl in Studio.Avalonia.Windowing.Windows)
			{
				PixelPoint position = testWindowImpl.Position;
				Size size = testWindowImpl.FrameSize ?? testWindowImpl.ClientSize;

				// TODO: Z-SORT!
				if (mousePoint.X > position.X
					&& mousePoint.Y > position.Y
					&& mousePoint.X < position.X + size.Width
					&& mousePoint.Y < position.Y + size.Height)
				{
					if (testWindowImpl.Window == null)
						continue;

					bestWindow = testWindowImpl;
					break;
				}
			}

			this.WindowUnderCursor = bestWindow?.Window;
		}
		else
		{
			this.WindowUnderCursor = this.capturedWindow;
		}

		if (this.WindowUnderCursor != null && this.windowUnderCursor?.PlatformImpl is WindowImpl windowImpl)
		{
			Point relativeMousePosition = new(
				mousePoint.X - this.WindowUnderCursor.Position.X,
				mousePoint.Y - this.WindowUnderCursor.Position.Y);

			ulong timeStamp = (ulong)DateTime.UtcNow.Ticks;

			// TODO:
			RawInputModifiers modifiers = RawInputModifiers.None;

			windowImpl.HandleInput(
				new RawPointerEventArgs(
					this,
					timeStamp,
					this.WindowUnderCursor,
					RawPointerEventType.Move,
					relativeMousePosition,
					modifiers));

			// Left Click
			InputStates mousePrimaryState = this.mousePrimaryClickListener.GetState();
			if (mousePrimaryState == InputStates.Activated)
			{
				windowImpl.HandleInput(
				new RawPointerEventArgs(
					this,
					timeStamp,
					this.WindowUnderCursor,
					RawPointerEventType.LeftButtonDown,
					relativeMousePosition,
					modifiers));
			}
			else if (mousePrimaryState == InputStates.Deactivated)
			{
				windowImpl.HandleInput(
				new RawPointerEventArgs(
					this,
					timeStamp,
					this.WindowUnderCursor,
					RawPointerEventType.LeftButtonUp,
					relativeMousePosition,
					modifiers));
			}

			// Right Click
			InputStates mouseSecondaryState = this.mouseSecondaryClickListener.GetState();
			if (mouseSecondaryState == InputStates.Activated)
			{
				windowImpl.HandleInput(
				new RawPointerEventArgs(
					this,
					timeStamp,
					this.WindowUnderCursor,
					RawPointerEventType.RightButtonDown,
					relativeMousePosition,
					modifiers));
			}
			else if (mouseSecondaryState == InputStates.Deactivated)
			{
				windowImpl.HandleInput(
				new RawPointerEventArgs(
					this,
					timeStamp,
					this.WindowUnderCursor,
					RawPointerEventType.RightButtonUp,
					relativeMousePosition,
					modifiers));
			}

			// Middle Click
			InputStates mouseMiddleState = this.mouseMiddleClickListener.GetState();
			if (mouseSecondaryState == InputStates.Activated)
			{
				windowImpl.HandleInput(
				new RawPointerEventArgs(
					this,
					timeStamp,
					this.WindowUnderCursor,
					RawPointerEventType.MiddleButtonDown,
					relativeMousePosition,
					modifiers));
			}
			else if (mouseSecondaryState == InputStates.Deactivated)
			{
				windowImpl.HandleInput(
				new RawPointerEventArgs(
					this,
					timeStamp,
					this.WindowUnderCursor,
					RawPointerEventType.MiddleButtonUp,
					relativeMousePosition,
					modifiers));
			}
		}
	}

	internal class StudioPointer()
		: Pointer(Pointer.GetNextFreeId(), PointerType.Mouse, true)
	{
		public static StudioPointer CreatePointer(out StudioPointer pointer)
		{
			return pointer = new();
		}

		protected override void PlatformCapture(IInputElement? element)
		{
			Studio.Avalonia.MouseDevice.HandleCapture(element);
		}
	}
}
