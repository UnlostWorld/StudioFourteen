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
using global::Avalonia.Collections;
using global::Avalonia.Controls;
using global::Avalonia.Input;
using global::Avalonia.Input.Raw;
using global::Avalonia.Interactivity;
using global::Avalonia.Platform;
using global::Avalonia.Rendering.Composition;
using StudioFourteen.Services.Tick;

public class WindowingPlatform : IWindowingPlatform, IDisposable
{
	private readonly AvaloniaList<WindowImpl> windows = new();
	private readonly StudioScreens screens;

	private readonly TouchDevice touchDevice = new();
	private readonly MouseDevice mouseDevice = new();
	private readonly PenDevice penDevice = new();

	public WindowingPlatform(StudioScreens screen)
	{
		this.screens = screen;

		Window.WindowOpenedEvent.AddClassHandler(typeof(Window), OnWindowOpened);

		Studio.Tick.Add(TickChannels.Ui, this.OnUiTick);
	}

	public Window? WindowUnderCursor { get; private set; } = null;

	public void Dispose()
	{
		Studio.Tick.Remove(TickChannels.Ui, this.OnUiTick);

		Studio.Tick.Dispatch(TickChannels.Ui, () =>
		{
			foreach (WindowImpl impl in this.windows)
			{
				impl.Window?.Close();

				if (!impl.IsDisposed)
				{
					Studio.Log.Warning($"Window did not dispose");
					impl.Dispose();
				}
			}
		});
	}

	public ITopLevelImpl CreateEmbeddableTopLevel() => throw new NotSupportedException();
	public IWindowImpl CreateEmbeddableWindow() => throw new NotSupportedException();
	public ITrayIconImpl? CreateTrayIcon() => throw new NotSupportedException();

	public IWindowImpl CreateWindow()
	{
		Compositor compositor = AvaloniaLocator.Current.GetRequiredService<Compositor>();
		WindowImpl impl = new(compositor, this.screens);
		this.windows.Add(impl);
		return impl;
	}

	public void ReloadAll()
	{
		foreach (WindowImpl windowImpl in this.windows)
		{
			if (windowImpl.Window is StudioWindowBase studioWindow)
			{
				studioWindow.WindowReference?.Reload();
			}
		}
	}

	private static void OnWindowOpened(object? sender, RoutedEventArgs e)
	{
		Window window = (Window)sender!;
		WindowImpl? impl = window.PlatformImpl as WindowImpl;
		impl?.Window = window;
	}

	private void OnUiTick()
	{
		if (Studio.Input.Mouse == null)
			return;

		Vector2 mousePosition = Studio.Input.Mouse.GetPosition();

		PixelPoint mousePoint = new(
			(int)(mousePosition.X * this.screens.RendererScreen.Bounds.Width),
			(int)(mousePosition.Y * this.screens.RendererScreen.Bounds.Height));

		this.WindowUnderCursor = null;

		foreach (WindowImpl windowImpl in this.windows)
		{
			PixelPoint position = windowImpl.Position;
			Size size = windowImpl.FrameSize ?? windowImpl.ClientSize;

			// TODO: Z-SORT!
			if (mousePoint.X > position.X
				&& mousePoint.Y > position.Y
				&& mousePoint.X < position.X + size.Width
				&& mousePoint.Y < position.Y + size.Height)
			{
				if (windowImpl.Window == null)
					continue;

				this.WindowUnderCursor = windowImpl.Window;

				Point relativeMousePosition = new(mousePoint.X - position.X, mousePoint.Y - position.Y);

				ulong ts = (ulong)DateTime.UtcNow.Ticks;

				RawPointerEventType type = RawPointerEventType.Move;
				RawInputModifiers modifiers = RawInputModifiers.None;
				var args = new RawPointerEventArgs(
					this.mouseDevice,
					ts,
					windowImpl.Window,
					type,
					relativeMousePosition,
					modifiers);
				windowImpl.HandleInput(args);
				break;
			}
		}
	}
}