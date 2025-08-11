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

using System;
using System.Threading.Tasks;
using StudioFourteen.Panels;

public class AioPanelContext : PanelContextBase
{
	public AioWindow? Window { get; set; }
	public Panel? CurrentPanel { get; private set; }

	public override async Task<Panel?> CreatePanelAsync(Type panelType, bool activate)
	{
		if (this.Window == null)
			throw new Exception("No AioWindow in context");

		this.CurrentPanel = await this.Window.CreatePanel(panelType);
		return this.CurrentPanel;
	}

	public override Task RestorePanels()
	{
		if (this.Settings.IsAioWindowOpen)
			AioWindow.OpenAio();

		return Task.CompletedTask;
	}

	public override Task StopPanels()
	{
		this.Settings.IsAioWindowOpen = AioWindow.GetIsOpen();
		AioWindow.CloseAio();
		return Task.CompletedTask;
	}

	public override async Task TogglePanel(Type panelType)
	{
		if (this.CurrentPanel?.GetType() == panelType)
			return;

		await this.CreatePanelAsync(panelType, true);
	}

	public override void OnPanelClosed(Panel panel, bool isMinimized)
	{
		base.OnPanelClosed(panel, true);
	}
}