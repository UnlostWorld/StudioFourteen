namespace ScreenshotStudio.Services;

using ScreenshotStudio.Studio;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

using Panel = ScreenshotStudio.Windows.Panel;

public class PanelService : ServiceBase
{
	private readonly List<Panel> openPanels = new List<Panel>();
	private readonly Dictionary<Type, Panel> lastOpenPanels = new();

	private BackgroundWindow? backgroundWindow;

	public override Task Initialize()
	{
		EventManager.RegisterClassHandler(typeof(FrameworkElement), FrameworkElement.LoadedEvent, new RoutedEventHandler((s, e) => this.OnLoaded(s, e)));

		return base.Initialize();
	}

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

	public override async Task Start()
	{
		await base.Start();

		this.backgroundWindow = await Panel.ShowAsync<BackgroundWindow>();

		foreach (string panelTypeName in this.Settings.OpenPanels)
		{
			Type? panelType = Type.GetType(panelTypeName);
			if (panelType != null)
			{
				Panel.Show(panelType);
			}
			else
			{
				this.Log.Information($"Failed to find panel type {panelTypeName}");
			}
		}
	}

	public override async Task Stop()
	{
		await base.Stop();

		this.Settings.OpenPanels.Clear();

		List<Panel> openPanels = new(this.openPanels);
		foreach (Panel? panel in openPanels)
		{
			if (panel == null)
				continue;

			if (panel is not BackgroundWindow && panel is not ErrorWindow)
			{
				string? panelTypeName = panel.GetType().FullName;
				if (panelTypeName != null)
				{
					this.Settings.OpenPanels.Add(panelTypeName);
				}
			}

			await panel.CloseAsync();
		}
	}

	private void OnLoaded(object s, RoutedEventArgs e)
	{
		ToolTipService.SetShowOnDisabled((DependencyObject)e.OriginalSource, true);
	}
}
