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
using Serilog;
using StudioFourteen.Settings;

public abstract class PanelContextBase
{
	protected readonly ILogger Log;

	protected readonly List<Panel> openPanels = new List<Panel>();
	protected readonly Dictionary<Type, Panel> openPanelsTypeLookup = new();

	public PanelContextBase()
	{
		this.Log = Logging.ForContext(this.GetType());
	}

	public delegate void PanelDelegate(Panel panel);

	public event PanelDelegate? PanelOpened;
	public event PanelDelegate? PanelMinimized;
	public event PanelDelegate? PanelClosed;
	public event PanelDelegate? PanelActivated;
	public event PanelDelegate? PanelDeactivated;

	protected ServiceManager Services => ServiceManager.Instance;
	protected Configuration Settings => this.Services.Settings.Current;

	public virtual void OnPanelOpened(Panel panel)
	{
		lock (this)
		{
			this.openPanels.Add(panel);

			Type panelType = panel.GetType();
			this.openPanelsTypeLookup[panelType] = panel;
		}

		this.PanelOpened?.Invoke(panel);
	}

	public virtual void OnPanelClosed(Panel panel, bool isMinimized)
	{
		lock (this)
		{
			this.openPanels.Remove(panel);

			Type panelType = panel.GetType();
			this.openPanelsTypeLookup.Remove(panelType);
		}

		if (isMinimized)
		{
			this.PanelMinimized?.Invoke(panel);
		}
		else
		{
			this.PanelClosed?.Invoke(panel);
		}
	}

	public T? GetOpenPanel<T>()
		where T : Panel
	{
		return this.GetOpenPanel(typeof(T)) as T;
	}

	public Panel? GetOpenPanel(Type panelType)
	{
		this.openPanelsTypeLookup.TryGetValue(panelType, out var panel);
		return panel;
	}

	public void CreatePanel<T>(bool activate = true)
		where T : Panel
	{
		this.CreatePanelAsync<T>(activate).RunAsynchronously();
	}

	public void CreatePanel(Type panelType, bool activate)
	{
		this.CreatePanelAsync(panelType, activate).RunAsynchronously();
	}

	public async Task<T?> CreatePanelAsync<T>(bool activate = true)
		where T : Panel
	{
		Panel? panel = await this.CreatePanelAsync(typeof(T), activate);
		return panel as T;
	}

	public abstract Task<Panel?> CreatePanelAsync(Type panelType, bool activate);
	public abstract Task StopPanels();
	public abstract Task RestorePanels();
	public abstract Task TogglePanel(Type panelType);

	public void SetIsOpen<T>(bool open, bool activate)
		where T : Panel
	{
		this.SetIsOpenAsync<T>(open, activate).RunAsynchronously();
	}

	public void SetIsOpen(Type panelType, bool open, bool activate)
	{
		this.SetIsOpenAsync(panelType, open, activate).RunAsynchronously();
	}

	public async Task<T?> SetIsOpenAsync<T>(bool open, bool activate)
		where T : Panel
	{
		T? panel = this.GetOpenPanel<T>();
		if (open && panel == null)
		{
			panel = await this.CreatePanelAsync<T>(activate);
		}
		else if (!open && panel != null)
		{
			panel.Close(false);
		}

		return panel;
	}

	public async Task SetIsOpenAsync(Type panelType, bool open, bool activate)
	{
		Panel? panel = this.GetOpenPanel(panelType);
		if (open && panel == null)
		{
			panel = await this.CreatePanelAsync(panelType, activate);
		}
		else if (open && panel != null)
		{
			await panel.Dispatcher.BeginInvoke(panel.Activate);
		}
		else if (!open && panel != null)
		{
			panel.Close(false);
		}
	}

	public void OnPanelActivated(Panel panel, bool active)
	{
		if (active)
		{
			this.PanelActivated?.Invoke(panel);
		}
		else
		{
			this.PanelDeactivated?.Invoke(panel);
		}
	}
}
