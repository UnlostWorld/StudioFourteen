namespace StudioFourteen.Services;

using StudioFourteen.Studio;
using StudioFourteen.Utilities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using TerraFX.Interop.Windows;
using WpfUtils.Extensions;

using Panel = StudioFourteen.Panels.Panel;
using PanelWindow = StudioFourteen.Panels.PanelWindow;

public class PanelService : ServiceBase
{
	private readonly List<Panel> openPanels = new List<Panel>();
	private readonly Dictionary<Type, Panel> lastOpenPanels = new();

	private bool hasRestoredPanels = false;
	private BackgroundWindow? backgroundWindow;

	public IEnumerable<Panel> OpenPanels => this.openPanels;

	public Panel? ActivePanel { get; set; }

	public override Task Initialize()
	{
		EventManager.RegisterClassHandler(typeof(FrameworkElement), FrameworkElement.LoadedEvent, new RoutedEventHandler((s, e) => this.OnLoaded(s, e)));
		this.Services.Studio.Opening += this.OnOpening;

		return base.Initialize();
	}

	public override Task Shutdown()
	{
		this.Services.Studio.Opening -= this.OnOpening;
		return base.Shutdown();
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

	public async Task<T?> Open<T>()
		where T : Panel, new()
	{
		Panel? p = await this.Open(typeof(T));
		return p as T;
	}

	public async Task<Panel?> Open(Type panelType)
	{
		PanelWindow? wnd = await PanelWindow.CreatePanelWindow<PanelWindow>();
		if (wnd != null)
		{
			await wnd.Dispatcher.InvokeAsync(() =>
			{
				wnd.Panel = Activator.CreateInstance(panelType) as Panel;

				if (wnd.Panel != null)
				{
					wnd.Panel.SetHost(wnd);
					wnd.ShowActivated = true; // ??
					wnd.Show();
				}
			});

			return wnd.Panel;
		}

		return null;
	}

	public void Close<T>()
		where T : Panel, new()
	{
		this.Get<T>()?.Close();
	}

	public void SetIsOpen<T>(bool value)
		where T : Panel, new()
	{
		if (value)
		{
			if (this.GetIsOpen<T>())
				return;

			this.Open<T>().Run();
		}
		else
		{
			this.Get<T>()?.Close();
		}
	}

	public override async Task Start()
	{
		await base.Start();

		this.backgroundWindow = await PanelWindow.CreatePanelWindow<BackgroundWindow>();
		this.backgroundWindow?.Dispatcher.InvokeAsync(() => this.backgroundWindow.Show());

		if (!this.hasRestoredPanels && this.Services.Studio.IsOpen)
		{
			this.RestorePanels().Run();
		}
	}

	public override async Task Stop()
	{
		await base.Stop();

		this.backgroundWindow?.Dispatcher.Invoke(this.backgroundWindow.Close);

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
	}

	private async Task RestorePanels()
	{
		this.hasRestoredPanels = true;

		// make sure at least one game frame as passed
		await Threads.FrameworkThread();

		// plus a short delay
		await Task.Delay(100);

		foreach (string panelTypeName in this.Settings.OpenPanels)
		{
			Type? panelType = Type.GetType(panelTypeName);
			if (panelType != null)
			{
				await this.Open(panelType);
			}
			else
			{
				this.Log.Information($"Failed to find panel type {panelTypeName}");
			}
		}
	}

	private void OnLoaded(object s, RoutedEventArgs e)
	{
		ToolTipService.SetShowOnDisabled((DependencyObject)e.OriginalSource, true);
	}

	private void OnOpening()
	{
		if (!this.hasRestoredPanels && this.Services.Studio.IsOpen)
		{
			this.RestorePanels().Run();
		}
	}
}
