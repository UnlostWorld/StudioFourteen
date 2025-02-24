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

namespace StudioFourteen.Server.Analytics;

using System.Text.Json;
using System.Threading.Tasks;

public enum AnalyticEvents
{
	None = 0,

	StudioStarted,
}

public class AnalyticEvent
{
	public AnalyticEvents Event { get; set; }
	public string? EventData { get; set; }

	public static void Send(AnalyticEvents evt, string? data = null)
	{
		AnalyticEvent eventObj = new();
		eventObj.Event = evt;
		eventObj.EventData = data;
		Task.Run(eventObj.Send);
	}

	public Task Send()
	{
		string json = JsonSerializer.Serialize(this);
		return ServerApi.PostAsync("/Analytics/Event", json, "application/json");
	}
}