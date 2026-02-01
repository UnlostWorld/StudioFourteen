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

namespace StudioFourteen.Services.Avalonia;

using System.Numerics;
using CommunityToolkit.Mvvm.ComponentModel;
using global::Avalonia;
using global::Avalonia.Controls;
using global::Avalonia.Layout;
using StudioFourteen;
using StudioFourteen.Services.Avalonia.Platform;
using StudioFourteen.Services.Tick;

public class WindowReference : ObservableObject
{
	private readonly AvaloniaContentReference<Visual> contentReference;
	private readonly AvaloniaContentReference<Visual>? chromeReference;

	private StudioWindowBase? window;
	private ContentControl? presenter;

	public WindowReference(
		string contentPath,
		string? chromePath = "UI/WindowChrome.ui")
	{
		this.contentReference = new(contentPath);
		this.contentReference.Reloaded += this.OnContentReloaded;

		if (!string.IsNullOrEmpty(chromePath))
		{
			this.chromeReference = new(chromePath);
			this.chromeReference.Reloaded += this.OnChromeReloaded;
		}
	}

	public virtual bool CanDragMove => true;
	public virtual Vector2 DefaultPosition => new(0.5f, 0.5f);

	public void Show()
	{
		if (this.window == null)
		{
			this.window = new();
			this.window.WindowReference = this;
			this.window.DataContext = this;
		}

		if (this.chromeReference != null)
		{
			Visual chromeVisual = this.chromeReference.Get();
			this.window.Content = chromeVisual;
			this.presenter = chromeVisual.Find<ContentControl>("WindowContents");
		}
		else
		{
			this.presenter = this.window;
		}

		this.Reload();
		this.window.Show();
	}

	public void Close()
	{
		this.window?.Close();
		this.window = null;
		this.presenter = null;
	}

	public void Hide()
	{
		this.window?.Hide();
	}

	public void Reload()
	{
		this.OnChromeReloaded();
		this.OnContentReloaded();
	}

	private void OnContentReloaded()
	{
		Studio.Tick.Dispatch(TickChannels.Ui, () =>
		{
			if (this.presenter == null || Studio.IsDisposed)
				return;

			this.presenter.Content = this.contentReference.Get();
		});
	}

	private void OnChromeReloaded()
	{
		Studio.Tick.Dispatch(TickChannels.Ui, () =>
		{
			if (this.window == null || this.chromeReference == null || Studio.IsDisposed)
				return;

			this.presenter?.Content = null;

			Visual chromeVisual = this.chromeReference.Get();
			this.window.Content = chromeVisual;
			this.presenter = chromeVisual.Find<ContentControl>("WindowContents");
			this.presenter?.Content = this.contentReference.Get();

			if (this.window.PlatformImpl is WindowImpl impl)
			{
				Border? chrome = chromeVisual.Find<Border>("Chrome");
				if (chrome != null)
				{
					impl.CornerRadius.X = (float)chrome.CornerRadius.TopLeft;
					impl.CornerRadius.Y = (float)chrome.CornerRadius.TopRight;
					impl.CornerRadius.Z = (float)chrome.CornerRadius.BottomRight;
					impl.CornerRadius.W = (float)chrome.CornerRadius.BottomLeft;
					impl.Margin.X = (float)chrome.Margin.Left;
					impl.Margin.Y = (float)chrome.Margin.Top;
					impl.Margin.Z = (float)chrome.Margin.Right;
					impl.Margin.W = (float)chrome.Margin.Bottom;
				}
			}
		});
	}
}