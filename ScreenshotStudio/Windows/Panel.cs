// © XivTools.
// Licensed under the MIT license.

namespace ScreenshotStudio.Windows;

using ScreenshotStudio.Services;
using ScreenshotStudio.Structs;
using ScreenshotStudio.Utilities;
using Serilog;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using XivToolsWpf;

public abstract partial class Panel : Window, IAutoNotify
{
	public static readonly DependencyProperty ShowBackgroundProperty = DependencyProperty.Register(
		nameof(Panel.ShowBackground),
		typeof(bool),
		typeof(Panel),
		new(true));

	protected readonly ILogger Log;

	public Panel()
	{
		this.Log = Logging.ForContext(this.GetType());

		this.Loaded += this.OnLoaded;
		this.Unloaded += this.OnUnloaded;

		// Load a new copy of the resources. Each panel needs its own instance for threading reasons.
		this.Resources = ScreenshotStudio.Resources.Load();
		this.Style = this.GetDefaultStyle();

		this.GetType().GetMethod("InitializeComponent")?.Invoke(this, null);
		this.DataContext = this;
	}

	public event PropertyChangedEventHandler? PropertyChanged;

	public ServiceManager Services => ServiceManager.Instance;

	public bool ShowBackground
	{
		get => (bool)this.GetValue(ShowBackgroundProperty);
		set => this.SetValue(ShowBackgroundProperty, value);
	}

	public unsafe Actor* Target => this.Services.Targets.GPoseTarget;

	public static void Show<T>()
		where T : Panel
	{
		Task.Run(async () => await ShowAsync<T>());
	}

	public static async Task<T?> ShowAsync<T>()
		where T : Panel
	{
		T? wnd = await CreateInstance<T>();

		if (wnd != null)
			await wnd.ShowAsync();

		return wnd;
	}

	public static async Task<T?> CreateInstance<T>()
		where T : Panel
	{
		Panel? pwb = await Panel.CreateInstance(typeof(T));
		return pwb as T;
	}

	public static async Task<Panel?> CreateInstance(Type panelWindowType)
	{
		return await new PanelThread().Start(panelWindowType);
	}

	public new void Show()
	{
		this.Services.Panels.OpenPanels.Add(this);
		this.Dispatcher.BeginInvoke(() => base.Show());
	}

	public async Task ShowAsync()
	{
		this.Services.Panels.OpenPanels.Add(this);
		await this.Dispatcher.MainThread();
		base.Show();
	}

	public new void Close()
	{
		this.Services.Panels.OpenPanels.Remove(this);
		this.Dispatcher.BeginInvoke(() => base.Close());
		this.Dispatcher.InvokeShutdown();
	}

	public async Task CloseAsync()
	{
		this.Services.Panels.OpenPanels.Remove(this);
		await this.Dispatcher.MainThread();
		base.Close();
		this.Dispatcher.InvokeShutdown();
	}

	public virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
	{
		this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	protected virtual Style GetDefaultStyle() => (Style)this.FindResource("PanelStyle");

	protected virtual void OnLoaded(object sender, RoutedEventArgs e)
	{
		XivWindow.Embed(this);
		AutoPropertyNotifyService.Register(this);
	}

	private void OnUnloaded(object sender, RoutedEventArgs e)
	{
		AutoPropertyNotifyService.Remove(this);
	}

	private class PanelThread
	{
		private static readonly object CreateInstanceLock = new();

		private Panel? panel;
		private Type? panelType;

		protected ILogger Log => Logging.ForContext<PanelThread>();

		public async Task<Panel?> Start(Type panelType)
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
				this.Log.Error($"Failed to create panel window {this.panelType}");

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
				lock (PanelThread.CreateInstanceLock)
				{
					this.panel = Activator.CreateInstance(this.panelType) as Panel;
				}
			}
			catch (Exception ex)
			{
				this.Log.Error(ex, $"Exception during panel construction: {this.panelType}");
				return;
			}

			this.Log.Information($"Panel: {this.panelType} has started");
			System.Windows.Threading.Dispatcher.Run();
			this.Log.Information($"Panel: {this.panelType} has shutdown");
		}
	}
}