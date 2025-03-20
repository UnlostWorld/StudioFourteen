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

namespace StudioFourteen.AIO;

using StudioFourteen.Panels;
using System.Threading.Tasks;
using System.Windows;
using WpfUtils.Windows;
using DependencyPropertyGenerator;
using System;
using WpfUtils;

[DependencyProperty<bool>("IsMenuOpen")]
public partial class AioWindow : PanelWindow
{
	private static AioWindow? instance;

	public static void OpenAio()
	{
		Task.Run(async () =>
		{
			AioWindow? aio = await PanelWindow.CreatePanelWindow<AioWindow>();
			if (aio != null)
			{
				instance = aio;

				aio.Dispatcher.Invoke(() =>
				{
					aio.Show();
				});
			}
		});
	}

	public static void CloseAio()
	{
		if (instance == null)
			return;

		instance.Dispatcher?.Invoke(instance.Close);
	}

	public static bool GetIsOpen()
	{
		if (instance == null)
			return false;

		return true;
	}

	protected override bool GetIsUiVisible() => true;

	protected override void OnOpened()
	{
		base.OnOpened();
		this.Services.Panels.CreateAllInOnePanelCallback = this.CreatePanel;
	}

	private async Task<Panel?> CreatePanel(Type panelType)
	{
		await this.MainThread();
		return this.PanelArea.SetPanel(panelType);
	}

	private void OnLaunchClicked(object sender, RoutedEventArgs e)
	{
		if (!this.IsMenuOpen)
		{
			this.IsMenuOpen = true;
		}
	}
}