namespace ScreenshotStudio.Windows;

using DependencyPropertyGenerator;
using Dalamud.Plugin.Services;
using ScreenshotStudio.Plugin;
using ScreenshotStudio.Services;
using ScreenshotStudio.Utilities;
using Serilog;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using WpfUtils;

[DependencyProperty<bool>("ShowBackground", DefaultValue = true)]
[DependencyProperty<bool>("IsShown", DefaultValue = false)]
[DependencyProperty<bool>("IsEmbedded", DefaultValue = true)]
public abstract partial class Panel : Window, IAutoNotify
{
	protected readonly ILogger Log;

	public Panel()
	{
		this.Log = Logging.ForContext(this.GetType());

		this.Loaded += this.OnLoaded;

		// Load a new copy of the resources. Each panel needs its own instance for threading reasons.
		this.Resources = ScreenshotStudio.Resources.Load();
		this.Style = this.GetDefaultStyle();

		this.GetType().GetMethod("InitializeComponent")?.Invoke(this, null);
		this.DataContext = this;

		this.PreviewMouseDown += this.OnPreviewMouseDown;
		this.PreviewKeyDown += this.OnPreviewKeyDown;
		this.PreviewKeyUp += this.OnPreviewKeyUp;
	}

	public event PropertyChangedEventHandler? PropertyChanged;

	public ServiceManager Services => ServiceManager.Instance;

	public bool IsUiVisible => !DalamudServices.GameGui?.GameUiHidden ?? true;

	public static void Show<T>()
		where T : Panel
	{
		Task.Run(async () => await ShowAsync<T>());
	}

	public static void Show(Type panelType)
	{
		Task.Run(async () => await ShowAsync(panelType));
	}

	public static async Task<T?> ShowAsync<T>()
		where T : Panel
	{
		T? wnd = await CreateInstance<T>();

		if (wnd != null)
			await wnd.ShowAsync();

		return wnd;
	}

	public static async Task<Panel?> ShowAsync(Type panelType)
	{
		Panel? wnd = await CreateInstance(panelType);

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
		if (ServiceManager.Instance.CurrentState > ServiceManagerBase.States.Started)
			return null;

		return await new PanelThread().Start(panelWindowType);
	}

	public new void Show()
	{
		this.Services.Panels.OnPanelOpened(this);
		this.Dispatcher.BeginInvoke(() => base.Show());
	}

	public async Task ShowAsync()
	{
		this.Services.Panels.OnPanelOpened(this);
		await this.Dispatcher.MainThread();

		try
		{
			base.Show();
		}
		catch (Exception ex)
		{
			this.Log.Error(ex, "Error showing window");
		}
	}

	public new void Close()
	{
		this.Dispatcher.BeginInvoke(() =>
		{
			try
			{
				this.OnClosed();
			}
			catch (Exception ex)
			{
				this.Log.Error(ex, "Error closing window");
			}

			base.Close();

			this.Dispatcher.InvokeShutdown();
		});
	}

	public async Task CloseAsync()
	{
		await this.Dispatcher.MainThread();

		try
		{
			this.OnClosed();
		}
		catch (Exception ex)
		{
			this.Log.Error(ex, "Error closing window");
		}

		base.Close();
		this.Dispatcher.InvokeShutdown();
	}

	public virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
	{
		this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	public virtual bool ShouldTickAutoProperties()
	{
		return this.IsVisible;
	}

	public virtual void OnResizeDelta(DragDeltaEventArgs e)
	{
		double newWidth = this.ActualWidth + e.HorizontalChange;
		double newHeight = this.ActualHeight + e.VerticalChange;

		if (newWidth >= this.MinWidth && newWidth <= this.MaxWidth)
		{
			this.Width = newWidth;
		}

		if (newHeight >= this.MinHeight && newHeight <= this.MaxHeight)
		{
			this.Height = newHeight;
		}
	}

	protected virtual Style GetDefaultStyle() => (Style)this.FindResource("PanelStyle");

	protected void OnLoaded(object sender, RoutedEventArgs e)
	{
		try
		{
			XivWindow.Embed(this);

			this.OnOpened();
		}
		catch (Exception ex)
		{
			this.Log.Error(ex, "Error opening panel");
		}
	}

	protected virtual void OnOpened()
	{
		if (DalamudServices.GameGui != null)
			DalamudServices.GameGui.UiHideToggled += this.OnGameUiToggled;

		if (DalamudServices.Framework != null)
			DalamudServices.Framework.Update += this.OnFrameworkUpdate;

		AutoPropertyNotifyService.Register(this);
		this.IsShown = true;
	}

	protected virtual void OnClosed()
	{
		if (DalamudServices.GameGui != null)
			DalamudServices.GameGui.UiHideToggled -= this.OnGameUiToggled;

		if (DalamudServices.Framework != null)
			DalamudServices.Framework.Update -= this.OnFrameworkUpdate;

		AutoPropertyNotifyService.Remove(this);
		this.Services.Panels.OnPanelClosed(this);
		this.IsShown = false;
	}

	protected virtual void OnFrameworkUpdate(IFramework framework)
	{
	}

	partial void OnIsEmbeddedChanged(bool newValue)
	{
		double t = this.Top;

		if (newValue)
		{
			XivWindow.Embed(this);
			this.Top = t - (XivWindow.TitleBarHeight + 10);
		}
		else
		{
			XivWindow.Unembed(this);
			this.Top = t;
		}

		// wiggle wiggle
		this.OnResizeDelta(new DragDeltaEventArgs(1, 1));
		this.OnResizeDelta(new DragDeltaEventArgs(-1, -1));

		this.Activate();
	}

	private void OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
	{
		this.Activate();
	}

	private void OnPreviewKeyDown(object sender, KeyEventArgs e)
	{
		if (!this.IsActive)
			return;

		if (Keyboard.FocusedElement is TextBoxBase tb)
		{
			if (tb.IsFocused && (tb.IsKeyboardFocused || tb.IsKeyboardFocusWithin))
			{
				if (e.Key == Key.Escape)
				{
					tb.SetFocusToWindow();
				}

				return;
			}
		}

		this.Services.Input.SetKeyDown(e.Key, true);
	}

	private void OnPreviewKeyUp(object sender, KeyEventArgs e)
	{
		if (!this.IsActive)
			return;

		this.Services.Input.SetKeyDown(e.Key, false);
	}

	private void OnGameUiToggled(object? sender, bool e)
	{
		this.NotifyPropertyChanged(nameof(Panel.IsUiVisible));
	}

	private class PanelThread
	{
		private static readonly object CreateInstanceLock = new();

		private Panel? panel;
		private Type? panelType;

		protected ILogger Log => Logging.ForContext<PanelThread>();

		public async Task<Panel?> Start(Type panelType)
		{
			try
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
			catch (Exception ex)
			{
				this.Log.Error(ex, $"Failed to create panel window {this.panelType}");
			}

			return null;
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

			bool run = true;
			while (run)
			{
				try
				{
					System.Windows.Threading.Dispatcher.Run();
					run = false;
				}
				catch (Exception ex)
				{
					this.Log.Error(ex, $"Error in {this.panelType} thread");
				}
			}

			this.Log.Information($"Panel: {this.panelType} has shutdown");
		}
	}
}