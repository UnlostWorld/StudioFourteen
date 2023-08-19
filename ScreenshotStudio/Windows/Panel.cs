namespace ScreenshotStudio.Windows;

using ScreenshotStudio.Utilities;
using Serilog;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

public abstract partial class Panel : Window
{
	public Panel()
	{
		this.Loaded += OnLoaded;

		// Load a new copy of the resources. Each panel needs its own instance for threading reasons.
		this.Resources = ScreenshotStudio.Resources.Load();
		this.Style = this.GetDefaultStyle();

		this.GetType().GetMethod("InitializeComponent")?.Invoke(this, null);
	}

	private void OnLoaded(object sender, RoutedEventArgs e)
	{
		XivWindow.Embed(this);
	}

	public new void Show() => this.Dispatcher.BeginInvoke(() => base.Show());
	public new void Close() => this.Dispatcher.BeginInvoke(() => base.Close());

	public static T? Show<T>()
		where T : Panel
	{
		T? wnd = CreateInstance<T>().Result;
		wnd?.Show();
		return wnd;
	}

	public static async Task<T?> ShowAsync<T>()
		where T : Panel
	{
		T? wnd = await CreateInstance<T>();
		wnd?.Show();
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

	protected virtual Style GetDefaultStyle() => (Style)this.FindResource("PanelStyle");

	private class PanelThread
	{
		private static readonly object createInstanceLock = new();

		private Panel? panel;
		private Type? panelType;

		protected ILogger Log => Serilog.Log.ForContext<PanelThread>();

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
				Log.Error($"Failed to create panel window {this.panelType}");

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
				lock (PanelThread.createInstanceLock)
				{
					this.panel = Activator.CreateInstance(this.panelType) as Panel;
				}
			}
			catch (Exception ex)
			{
				Log.Error(ex, $"Exception during panel construction: {this.panelType}");
				return;
			}

			this.Log.Information($"Panel: {this.panelType} has started");
			System.Windows.Threading.Dispatcher.Run();
			this.Log.Information($"Panel: {this.panelType} has shutdown");
		}
	}
}