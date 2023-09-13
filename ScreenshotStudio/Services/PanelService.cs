// © XivTools.
// Licensed under the MIT license.

namespace ScreenshotStudio.Services;

using ScreenshotStudio.Windows;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class PanelService : ServiceBase
{
	private readonly List<Panel> openPanels = new List<Panel>();
	private readonly Dictionary<Type, Panel> lastOpenPanels = new();

	// Temp till we have theme settings
	public bool IsUserARealGamer { get; set; } = false;

	public void OnPanelOpened(Panel panel)
	{
		this.openPanels.Add(panel);

		Type panelType = panel.GetType();
		if (!this.lastOpenPanels.ContainsKey(panelType))
			this.lastOpenPanels.Add(panelType, panel);

		this.lastOpenPanels[panelType] = panel;
	}

	public void OnPanelClosed(Panel panel)
	{
		this.openPanels.Remove(panel);

		Type panelType = panel.GetType();
		if (this.lastOpenPanels.ContainsKey(panelType))
		{
			this.lastOpenPanels.Remove(panelType);
		}
	}

	public T? Get<T>()
		where T : Panel, new()
	{
		this.lastOpenPanels.TryGetValue(typeof(T), out var panel);
		return panel as T;
	}

	public bool GetIsOpen<T>()
		where T : Panel, new()
	{
		return this.Get<T>() != null;
	}

	public void SetIsOpen<T>(bool value)
		where T : Panel, new()
	{
		if (value)
		{
			if (this.GetIsOpen<T>())
				return;

			Panel.Show<T>();
		}
		else
		{
			T? panel = this.Get<T>();
			if (panel == null)
				return;

			panel.Close();
		}
	}

	public override async Task Stop()
	{
		await base.Stop();

		List<Panel> openPanels = new(this.openPanels);
		foreach (Panel? panel in openPanels)
		{
			if (panel == null)
				continue;

			await panel.CloseAsync();
		}
	}
}
