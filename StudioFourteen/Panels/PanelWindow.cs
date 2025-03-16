// .                    @@             _____ _______ _    _ _____ _____ ____
//          @       @@@@@             / ____|__   __| |  | |  __ \_   _/ __ \
//         @@@  @@@@                 | (___    | |  | |  | | |  | || || |  | |
//         @@@@@@@@@  @    @          \___ \   | |  | |  | | |  | || || |  | |
//        @@@@       @@@@@@@          ____) |  | |  | |__| | |__| || || |__| |
//    @@@@@             @@@          |_____/   |_|   \____/|_____/_____\____/
//     @@@      @@@      @@        ___     _    _   _  __   _____  ___  ___  _  _
//      @@    @@@@@@@    @@       |  _|  / _ \ | | | || _ \|_   _|| __|| __|| \| |
//      @@    @@@@@@@    @   @    | __| | (_) || |_| ||   /  | |  | _| | _| | .` |
//    @@@@      @@@      @@@@     |_|    \___/  \___/ |_|_\  |_|  |___||___||_|\_|
//     @@@@             @@@        https://github.com/UnlostWorld/StudioFourteen
//       @@@@@      @@@@@
//        @@@@@@@@@@@@@@                This software is licensed under the
//            @@@@  @                  GNU AFFERO GENERAL PUBLIC LICENSE v3

namespace StudioFourteen.Panels;

using DependencyPropertyGenerator;
using PropertyChanged.SourceGenerator;
using Serilog;
using StudioFourteen.Input;
using StudioFourteen.Mvm;
using StudioFourteen.Plugin;
using StudioFourteen.Services;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using WpfUtils;
using WpfUtils.Extensions;
using WpfUtils.Windows;

[DependencyProperty<bool>("IsEmbedded", DefaultValue = true)]
[DependencyProperty<bool>("CanClose", DefaultValue = true)]
[DependencyProperty<bool>("CanChangeEmbed", DefaultValue = true)]
[DependencyProperty<double>("Scale", DefaultValue = 1.0)]
[DependencyProperty<bool>("IsMaximized", DefaultValue = false)]
[DependencyProperty<bool>("RememberState", DefaultValue = false)]
[DependencyProperty<Point>("DefaultPosition", DefaultValueExpression = "new System.Windows.Point(0.5, 0.5)")]
public partial class PanelWindow : MultithreadedWindow, IAutoNotify, Panel.IHost
{
	public readonly Navigation? Navigation;
	protected readonly ILogger Log;

	private double preScaleHeight;
	private double preScaleWidth;
	private Panel? panel;
	private bool isDragMoving = false;
	private bool isMinimizing = false;
	private Point desiredPosition;

	[Notify] private bool isUiVisible = true;
	[Notify] private bool isUiVisibleAndOpen = true;

	public PanelWindow()
	{
		this.Log = Logging.ForContext(this.GetType());

		this.WindowStartupLocation = WindowStartupLocation.Manual;

		// Load a new copy of the resources. Each window needs its own instance for threading reasons.
		this.Resources = StudioFourteen.Resources.Load();
		this.Style = this.DefaultStyle;

		this.GetType().GetMethod("InitializeComponent")?.Invoke(this, null);
		this.DataContext = this;

		this.Loaded += this.OnLoaded;
		this.PreviewMouseDown += this.OnPreviewMouseDown;
		this.PreviewMouseUp += this.OnPreviewMouseUp;
		this.PreviewKeyDown += this.OnPreviewKeyDown;
		this.PreviewKeyUp += this.OnPreviewKeyUp;
		this.Services.Studio.PropertyChanged += this.OnStudioPropertyChanged;
		this.Services.Reshade.ReshadeOverlayChanged += this.OnReshadeOverlayChanged;
		this.Services.Photos.PropertyChanged += this.OnPhotosPropertyChanged;

		if (this.CanNavigate)
		{
			this.Navigation = new(this);
		}
	}

	public event PropertyChangedEventHandler? PropertyChanged;

	public ServiceManager Services => ServiceManager.Instance;

	public bool HasIcon => this.Panel != null && !string.IsNullOrEmpty(this.Panel.TitleIcon);
	public bool HasSubtitle => this.Panel != null && !string.IsNullOrEmpty(this.Panel.Subtitle);
	public bool IsOpen { get; private set; }
	public virtual bool CanNavigate => true;

	public Style DefaultStyle => (Style)this.FindResource("PanelWindowStyle");

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
		get => this.Services.Windows.GetPosition(this);
		set
		{
			this.desiredPosition = value;
			this.Services.Windows.SetPosition(this, value, false);
		}
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
			if (this.panel != null)
			{
				this.panel.PropertyChanged -= this.OnPanelPropertyChanged;
			}

			this.Content = value;
			this.panel = value;

			if (this.IsOpen)
			{
				this.panel?.SetIsOpen(this, true, false);
			}

			if (this.panel != null)
			{
				this.Title = this.panel.Title;
				this.SizeToContent = this.panel.SizeToContent;
				this.ResizeMode = this.panel.ResizeMode;
				this.Width = this.panel.Width;
				this.Height = this.panel.Height;
				this.MinWidth = this.panel.MinWidth + 24 + 6;
				this.MinHeight = this.panel.MinHeight + 24 + 30;
				this.RememberState = this.panel.RememberWindowState;
				this.DefaultPosition = this.panel.DefaultPosition;

				if (this.panel.HostStyle != null)
					this.Style = this.panel.HostStyle;

				this.panel.Width = double.NaN;
				this.panel.HorizontalAlignment = HorizontalAlignment.Stretch;
				this.panel.Height = double.NaN;
				this.panel.VerticalAlignment = VerticalAlignment.Stretch;

				this.panel.PropertyChanged += this.OnPanelPropertyChanged;

				this.NotifyPropertyChanged(nameof(this.IsUiVisibleAndOpen));
				this.NotifyPropertyChanged(nameof(this.IsUiVisible));
				this.NotifyPropertyChanged(nameof(this.HasIcon));
				this.NotifyPropertyChanged(nameof(this.HasSubtitle));

				if (this.SavedPosition != null && this.RememberState)
				{
					this.Position = this.SavedPosition ?? new Point(0, 0);
				}
				else
				{
					this.Position = this.panel.DefaultPosition;
				}
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

	public virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
	{
		this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	public virtual bool ShouldTickAutoProperties()
	{
		return this.IsVisible && this.IsLoaded;
	}

	public virtual T? GetPersistence<T>([CallerMemberName] string id = "")
	{
		if (this.Panel == null)
			return default;

		return this.Panel.GetPersistence<T>(id);
	}

	public virtual void SetPersistence(object? value, [CallerMemberName] string id = "")
	{
		this.Panel?.SetPersistence(id, value);
	}

	public virtual void SetPersistence(string id, object? value)
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

	public virtual async Task CloseAsync(bool minimize = false)
	{
		this.IsOpen = false;
		this.NotifyPropertyChanged(nameof(this.IsOpen));
		this.isMinimizing = minimize;

		await Task.Delay(250);
		this.Dispatcher.Invoke(this.Close);
	}

	public new void DragMove()
	{
		this.isDragMoving = true;
		base.DragMove();
		this.isDragMoving = false;
	}

	protected void OnLoaded(object sender, RoutedEventArgs e)
	{
		try
		{
			this.Services.Windows.OnWindowOpening(this);

			if (this.IsEmbedded)
				this.Services.Windows.Embed(this);

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

		this.Navigation?.Deactivate();

		if (ServiceManager.ShutdownRequested)
			return;

		this.Services.Windows.XivClientSizeChanged -= this.OnXivClientSizeChanged;
		this.Services.Windows.OnWindowClosing(this);

		this.OnClosed();
	}

	protected virtual void OnOpened()
	{
		this.Services.Windows.XivClientSizeChanged += this.OnXivClientSizeChanged;

		this.IsOpen = true;
		this.NotifyPropertyChanged(nameof(this.IsOpen));

		if (DalamudServices.GameGui != null)
			DalamudServices.GameGui.UiHideToggled += this.OnGameUiToggled;

		AutoPropertyNotifyService.Register(this);

		this.preScaleHeight = this.Height;
		this.preScaleWidth = this.Width;

		this.Scale = this.SavedScale;

		if (this.SavedPosition != null && this.RememberState)
		{
			this.Position = this.SavedPosition ?? new Point(0, 0);
		}
		else if (this.panel != null)
		{
			this.Position = this.panel.DefaultPosition;
		}

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

		if (this.Services.Windows.XivProcess == null)
		{
			this.CanChangeEmbed = false;
		}

		this.panel?.SetIsOpen(this, true, false);

		if (this.SavedPosition != null)
		{
			this.Services.Windows.SetPosition(this, (Point)this.SavedPosition, true);
		}
		else
		{
			this.Services.Windows.SetPosition(this, this.Position, true);
		}
	}

	protected virtual void OnClosed()
	{
		if (ServiceManager.ShutdownRequested)
			return;

		this.IsOpen = false;
		this.NotifyPropertyChanged(nameof(this.IsOpen));

		if (DalamudServices.GameGui != null)
			DalamudServices.GameGui.UiHideToggled -= this.OnGameUiToggled;

		this.SavedPosition = this.Position;

		if (this.SizeToContent != SizeToContent.WidthAndHeight)
			this.SavedSize = new Point(this.Width, this.Height);

		AutoPropertyNotifyService.Remove(this);
		this.panel?.SetIsOpen(this, false, this.isMinimizing);
	}

	protected override void OnActivated(EventArgs e)
	{
		if (ServiceManager.ShutdownRequested)
			return;

		if (this.Panel != null)
			this.Services.Panels.OnPanelActivated(this.Panel, true);

		this.Navigation?.Activate();
		base.OnActivated(e);
	}

	protected override void OnDeactivated(EventArgs e)
	{
		if (ServiceManager.ShutdownRequested)
			return;

		if (this.Panel != null)
			this.Services.Panels.OnPanelActivated(this.Panel, false);

		this.Navigation?.Deactivate();
		base.OnDeactivated(e);
	}

	protected override void OnStateChanged(EventArgs e)
	{
		if (ServiceManager.ShutdownRequested)
			return;

		base.OnStateChanged(e);
		this.IsMaximized = this.WindowState == WindowState.Maximized;
	}

	protected virtual void OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
	{
		if (this.IsActive)
			return;

		// Hack fix for window focus states causing popups to open then close sometimes
		// when Windows takes too long to set focus to everything.
		int delay = 0;
		if (Mouse.DirectlyOver is FrameworkElement el)
		{
			ButtonBase? button = el.FindParent<ButtonBase>();
			if (button != null)
			{
				delay = 50;
			}
		}

		if (delay > 0)
			Thread.Sleep(delay);

		this.Services.Windows.BringToTop(this);

		if (delay > 0)
			Thread.Sleep(delay);

		this.Activate();

		if (delay > 0)
			Thread.Sleep(delay);
	}

	protected virtual void OnPreviewMouseUp(object sender, MouseButtonEventArgs e)
	{
	}

	protected override void OnLocationChanged(EventArgs e)
	{
		base.OnLocationChanged(e);

		if (!this.isDragMoving)
			return;

		this.SavedPosition = this.Position;
		this.desiredPosition = this.Position;
	}

	protected virtual bool GetIsUiVisible()
	{
		if (this.panel?.VisibilityMode == PanelVisibility.Always)
			return true;

		if (this.Services.Photos.IsPhotoMode)
			return this.panel?.VisibilityMode == PanelVisibility.PhotoMode;

		if (DalamudServices.GameGui?.GameUiHidden == true)
			return false;

		if (this.Services.Reshade.IsReshadeOverlayOpen)
			return false;

		return true;
	}

	protected virtual bool GetIsUiVisibleAndOpen()
	{
		if (this.panel?.VisibilityMode == PanelVisibility.Always)
			return true;

		if (this.Services.Photos.IsPhotoMode)
			return this.panel?.VisibilityMode == PanelVisibility.PhotoMode;

		if (!this.IsUiVisible)
			return false;

		return this.Services.Studio.IsOpen;
	}

	partial void OnIsEmbeddedChanged(bool newValue)
	{
		this.WindowState = WindowState.Normal;

		if (newValue)
		{
			this.Services.Windows.Embed(this);
		}
		else
		{
			this.Services.Windows.Unembed(this);
		}

		// wiggle wiggle
		this.OnResizeDelta(new DragDeltaEventArgs(1, 1));
		this.OnResizeDelta(new DragDeltaEventArgs(-1, -1));
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

	private void OnXivClientSizeChanged(Rect newSize)
	{
		this.Dispatcher.Invoke(() =>
		{
			if (this.RememberState)
			{
				if (this.SavedPosition != null)
				{
					this.Position = this.SavedPosition.Value;
				}
			}
			else
			{
				this.Position = this.DefaultPosition;
			}
		});
	}

	private void OnPreviewKeyDown(object sender, KeyEventArgs e)
	{
		if (this.Services.Input.IsStudioTextInputActive)
			return;

		this.Services.Input.Keyboard?.HandleKey(e.Key, true);
		e.Handled = true;
	}

	private void OnPreviewKeyUp(object sender, KeyEventArgs e)
	{
		if (this.Services.Input.IsStudioTextInputActive)
			return;

		this.Services.Input.Keyboard?.HandleKey(e.Key, false);
		e.Handled = true;
	}

	private void OnReshadeOverlayChanged(bool open)
	{
		this.UpdateUiVisible();
	}

	private void OnGameUiToggled(object? sender, bool e)
	{
		this.UpdateUiVisible();
	}

	private void OnStudioPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		this.UpdateUiVisible();
	}

	private void OnPhotosPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		this.UpdateUiVisible();
	}

	private void OnPanelPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		this.NotifyPropertyChanged(nameof(PanelWindow.HasIcon));
		this.NotifyPropertyChanged(nameof(PanelWindow.HasSubtitle));
	}

	private void UpdateUiVisible()
	{
		this.Dispatcher.Invoke(() =>
		{
			this.IsUiVisible = this.GetIsUiVisible();
			this.IsUiVisibleAndOpen = this.GetIsUiVisibleAndOpen();
		});
	}
}