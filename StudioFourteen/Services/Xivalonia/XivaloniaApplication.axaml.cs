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

namespace StudioFourteen.Services.Xivalonia;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using StudioFourteen.Services.Content;
using StudioFourteen.Services.Tick;

public partial class XivaloniaApplication : Application
{
	private readonly AvaloniaContentReference<ResourceDictionary> theme = new("UI/Theme.ui");

	public XivaloniaApplication()
	{
		this.theme.OnReloaded += this.OnThemeChanged;
	}

	public override void Initialize()
	{
		AvaloniaXamlLoader.Load(this);
	}

	public void LoadTheme()
	{
		this.Resources = this.theme.Get();
	}

	private void OnThemeChanged()
	{
		Studio.Tick.Dispatch(TickChannels.Ui, () =>
		{
			this.Resources = this.theme.Get();
		});
	}
}