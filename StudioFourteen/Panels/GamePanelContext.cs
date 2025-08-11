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

namespace StudioFourteen.Panels;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using WpfUtils;

public class GamePanelContext : PanelContextBase
{
	public override async Task<Panel?> CreatePanelAsync(Type panelType, bool activate)
	{
		PanelWindow? wnd = await PanelWindow.CreatePanelWindow<PanelWindow>(this);
		if (wnd != null)
		{
			await wnd.Dispatcher.InvokeAsync(() =>
			{
				wnd.Panel = Activator.CreateInstance(panelType) as Panel;

				if (wnd.Panel != null)
				{
					wnd.Panel.SetHost(wnd);
					wnd.Show();

					if (activate)
					{
						wnd.Activate();
					}
				}
			});

			return wnd.Panel;
		}

		return null;
	}

	public override async Task RestorePanels()
	{
		foreach (string panelTypeName in this.Settings.OpenPanels)
		{
			Type? panelType = Type.GetType(panelTypeName);
			if (panelType != null)
			{
				await this.CreatePanelAsync(panelType, false);
			}
			else
			{
				this.Log.Information($"Failed to find panel type {panelTypeName}");
			}
		}
	}

	public override Task StopPanels()
	{
		this.Settings.OpenPanels.Clear();

		List<Panel> openPanels = new(this.openPanels);
		foreach (Panel? panel in openPanels)
		{
			if (panel == null)
				continue;

			if (panel.RememberWindowState)
			{
				string? panelTypeName = panel.GetType().FullName;
				if (panelTypeName != null)
				{
					this.Settings.OpenPanels.Add(panelTypeName);
				}
			}

			panel.Close();
		}

		this.Services.Settings.SaveImmediate();
		return Task.CompletedTask;
	}

	public override async Task TogglePanel(Type panelType)
	{
		Panel? panel = this.GetOpenPanel(panelType);

		if (panel == null)
		{
			await this.CreatePanelAsync(panelType, true);
		}
		else if (panel != null)
		{
			await panel.MainThread();
			PanelWindow? wnd = panel.FindParent<PanelWindow>();
			if (wnd != null)
			{
				if (this.Services.Windows.IsActive(wnd) || this.Services.Windows.WasLastActive(wnd))
				{
					await wnd.CloseAsync(true);
				}
				else
				{
					wnd.Activate();
				}
			}
		}
	}
}
