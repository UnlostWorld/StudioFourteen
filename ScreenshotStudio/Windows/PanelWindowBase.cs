namespace ScreenshotStudio.Windows;
using Serilog;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

public abstract partial class PanelWindowBase : Window
{
	public static readonly DependencyProperty ShowBackgroundProperty = DependencyProperty.Register(
		"ShowBackground",
		typeof(bool),
		typeof(PanelWindowBase));

	public PanelWindowBase()
	{
		this.Resources = ScreenshotStudio.Resources.Instance;
		this.GetType().GetMethod("InitializeComponent")?.Invoke(this, null);
	}

	public bool ShowBackground
	{
		get => (bool)GetValue(ShowBackgroundProperty);
		set => SetValue(ShowBackgroundProperty, value);
	}

	public new void Show() => this.Dispatcher.BeginInvoke(() => base.Show());
	public new void Close() => this.Dispatcher.BeginInvoke(() => base.Close());

	public static async Task<T?> CreateInstance<T>()
		where T : PanelWindowBase
	{
		PanelWindowBase? pwb = await PanelWindowBase.CreateInstance(typeof(T));
		return pwb as T;
	}

	public static async Task<PanelWindowBase?> CreateInstance(Type panelWindowType)
	{
		return await new PanelWindowThread().Start(panelWindowType);
	}

	private class PanelWindowThread
	{
		private static readonly object createInstanceLock = new();

		private PanelWindowBase? panel;
		private Type? panelType;

		protected ILogger Log => Serilog.Log.ForContext<PanelWindowThread>();

		public async Task<PanelWindowBase?> Start(Type panelType)
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
				lock (PanelWindowThread.createInstanceLock)
				{
					this.panel = Activator.CreateInstance(this.panelType) as PanelWindowBase;
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