

namespace ScreenshotStudio.Services;
using ScreenshotStudio;
using ScreenshotStudio.Panels;
using Serilog;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using XivToolsWpf;
using XivToolsWpf.Extensions;

public class PanelService : ServiceBase
{
	private static readonly object ComponentLock = new();

	private static readonly List<Type> PreLoadPanels = new()
	{
		
	};


	public List<PanelBase> OpenPanels { get; init; } = new();
	public List<PanelBase> ActivePanels { get; init; } = new();
	private Dictionary<Type, PanelBase?> ClosedPanelCache { get; init; } = new();

	public async Task<PanelBase> Show(string panelId)
	{
		Type? panelType = Type.GetType(panelId);

		if (panelType == null)
			throw new Exception($"Failed to locate panel type: {panelId}");

		return await this.Show(panelType);
	}

	public async Task<T> Show<T>()
		where T : PanelBase
	{
		PanelBase panel = await this.Show(typeof(T));

		if (panel is not T tPanel)
			throw new Exception("Panel was wrong type");

		return tPanel;
	}

	public List<PanelBase> GetPanels(Type panelType)
	{
		List<PanelBase> results = new();

		foreach (PanelBase panel in this.OpenPanels)
		{
			if (panel.GetType() == panelType)
			{
				results.Add(panel);
			}
		}

		return results;
	}

	public async Task<PanelBase> Spawn(Type panelType)
	{
		// Do we have a cached version of this panel?
		if (this.ClosedPanelCache.TryGetValue(panelType, out PanelBase? cached))
		{
			this.ClosedPanelCache.Remove(panelType);

			if (cached != null)
			{
				return cached;
			}
		}

		return await new PanelThread().Start(panelType);
	}

	public async Task<PanelBase> Show(Type panelType)
	{
		foreach (PanelBase otherPanel in this.OpenPanels)
		{
			if (otherPanel?.GetType() == panelType)
			{
				// This panel is open, swap to it instead of opening another.
				await otherPanel.Dispatcher.MainThread();
				otherPanel.Window.Activate();
				return otherPanel;
			}
		}

		PanelBase panel = await this.Spawn(panelType);

		await panel.Dispatcher.MainThread();
		PanelHostWindow panelHost = this.CreateWindow();
		panelHost.Panel = panel;
		panelHost.Show();

		this.OpenPanels.Add(panel);
		return panel;
	}

	public void OnPanelClosed(PanelBase panel)
	{
		lock (this)
		{
			this.OpenPanels.Remove(panel);

			Type panelType = panel.GetType();

			if (!this.ClosedPanelCache.ContainsKey(panelType))
			{
				this.ClosedPanelCache.Add(panelType, panel);
			}
		}
	}

	public override async Task Start()
	{
		await Services.Panels.Show<WelcomePanel>();
		this.CompleteStart().Run();
		await base.Start();
	}

	public override Task Shutdown()
	{
		foreach (PanelBase? panel in this.OpenPanels)
		{
			panel?.Dispatcher.BeginInvokeShutdown(DispatcherPriority.Normal);
		}

		foreach (PanelBase? panel in this.ClosedPanelCache.Values)
		{
			panel?.Dispatcher.BeginInvokeShutdown(DispatcherPriority.Normal);
		}

		return base.Shutdown();
	}

	public void OnPanelActivated(PanelBase panel)
	{
		this.ActivePanels.Add(panel);
	}

	public void OnPanelDeactivated(PanelBase panel)
	{
		this.ActivePanels.Remove(panel);
	}

	private PanelHostWindow CreateWindow()
	{
		lock (ComponentLock)
		{
			return new PanelHostOverlayWindow();

			// Pop out?
		}
	}

	private async Task CompleteStart()
	{
		await Dispatch.NonUiThread();

		try
		{
			List<Task> tasks = new();

			foreach (Type panelType in PreLoadPanels)
			{
				Log.Information($"Spawning panel: {panelType}");
				PanelBase panel = await this.Spawn(panelType);
				this.ClosedPanelCache.Add(panelType, panel);
			}

			await Task.WhenAll(tasks);
		}
		catch (Exception ex)
		{
			Log.Error(ex, "Failed to preload panels");
		}
	}

	[Serializable]
	public class PanelSettings
	{
		public PanelSettings()
		{
		}

		public Point? Position { get; set; } = null;
		public Size? Size { get; set; } = null;

		public void Save()
		{
			// TODO
		}
	}

	private class PanelThread
	{
		private PanelBase? panel;
		private Type? panelType;

		protected ILogger Log => Serilog.Log.ForContext<PanelThread>();

		public async Task<PanelBase> Start(Type panelType)
		{
			this.panelType = panelType;

			Thread panelMainThread = new Thread(this.PanelMainThread);
			panelMainThread.SetApartmentState(ApartmentState.STA);
			panelMainThread.Start(this);

			// Wait for the panel to load for up to 5 seconds.
			int timeOut = 5000;
			while (this.panel == null && timeOut > 0)
			{
				await Task.Delay(10);
				timeOut -= 10;
			}

			if (this.panel == null)
				throw new Exception($"Failed to start panel {this.panelType}");

			return this.panel;
		}

		private void PanelMainThread(object? param)
		{
			if (this.panelType == null)
				throw new Exception("No panel type in panel thread");

			try
			{
				// Even though we're doing this on another thread, we can still only do one panel
				// at a time since WPF's LoadComponent system isn't thread safe.
				lock (PanelService.ComponentLock)
				{
					this.panel = Activator.CreateInstance(this.panelType) as PanelBase;
				}
			}
			catch (Exception ex)
			{
				Log.Error(ex, $"Exception during panel construction: {this.panelType}");
				return;
			}

			this.Log.Information($"Panel: {this.panelType} has started");
			Dispatcher.Run();
			this.Log.Information($"Panel: {this.panelType} has shutdown");
		}
	}
}
