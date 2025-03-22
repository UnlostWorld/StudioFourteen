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

using StudioFourteen.Panels;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using WpfUtils;
using WpfUtils.Extensions;

using Panel = StudioFourteen.Panels.Panel;

public partial class SettingsPanel : Panel
{
	public static void Show(PanelContextBase context, string? elementName = null)
	{
		ShowAsync(context, elementName).Run();
	}

	public static async Task ShowAsync(PanelContextBase context, string? elementName = null)
	{
		SettingsPanel? panel = await context.SetIsOpenAsync<SettingsPanel>(true, true);
		if (panel == null)
			return;

		if (elementName != null)
		{
			await panel.MainThread();
			panel.ShowElement(elementName);
		}
	}

	public void ShowElement(string elementName)
	{
		object? element = this.FindName(elementName);
		if (element == null)
			return;

		if (element is TabItem tabItem)
		{
			tabItem.IsSelected = true;
		}
	}

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
