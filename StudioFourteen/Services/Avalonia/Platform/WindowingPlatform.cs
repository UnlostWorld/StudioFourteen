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
using System.Collections.Generic;
using global::Avalonia;
using global::Avalonia.Platform;
using global::Avalonia.Rendering.Composition;
using StudioFourteen.Services.Tick;

public class WindowingPlatform : IWindowingPlatform, IDisposable
{
	public readonly List<WindowImpl> Windows = new();
	public readonly StudioScreens Screens;

	public WindowingPlatform(StudioScreens screen)
	{
		this.Screens = screen;
	}

	public void Dispose()
	{
		Studio.Tick.Dispatch(TickChannels.Ui, () =>
		{
			foreach (WindowImpl impl in this.Windows.ToArray())
			{
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
		WindowImpl impl = new(compositor, this.Screens);
		this.Windows.Add(impl);
		return impl;
	}

	public IPopupImpl CreatePopup(WindowImpl owner)
	{
		Compositor compositor = AvaloniaLocator.Current.GetRequiredService<Compositor>();
		PopupImpl impl = new(owner, compositor, this.Screens);
		this.Windows.Add(impl);
		return impl;
	}

	public void ReloadAll()
	{
		foreach (WindowImpl windowImpl in this.Windows)
		{
			if (windowImpl.InputRoot is StudioWindowBase studioWindow)
			{
				studioWindow.WindowReference?.Reload();
			}
		}
	}
}