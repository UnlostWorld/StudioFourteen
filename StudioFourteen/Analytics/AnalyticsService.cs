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

using StudioFourteen.Services;
using StudioOnline.Analytics;
using System.Threading.Tasks;

[Service]
public class AnalyticsService : ServiceBase
{
	public override async Task Start()
	{
		// Optional analytics default to false, so on first run we wont
		// send the started event, the user will be able to opt-in later
		// in the first run.
		if (this.Services.Settings.Current.SendOptionalAnalytics)
			AnalyticEvent.Send(AnalyticEvents.StudioStarted);

		if (!this.Services.Settings.Current.HasConfirmedAnalyticOptions)
			this.Services.Panels.GamePanels.CreatePanel<AnalyticsOptPanel>(true);

		await base.Start();
	}
}
