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

using System;
using Avalonia.Layout;
using Avalonia.Media;
using StudioFourteen;
using StudioFourteen.Services.Content;
using StudioFourteen.Services.Tick;
using StudioFourteen.Services.Xivalonia;

public class StudioWindow
{
	private readonly XamlContentReference<Layoutable> contentReference;
	private XivaloniaWindow? window;

	public StudioWindow(string contentPath)
	{
		this.contentReference = new(contentPath);
		this.contentReference.OnReloaded += this.OnContentReloaded;
	}

	public void Show()
	{
		if (this.window == null)
			this.window = new();

		////this.window.Content = this.contentReference.Get();
		this.window.Width = 600;
		this.window.Height = 500;
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
			if (this.window == null)
				return;

			this.window.Content = this.contentReference.Get();
			this.window.UpdateLayout();
		});
	}
}