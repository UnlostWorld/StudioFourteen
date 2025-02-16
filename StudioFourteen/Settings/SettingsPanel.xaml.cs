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

namespace StudioFourteen.Settings;
using StudioFourteen.Mvm;
using StudioFourteen.Panels;
using System.IO;
using System.Windows;

public partial class SettingsPanel : Panel
{
	[AutoNotify] public SettingsService.Configuration Settings => this.Services.Settings.Current;

	private async void OnBrosePhotoDirectoryClicked(object sender, RoutedEventArgs e)
	{
		DirectoryInfo? dir = null;
		if (this.Settings.PhotoDirectory != null)
			dir = new DirectoryInfo(this.Settings.PhotoDirectory);

		DirectoryInfo? newDir = await this.Services.Files.ShowDirectoryDialog(dir);

		if (newDir == null)
			return;

		this.Settings.PhotoDirectory = newDir.FullName;
	}
}
