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
using Avalonia.Controls;
using StudioFourteen;
using StudioFourteen.Services.Content;
using StudioFourteen.Services.Tick;

public class XivaloniaContent
{
	private readonly XamlContentReference<Window> contentReference;
	private Window? content;

	public XivaloniaContent(string path)
	{
		this.contentReference = new(path);
		this.contentReference.OnReloaded += this.OnContentReloaded;
	}

	public void Show()
	{
		this.content = this.contentReference.Get();
		this.content.Show();
	}

	public void Close()
	{
		this.content?.Close();
		this.content = null;
	}

	private void OnContentReloaded()
	{
		Studio.Tick.Dispatch(TickChannels.Ui, () =>
		{
			this.content?.Close();
			this.content = this.contentReference.Get();
			this.content?.Show();
		});
	}
}