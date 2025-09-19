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

public partial class ErrorReportPanel : Panel
{
	public ErrorReportPanel()
	{
		this.ReportingEnabled = true;
	}

	[Bind] public partial string? ErrorMessage { get; set; }
	[Bind] public partial bool IsSending { get; set; }
	[Bind] public partial string? ShortCode { get; set; }
	[Bind] public partial bool ReportingEnabled { get; set; }

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