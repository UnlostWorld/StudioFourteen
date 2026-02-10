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
using global::Avalonia.Platform;
using global::Avalonia;
using global::Avalonia.Controls;
using global::Avalonia.Input;
using global::Avalonia.Input.Raw;
using StudioFourteen.Services.Input;
using StudioFourteen.Services.Tick;

public class StudioMouseDevice : MouseDevice
{
	private readonly StudioPointer pointer;

	private readonly Input0DListener mousePrimaryClickListener = new(InputAction.UI_PrimaryClick);
	private readonly Input0DListener mouseSecondaryClickListener = new(InputAction.UI_SecondaryClick);
	private readonly Input0DListener mouseMiddleClickListener = new(InputAction.UI_MiddleClick);
	private readonly Input1DListener mouseScrollListener = new(InputAction.UI_ScrollDown, InputAction.UI_ScrollUp);

	private WindowImpl? windowUnderCursor = null;
	private WindowImpl? capturedWindow = null;

	public StudioMouseDevice()
		: base(StudioPointer.CreatePointer(out var pointer))
	{
		this.pointer = pointer;
		Studio.Tick.Add(TickChannels.Ui, this.OnUiTick);
	}

	public WindowImpl? WindowUnderCursor
	{
		get => this.windowUnderCursor;
		private set
		{
			if (this.windowUnderCursor == value)
				return;

			if (this.windowUnderCursor != null && this.windowUnderCursor.InputRoot != null)
			{
				ulong ts = (ulong)DateTime.UtcNow.Ticks;
				RawPointerEventType type = RawPointerEventType.LeaveWindow;
				RawInputModifiers modifiers = RawInputModifiers.None;
				RawPointerEventArgs args = new(
					this,
					ts,
					this.windowUnderCursor.InputRoot,
					type,
					new Point(0, 0),
					modifiers);
				this.windowUnderCursor.HandleInput(args);
			}

			this.windowUnderCursor = value;

			if (this.windowUnderCursor == null)
			{
				this.mousePrimaryClickListener.Disable();
				this.mouseSecondaryClickListener.Disable();
				this.mouseMiddleClickListener.Disable();
				this.mouseScrollListener.Disable();
			}
			else
			{
				this.mousePrimaryClickListener.Enable();
				this.mouseSecondaryClickListener.Enable();
				this.mouseMiddleClickListener.Enable();
				this.mouseScrollListener.Enable();
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

	internal void Capture(WindowImpl window)
	{
		this.capturedWindow = window;
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
				if (testWindowImpl.InputRoot is Visual vis && !vis.IsVisible)
					continue;

				PixelPoint position = testWindowImpl.Position;
				Size size = testWindowImpl.FrameSize ?? testWindowImpl.ClientSize;

				// TODO: Z-SORT!
				if (mousePoint.X > position.X
					&& mousePoint.Y > position.Y
					&& mousePoint.X < position.X + size.Width
					&& mousePoint.Y < position.Y + size.Height)
				{
					if (testWindowImpl.InputRoot == null)
						continue;

					if (bestWindow != null)
					{
						if (!testWindowImpl.IsTopMost && bestWindow.IsTopMost)
							continue;

						if (testWindowImpl.ActivatedTime < bestWindow.ActivatedTime)
							continue;
					}

					bestWindow = testWindowImpl;
				}
			}

			this.WindowUnderCursor = bestWindow;
		}
		else
		{
			this.WindowUnderCursor = this.capturedWindow;
		}

		if (this.WindowUnderCursor != null && this.WindowUnderCursor.InputRoot != null)
		{
			Point relativeMousePosition = new(
				mousePoint.X - this.WindowUnderCursor.Position.X,
				mousePoint.Y - this.WindowUnderCursor.Position.Y);

			ulong timeStamp = (ulong)DateTime.UtcNow.Ticks;

			// TODO:
			RawInputModifiers modifiers = RawInputModifiers.None;

			this.WindowUnderCursor.HandleInput(
				new RawPointerEventArgs(
					this,
					timeStamp,
					this.WindowUnderCursor.InputRoot,
					RawPointerEventType.Move,
					relativeMousePosition,
					modifiers));

			// Left Click
			InputStates mousePrimaryState = this.mousePrimaryClickListener.GetState();
			if (mousePrimaryState == InputStates.Activated)
			{
				this.WindowUnderCursor.HandleInput(
				new RawPointerEventArgs(
					this,
					timeStamp,
					this.WindowUnderCursor.InputRoot,
					RawPointerEventType.LeftButtonDown,
					relativeMousePosition,
					modifiers));
			}
			else if (mousePrimaryState == InputStates.Deactivated)
			{
				this.WindowUnderCursor.HandleInput(
				new RawPointerEventArgs(
					this,
					timeStamp,
					this.WindowUnderCursor.InputRoot,
					RawPointerEventType.LeftButtonUp,
					relativeMousePosition,
					modifiers));
			}

			// Right Click
			InputStates mouseSecondaryState = this.mouseSecondaryClickListener.GetState();
			if (mouseSecondaryState == InputStates.Activated)
			{
				this.WindowUnderCursor.HandleInput(
				new RawPointerEventArgs(
					this,
					timeStamp,
					this.WindowUnderCursor.InputRoot,
					RawPointerEventType.RightButtonDown,
					relativeMousePosition,
					modifiers));
			}
			else if (mouseSecondaryState == InputStates.Deactivated)
			{
				this.WindowUnderCursor.HandleInput(
				new RawPointerEventArgs(
					this,
					timeStamp,
					this.WindowUnderCursor.InputRoot,
					RawPointerEventType.RightButtonUp,
					relativeMousePosition,
					modifiers));
			}

			// Middle Click
			InputStates mouseMiddleState = this.mouseMiddleClickListener.GetState();
			if (mouseSecondaryState == InputStates.Activated)
			{
				this.WindowUnderCursor.HandleInput(
				new RawPointerEventArgs(
					this,
					timeStamp,
					this.WindowUnderCursor.InputRoot,
					RawPointerEventType.MiddleButtonDown,
					relativeMousePosition,
					modifiers));
			}
			else if (mouseSecondaryState == InputStates.Deactivated)
			{
				this.WindowUnderCursor.HandleInput(
				new RawPointerEventArgs(
					this,
					timeStamp,
					this.WindowUnderCursor.InputRoot,
					RawPointerEventType.MiddleButtonUp,
					relativeMousePosition,
					modifiers));
			}

			float scroll = this.mouseScrollListener.Value;
			if (scroll != 0)
			{
				this.WindowUnderCursor.HandleInput(
				new RawMouseWheelEventArgs(
					this,
					timeStamp,
					this.WindowUnderCursor.InputRoot,
					relativeMousePosition,
					new global::Avalonia.Vector(0, scroll),
					modifiers));
			}
		}
		else
		{
			// Clicking outside of a studio window.
			if (Studio.Input.Mouse?.GetButton(Input.Devices.MouseButtons.Left) == true)
			{
				IFocusManager? focusManager = AvaloniaLocator.Current.GetService<IFocusManager>();
				focusManager?.ClearFocus();
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
