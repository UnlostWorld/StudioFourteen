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

namespace StudioFourteen.Studio;

using StudioFourteen.Panels;
using System.Windows;
using StudioFourteen.Plugin;
using WpfUtils.Extensions;

public partial class ErrorWindow : Panel
{
	private static ErrorWindow? instance;
	private static bool isOpening = false;
	private static string? message = "An Unknown error has occurred";

	public string? ErrorMessage
	{
		get => message;
		set
		{
			message = value;
			this.NotifyPropertyChanged();
		}
	}

	public static void Show(string message)
	{
		ErrorWindow.message = message;

		if (instance == null)
		{
			if (isOpening)
				return;

			isOpening = true;
			ServiceManager.Instance.Panels.Open<ErrorWindow>().Run();
		}
		else
		{
			instance.NotifyPropertyChanged(nameof(ErrorMessage));
		}
	}

	protected override void OnOpened()
	{
		instance = this;
		isOpening = false;

		base.OnOpened();
	}

	protected override void OnClosed()
	{
		instance = null;
		base.OnClosed();
	}

	private void OnConsoleClicked(object sender, RoutedEventArgs e)
	{
		DalamudServices.CommandManager?.ProcessCommand("/xllog");
	}

	private void OnGitHubClicked(object sender, RoutedEventArgs e)
	{
		UrlUtility.Open("https://github.com/XIV-Tools/StudioFourteen");
	}

	private void OnDiscordClicked(object sender, RoutedEventArgs e)
	{
		UrlUtility.Open("https://discord.gg/KvGJCCnG8t");
	}

	private void OnConfirmClicked(object sender, RoutedEventArgs e)
	{
		this.Close();
	}
}