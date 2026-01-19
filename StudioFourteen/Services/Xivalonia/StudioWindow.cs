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

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Layout;
using StudioFourteen;
using StudioFourteen.Services.Content;
using StudioFourteen.Services.Tick;
using StudioFourteen.Services.Xivalonia;

public class StudioWindow
{
	private readonly AvaloniaContentReference<Visual> contentReference;
	private readonly AvaloniaContentReference<Visual>? chromeReference;

	private XivaloniaWindow? window;
	private ContentControl? presenter;

	public StudioWindow(string contentPath, bool hasChrome = true)
	{
		this.contentReference = new(contentPath);
		this.contentReference.OnReloaded += this.OnContentReloaded;

		if (hasChrome)
		{
			this.chromeReference = new("UI/WindowChrome.ui");
			this.chromeReference.OnReloaded += this.OnChromeReloaded;
		}
	}

	public void Show()
	{
		if (this.window == null)
			this.window = new();

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

		this.presenter?.Content = this.contentReference.Get();
		this.window.Show();
	}

	public void Close()
	{
		this.window?.Close();
	}

	private void OnContentReloaded()
	{
		Studio.Tick.Dispatch(TickChannels.Ui, () =>
		{
			if (this.presenter == null)
				return;

			this.presenter.Content = this.contentReference.Get();
		});
	}

	private void OnChromeReloaded()
	{
		Studio.Tick.Dispatch(TickChannels.Ui, () =>
		{
			if (this.window == null || this.chromeReference == null)
				return;

			this.presenter?.Content = null;

			Visual chromeVisual = this.chromeReference.Get();
			this.window.Content = chromeVisual;
			this.presenter = chromeVisual.Find<ContentControl>("WindowContents");

			this.presenter?.Content = this.contentReference.Get();
		});
	}
}