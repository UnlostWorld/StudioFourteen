namespace ScreenshotStudio.Services;

using ScreenshotStudio.Studio;
using ScreenshotStudio.Utilities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using WpfUtils.Extensions;

using Panel = ScreenshotStudio.Windows.Panel;
using PanelWindow = ScreenshotStudio.Windows.PanelWindow;

public class PanelService : ServiceBase
{
	private readonly List<Panel> openPanels = new List<Panel>();
	private readonly Dictionary<Type, Panel> lastOpenPanels = new();

	private BackgroundWindow? backgroundWindow;

	public IEnumerable<Panel> OpenPanels => this.openPanels;

	public Panel? ActivePanel { get; set; }

	public static async Task WhileShown(Panel panel)
	{
		bool isShown = true;
		panel.Dispatcher.ShutdownStarted += (s, e) =>
		{
			isShown = false;
		};

		/*panel.Closing += (s, e) =>
		{
			isShown = false;
		};*/

		while (isShown)
		{
			await Task.Delay(100);
		}
	}

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

	public async Task<T?> Open<T>()
		where T : Panel, new()
	{
		PanelWindow? wnd = await PanelWindow.CreatePanelWindow<PanelWindow>();
		if (wnd != null)
		{
			await wnd.Dispatcher.InvokeAsync(() =>
			{
				wnd.Panel = new T();
				wnd.Panel.SetHost(wnd);
				wnd.ShowActivated = true; // ??
				wnd.Show();
			});

			return wnd.Panel as T;
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

		this.RestorePanels().Run();
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

			string? panelTypeName = panel.GetType().FullName;
			if (panelTypeName != null)
			{
				this.Settings.OpenPanels.Add(panelTypeName);
			}

			panel.Close();
		}
	}

	private async Task RestorePanels()
	{
		// make sure at least one game frame as passed
		await Threads.FrameworkThread();

		// plus a short delay
		await Task.Delay(100);

		foreach (string panelTypeName in this.Settings.OpenPanels)
		{
			Type? panelType = Type.GetType(panelTypeName);
			if (panelType != null)
			{
				////PanelWindow.Show(panelType);
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
}
