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

namespace StudioFourteen.Analytics;

using StudioFourteen.Panels;
using System.Windows;
using StudioFourteen.Plugin;
using PropertyChanged.SourceGenerator;

public partial class ErrorReportPanel : Panel
{
	[Notify] private string? errorMessage;
	[Notify] private bool isSending = false;
	[Notify] private string? shortCode;
	[Notify] private bool reportingEnabled = true;

	public bool IsDebug
	{
		get
		{
#if DEBUG
			return true;
#else
			return false;
#endif
		}
	}

	private void OnConsoleClicked(object sender, RoutedEventArgs e)
	{
		DalamudServices.CommandManager?.ProcessCommand("/xllog");
	}

	private void OnGitHubClicked(object sender, RoutedEventArgs e)
	{
		UrlUtility.Open("https://github.com/UnlostWorld/StudioFourteen");
	}

	private void OnDiscordClicked(object sender, RoutedEventArgs e)
	{
		////UrlUtility.Open("");
	}

	private void OnConfirmClicked(object sender, RoutedEventArgs e)
	{
		this.Close();
	}
}