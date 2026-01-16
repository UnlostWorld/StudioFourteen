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

namespace StudioFourteen.Services.Xivalonia.Platform;

using System;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform;
using Avalonia.Rendering.Composition;

public class WindowingPlatform : IWindowingPlatform, IDisposable
{
	private static readonly AvaloniaList<Window> Windows = new();
	private static readonly AvaloniaList<WindowImpl> Implementations = new();

	public WindowingPlatform()
	{
		Window.WindowOpenedEvent.AddClassHandler(typeof(Window), OnWindowOpened);
		Window.WindowClosedEvent.AddClassHandler(typeof(Window), OnWindowClosed);
	}

	public void Dispose()
	{
		Studio.Avalonia.Dispatcher.Signaled += () =>
		{
			foreach (Window wnd in Windows)
			{
				wnd.Close();
			}

			foreach (WindowImpl impl in Implementations)
			{
				if (!impl.IsDisposed)
				{
					Studio.Log.Warning($"Window did not dispose");
					impl.Dispose();
				}
			}
		};

		Studio.Avalonia.Dispatcher.Signal();

		////Window.WindowOpenedEvent.RemoveClassHandler(typeof(Window), OnWindowOpened);
		////Window.WindowClosedEvent.RemoveClassHandler(typeof(Window), OnWindowClosed);
	}

	public ITopLevelImpl CreateEmbeddableTopLevel() => throw new NotSupportedException();
	public IWindowImpl CreateEmbeddableWindow() => throw new NotSupportedException();
	public ITrayIconImpl? CreateTrayIcon() => throw new NotSupportedException();

	public IWindowImpl CreateWindow()
	{
		Compositor compositor = AvaloniaLocator.Current.GetRequiredService<Compositor>();
		WindowImpl impl = new(compositor);
		Implementations.Add(impl);
		return impl;
	}

	private static void OnWindowClosed(object? sender, RoutedEventArgs e)
	{
		var window = (Window)sender!;
		Windows.Remove(window);

		if (window.PlatformImpl is WindowImpl impl)
		{
			Implementations.Remove(impl);
		}
	}

	private static void OnWindowOpened(object? sender, RoutedEventArgs e)
	{
		var window = (Window)sender!;
		if (!Windows.Contains(window))
		{
			Windows.Add(window);
		}
	}
}