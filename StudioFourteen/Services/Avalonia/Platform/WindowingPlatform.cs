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
using global::Avalonia;
using global::Avalonia.Collections;
using global::Avalonia.Controls;
using global::Avalonia.Interactivity;
using global::Avalonia.Platform;
using global::Avalonia.Rendering.Composition;
using StudioFourteen.Services.Tick;

public class WindowingPlatform : IWindowingPlatform, IDisposable
{
	private readonly AvaloniaList<WindowImpl> windows = new();
	private readonly RendererScreen screen;

	public WindowingPlatform(RendererScreen screen)
	{
		this.screen = screen;

		Window.WindowOpenedEvent.AddClassHandler(typeof(Window), OnWindowOpened);
	}

	public void Dispose()
	{
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

		Studio.Avalonia.Dispatcher.Signal();
	}

	public ITopLevelImpl CreateEmbeddableTopLevel() => throw new NotSupportedException();
	public IWindowImpl CreateEmbeddableWindow() => throw new NotSupportedException();
	public ITrayIconImpl? CreateTrayIcon() => throw new NotSupportedException();

	public IWindowImpl CreateWindow()
	{
		Compositor compositor = AvaloniaLocator.Current.GetRequiredService<Compositor>();
		WindowImpl impl = new(compositor, this.screen);
		this.windows.Add(impl);
		return impl;
	}

	private static void OnWindowOpened(object? sender, RoutedEventArgs e)
	{
		Window window = (Window)sender!;
		WindowImpl? impl = window.PlatformImpl as WindowImpl;
		impl?.Window = window;
	}
}