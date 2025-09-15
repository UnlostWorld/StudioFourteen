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

using PropertyChanged.SourceGenerator;
using StudioFourteen.Panels;
using System.Threading.Tasks;

public partial class LongTaskWindow : Panel
{
	public LongTaskWindow()
	{
		this.Status = string.Empty;
	}

	[Bind] public partial string Status { get; set; }
	[Bind] public partial double? Progress { get; set; }

	public static async Task<LongTaskWindow?> Show()
	{
		return await ServiceManager.Instance.Panels.GamePanels.CreatePanelAsync<LongTaskWindow>();
	}

	public void SetStatus(string status)
	{
		this.Status = status;
	}

	public void SetProgress(double? progress)
	{
		if (progress == null)
		{
			this.Progress = null;
		}
		else
		{
			this.Progress = progress * 100;
		}
	}
}