// .                      @@             _____ _______ _    _ _____ _____ ____
//            @       @@@@@             / ____|__   __| |  | |  __ \_   _/ __ \
//           @@@  @@@@                 | (___    | |  | |  | | |  | || || |  | |
//           @@@@@@@@@  @    @          \___ \   | |  | |  | | |  | || || |  | |
//          @@@@       @@@@@@@          ____) |  | |  | |__| | |__| || || |__| |
//      @@@@@             @@@          |_____/   |_|   \____/|_____/_____\____/
//       @@@      @@@      @@        ___     _    _   _  __   _____  ___  ___  _  _
//        @@    @@@@@@@    @@       |  _|  / _ \ | | | || _ \|_   _|| __|| __|| \| |
//        @@    @@@@@@@    @   @    | __| | (_) || |_| ||   /  | |  | _| | _| | .` |
//      @@@@      @@@      @@@@     |_|    \___/  \___/ |_|_\  |_|  |___||___||_|\_|
//       @@@@             @@@
//         @@@@@      @@@@@               This software is licensed under the
//          @@@@@@@@@@@@@@                 GNU AFFERO GENERAL PUBLIC LICENSE
//              @@@@  @                       Version 3, 19 November 2007

namespace StudioFourteen.SPA;

using StudioFourteen.Panels;
using System.Threading.Tasks;
using WpfUtils.Windows;

public partial class SpaWindow : PanelWindow
{
	private static SpaWindow? instance;

	public static void OpenSpa()
	{
		Task.Run(async () =>
		{
			SpaWindow? spa = await PanelWindow.CreatePanelWindow<SpaWindow>();
			if (spa != null)
			{
				instance = spa;

				spa.Dispatcher.Invoke(() =>
				{
					spa.Show();
				});
			}
		});
	}

	public static void CloseSpa()
	{
		if (instance == null)
			return;

		instance.Dispatcher?.Invoke(instance.Close);
	}
}