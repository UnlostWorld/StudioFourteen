namespace StudioFourteen.Panels;

using DependencyPropertyGenerator;
using StudioFourteen.Mvm;
using StudioFourteen.Plugin;
using StudioFourteen.Services;
using StudioFourteen.Utilities;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using WpfUtils.Extensions;
using WpfUtils.Windows;

[DependencyProperty<bool>("IsEmbedded", DefaultValue = true)]
[DependencyProperty<bool>("CanClose", DefaultValue = true)]
[DependencyProperty<bool>("CanChangeEmbed", DefaultValue = true)]
[DependencyProperty<double>("Scale", DefaultValue = 1.0)]
[DependencyProperty<bool>("IsMaximized", DefaultValue = false)]
public partial class PanelWindow : MultithreadedWindow, IAutoNotify, Panel.IHost
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
		this.Resources = StudioFourteen.Resources.Load();
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
				this.MinWidth = this.panel.MinWidth + 24;
				this.MinHeight = this.panel.MinHeight + 24 + 30;

				this.panel.Width = double.NaN;
				this.panel.HorizontalAlignment = HorizontalAlignment.Stretch;
				this.panel.Height = double.NaN;
				this.panel.VerticalAlignment = VerticalAlignment.Stretch;
			}
		}
	}

	public static async Task<T?> CreatePanelWindow<T>()
		where T : PanelWindow
	{
		if (ServiceManager.Instance.CurrentState > ServiceManagerBase.States.Started)
			return null;

		return await MultithreadedWindow.CreateInstanceAsync<T>();
	}

	void Panel.IHost.Close()
	{
		this.Dispatcher.BeginInvoke(this.Close);
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
			if (this.IsEmbedded)
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

		if (ServiceManager.ShutdownRequested)
			return;

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
		if (ServiceManager.ShutdownRequested)
			return;

		this.isOpen = false;

		if (DalamudServices.GameGui != null)
			DalamudServices.GameGui.UiHideToggled -= this.OnGameUiToggled;

		if (this.Services.Panels.ActivePanel == this.Panel)
			this.Services.Panels.ActivePanel = null;

		this.SavedPosition = this.Position;
		this.SavedSize = new Point(this.Width, this.Height);

		AutoPropertyNotifyService.Remove(this);
		this.panel?.SetIsOpen(this, false);
	}

	protected override void OnActivated(EventArgs e)
	{
		if (ServiceManager.ShutdownRequested)
			return;

		this.Services.Panels.ActivePanel = this.Panel;
		base.OnActivated(e);
	}

	protected override void OnDeactivated(EventArgs e)
	{
		if (ServiceManager.ShutdownRequested)
			return;

		if (this.Services.Panels.ActivePanel == this.Panel)
			this.Services.Panels.ActivePanel = null;

		base.OnDeactivated(e);
	}

	protected override void OnStateChanged(EventArgs e)
	{
		if (ServiceManager.ShutdownRequested)
			return;

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
}