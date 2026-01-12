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
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform;
using Avalonia.Rendering;
using Avalonia.Rendering.Composition;
using Avalonia.Threading;

public partial class XivaloniaPlatform : IWindowingPlatform, IPlatformLifetimeEventsImpl
{
	public static DispatcherImpl Dispatcher = new();

	private static readonly XivaloniaPlatform Instance = new();
	private static readonly AvaloniaList<Window> Windows = new();
	private static readonly AvaloniaList<WindowImpl> Implementations = new();
	private static Compositor? compositor;

	public event EventHandler<ShutdownRequestedEventArgs>? ShutdownRequested;

	public static Compositor Compositor => compositor ?? throw new Exception("Platform not initialized");

	public static void Initialize()
	{
		AvaloniaLocator.CurrentMutable.Bind<IScreenImpl>().ToSingleton<ScreenImpl>();
		AvaloniaLocator.CurrentMutable.Bind<IDispatcherImpl>().ToConstant(Dispatcher);

		AvaloniaLocator.CurrentMutable.Bind<IRenderTimer>().ToConstant(new DefaultRenderTimer(60));
		AvaloniaLocator.CurrentMutable.Bind<IWindowingPlatform>().ToConstant(Instance);
		AvaloniaLocator.CurrentMutable.Bind<IPlatformLifetimeEventsImpl>().ToConstant(Instance);
		AvaloniaLocator.CurrentMutable.Bind<ICursorFactory>().ToConstant(new CursorFactory());

		IPlatformGraphics? platformGraphics = GlManager.Initialize();
		compositor = new Compositor(platformGraphics);
		AvaloniaLocator.CurrentMutable.Bind<Compositor>().ToConstant(compositor);

		Window.WindowOpenedEvent.AddClassHandler(typeof(Window), OnWindowOpened);
		Window.WindowClosedEvent.AddClassHandler(typeof(Window), OnWindowClosed);
	}

	public static void Stop()
	{
		Dispatcher.Signaled += () =>
		{
			foreach (Window wnd in Windows)
			{
				wnd.Close();
			}

			Instance.ShutdownRequested?.Invoke(Instance, new ShutdownRequestedEventArgs());

			foreach (WindowImpl impl in Implementations)
			{
				if (!impl.IsDisposed)
				{
					Studio.Log.Warning($"Window did not dispose");
					impl.Dispose();
				}
			}
		};

		Dispatcher.Signal();
	}

	public ITopLevelImpl CreateEmbeddableTopLevel() => throw new NotSupportedException();
	public IWindowImpl CreateEmbeddableWindow() => throw new NotSupportedException();
	public ITrayIconImpl? CreateTrayIcon() => throw new NotSupportedException();

	public IWindowImpl CreateWindow()
	{
		WindowImpl impl = new();
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
