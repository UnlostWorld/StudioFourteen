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
using FontAwesome.Sharp;
using WpfUtils.Extensions;
using System.Collections.Generic;

[DependencyProperty<bool>("IsEmbedded", DefaultValue = true)]
[DependencyProperty<bool>("CanClose", DefaultValue = true)]
[DependencyProperty<bool>("CanChangeEmbed", DefaultValue = true)]
[DependencyProperty<double>("Scale", DefaultValue = 1.0)]
[DependencyProperty<bool>("IsMaximized", DefaultValue = false)]
public partial class PanelWindow : Window, IAutoNotify
{
	protected readonly ILogger Log;

	private readonly string panelId;
	private readonly Dictionary<string, object?> persistenceCache = new();
	private double preScaleHeight;
	private double preScaleWidth;
	private Panel? panel;
	private bool isOpen = false;

	public PanelWindow()
	{
		this.panelId = this.GetType().Name;
		this.Log = Logging.ForContext(this.GetType());

		this.Loaded += this.OnLoaded;

		// Load a new copy of the resources. Each panel needs its own instance for threading reasons.
		this.Resources = ScreenshotStudio.Resources.Load();
		this.Style = (Style)this.FindResource("PanelWindowStyle");

		this.GetType().GetMethod("InitializeComponent")?.Invoke(this, null);
		this.DataContext = this;

		this.PreviewMouseDown += this.OnPreviewMouseDown;
		this.PreviewKeyDown += this.OnPreviewKeyDown;
	}

	public event PropertyChangedEventHandler? PropertyChanged;

	public ServiceManager Services => ServiceManager.Instance;

	public bool IsUiVisible => !DalamudServices.GameGui?.GameUiHidden ?? true;

	public virtual Point? SavedPosition
	{
		get => this.GetPersistence<Point?>();
		set => this.SetPersistence(value);
	}

	public virtual double SavedScale
	{
		get => this.GetPersistence<double?>() ?? 1.0;
		set => this.SetPersistence(value);
	}

	public Point? SavedSize
	{
		get => this.GetPersistence<Point?>();
		set => this.SetPersistence(value);
	}

	public Point Position
	{
		get => XivWindow.GetPosition(this);
		set => XivWindow.SetPosition(this, value);
	}

	public FastObservableCollection<double> ZoomOptions { get; init; } = new()
	{
		0.5,
		0.75,
		1.0,
		1.25,
		1.5,
		2.0,
		2.5,
	};

	public Panel? Panel
	{
		get => this.panel;
		set
		{
			this.Content = value;
			this.panel = value;

			if (this.isOpen)
			{
				this.panel?.SetIsOpen(this, true);
			}

			if (this.panel != null)
			{
				this.Title = this.panel.Title;
				this.SizeToContent = this.panel.SizeToContent;
				this.ResizeMode = this.panel.ResizeMode;
				this.Width = this.panel.Width;
				this.Height = this.panel.Height;

				this.panel.Width = double.NaN;
				this.panel.HorizontalAlignment = HorizontalAlignment.Stretch;
				this.panel.Height = double.NaN;
				this.panel.VerticalAlignment = VerticalAlignment.Stretch;
			}
		}
	}

	public static async Task<T?> CreateInstanceAsync<T>()
		where T : PanelWindow
	{
		if (ServiceManager.Instance.CurrentState > ServiceManagerBase.States.Started)
			return null;

		return await new PanelWindowThread().Start(typeof(T)) as T;
	}

	public virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
	{
		this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	public virtual bool ShouldTickAutoProperties()
	{
		return this.IsVisible && this.IsLoaded;
	}

	public T? GetPersistence<T>([CallerMemberName] string id = "")
	{
		if (this.Panel == null)
			return default;

		return this.Panel.GetPersistence<T>(id);
	}

	public void SetPersistence(object? value, [CallerMemberName] string id = "")
	{
		this.Panel?.SetPersistence(id, value);
	}

	public void SetPersistence(string id, object? value)
	{
		this.Panel?.SetPersistence(id, value);
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

	protected override void OnClosing(CancelEventArgs e)
	{
		base.OnClosing(e);

		this.OnClosed();
	}

	protected virtual void OnOpened()
	{
		this.isOpen = true;

		if (DalamudServices.GameGui != null)
			DalamudServices.GameGui.UiHideToggled += this.OnGameUiToggled;

		AutoPropertyNotifyService.Register(this);

		this.preScaleHeight = this.Height;
		this.preScaleWidth = this.Width;

		this.Scale = this.SavedScale;

		this.Opacity = 0;

		if (this.SavedPosition != null)
			this.Position = (Point)this.SavedPosition;

		if (this.SavedSize != null)
		{
			bool canResize = this.ResizeMode > ResizeMode.CanMinimize;
			if (canResize)
			{
				if (this.SizeToContent == SizeToContent.Width)
				{
					this.Height = this.SavedSize.Value.Y;
				}
				else if (this.SizeToContent == SizeToContent.Height)
				{
					this.Width = this.SavedSize.Value.X;
				}
				else if (this.SizeToContent == SizeToContent.Manual)
				{
					this.Width = this.SavedSize.Value.X;
					this.Height = this.SavedSize.Value.Y;
				}
			}
		}

		if (XivWindow.Process == null)
		{
			this.CanChangeEmbed = false;
		}

		this.panel?.SetIsOpen(this, true);
	}

	protected virtual void OnClosed()
	{
		this.isOpen = false;

		if (DalamudServices.GameGui != null)
			DalamudServices.GameGui.UiHideToggled -= this.OnGameUiToggled;

		this.SavedPosition = this.Position;
		this.SavedSize = new Point(this.Width, this.Height);

		AutoPropertyNotifyService.Remove(this);
		this.panel?.SetIsOpen(this, false);
	}

	protected override void OnActivated(EventArgs e)
	{
		this.Services.Panels.ActivePanel = this.Panel;
		base.OnActivated(e);
	}

	protected override void OnDeactivated(EventArgs e)
	{
		if (this.Services.Panels.ActivePanel == this.Panel)
			this.Services.Panels.ActivePanel = null;

		base.OnDeactivated(e);
	}

	protected override void OnStateChanged(EventArgs e)
	{
		base.OnStateChanged(e);
		this.IsMaximized = this.WindowState == WindowState.Maximized;
	}

	partial void OnIsEmbeddedChanged(bool newValue)
	{
		this.WindowState = WindowState.Normal;

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

	partial void OnIsMaximizedChanged(bool newValue)
	{
		this.WindowState = newValue ? WindowState.Maximized : WindowState.Normal;
	}

	partial void OnScaleChanged()
	{
		this.SavedScale = this.Scale;

		if (this.ResizeMode == ResizeMode.NoResize)
		{
			if (this.SizeToContent == SizeToContent.Width)
			{
				this.Height = this.preScaleHeight * this.Scale;
			}
			else if (this.SizeToContent == SizeToContent.Height)
			{
				this.Width = this.preScaleWidth * this.Scale;
			}
		}
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
	}

	private void OnGameUiToggled(object? sender, bool e)
	{
		this.NotifyPropertyChanged(nameof(PanelWindow.IsUiVisible));
	}

	private class PanelWindowThread
	{
		private static readonly object CreateInstanceLock = new();

		private PanelWindow? window;
		private Type? windowType;

		protected ILogger Log => Logging.ForContext<PanelWindowThread>();

		public async Task<PanelWindow?> Start(Type panelType)
		{
			try
			{
				this.windowType = panelType;

				Thread panelMainThread = new Thread(this.PanelMainThread);
				panelMainThread.SetApartmentState(ApartmentState.STA);
				panelMainThread.Start(this);

				// Wait for the panel to load for up to 5 seconds.
				int timeOut = 5000;
				while (this.window == null && timeOut > 0)
				{
					await Task.Delay(10);
					timeOut -= 10;
				}

				if (this.window == null)
					this.Log.Error($"Failed to create window {this.windowType}");

				return this.window;
			}
			catch (Exception ex)
			{
				this.Log.Error(ex, $"Failed to create window {this.windowType}");
			}

			return null;
		}

		private void PanelMainThread(object? param)
		{
			if (this.windowType == null)
				throw new Exception("No panel type in thread");

			AppDomain.CurrentDomain.UnhandledException += (s, e) =>
			{
				Exception? ex = e.ExceptionObject as Exception;
				this.Log.Error(ex, $"Unhandled Exception in window: {this.windowType}");
			};

			System.Windows.Threading.Dispatcher.CurrentDispatcher.UnhandledException += (s, e) =>
			{
				this.Log.Error(e.Exception, $"Unhandled Exception in window: {this.windowType}");
			};

			try
			{
				// Even though we're doing this on another thread, we can still only do one panel window
				// at a time since WPF's LoadComponent system isn't thread safe.
				lock (PanelWindowThread.CreateInstanceLock)
				{
					this.window = Activator.CreateInstance(this.windowType) as PanelWindow;
				}
			}
			catch (Exception ex)
			{
				this.Log.Error(ex, $"Exception during window construction: {this.windowType}");
				return;
			}

			this.Log.Information($"Panel: {this.windowType} has started");

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
					this.Log.Error(ex, $"Error in {this.windowType} thread");
				}
			}

			this.Log.Information($"Panel: {this.windowType} has shutdown");
		}
	}
}