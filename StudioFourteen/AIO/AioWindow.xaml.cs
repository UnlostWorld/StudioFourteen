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
using PropertyChanged.SourceGenerator;

[DependencyProperty<bool>("IsMenuOpen")]
public partial class AioWindow : PanelWindow
{
	private static AioWindow? instance;

	[Notify] private Panel? currentPanel;
	[Notify] private string? currentTitle;

	public static void OpenAio()
	{
		if (instance != null)
			return;

		Task.Run(async () =>
		{
			AioWindow? aio = await PanelWindow.CreatePanelWindow<AioWindow>(ServiceManager.Instance.Panels.AioPanels);
			if (aio != null)
			{
				ServiceManager.Instance.Panels.AioPanels.Window = aio;
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

	public async Task<Panel?> CreatePanel(Type panelType)
	{
		await this.MainThread();
		this.CurrentPanel = this.PanelArea.SetPanel(panelType);

		string currentTitle = StudioFourteen.Resources.Find("LOC_AIO_Title", "Studio Fourteen");
		if (this.CurrentPanel != null)
			currentTitle += " - " + this.CurrentPanel.Title;

		this.CurrentTitle = currentTitle;

		return this.CurrentPanel;
	}

	protected override bool GetIsUiVisible() => true;

	protected override void OnOpened()
	{
		this.CurrentTitle = StudioFourteen.Resources.Find("LOC_AIO_Title", "Studio Fourteen");
		base.OnOpened();
	}

	protected override void OnClosed()
	{
		base.OnClosed();
		instance = null;
	}

	private void OnLaunchClicked(object sender, RoutedEventArgs e)
	{
		if (!this.IsMenuOpen)
		{
			this.IsMenuOpen = true;
		}
	}
}