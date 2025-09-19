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
using Serilog;
using StudioFourteen.Mvm;
using StudioFourteen.Services;
using StudioFourteen.Settings;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using StudioFourteen.Xaml;

public enum PanelVisibility
{
	Always,
	WithUI,
	PhotoMode,
}

[DependencyProperty<string>("Subtitle")]
[DependencyProperty<SizeToContent>("SizeToContent", DefaultValue = SizeToContent.Manual)]
[DependencyProperty<ResizeMode>("ResizeMode", DefaultValue = ResizeMode.CanResizeWithGrip)]
[DependencyProperty<PanelVisibility>("VisibilityMode", DefaultValue = PanelVisibility.WithUI)]
[DependencyProperty<Style>("HostStyle")]
[DependencyProperty<Point>("DefaultPosition", DefaultValueExpression = "new System.Windows.Point(0.5, 0.5)")]
public partial class Panel : ContentControl, IAutoNotify
{
	protected readonly ILogger Log;

	private readonly string panelId;

	private Exception? frameworkException;

	private bool isVisible;
	private bool isMinimized;

	public Panel()
	{
		this.panelId = this.GetType().Name;
		this.Log = Logging.ForContext(this.GetType());
		this.Persistence = Persistence.GetPersistence($"Panel_{this.panelId}");

		// Load a new copy of the resources. Each panel needs its own instance for threading reasons.
		this.Resources = XamlResources.Load();

		this.GetType().GetMethod("InitializeComponent")?.Invoke(this, null);
		this.DataContext = this;
		this.Focusable = false;

		this.IsVisibleChanged += (s, e) => this.isVisible = this.IsVisible;
	}

	public event PropertyChangedEventHandler? PropertyChanged;

	public interface IHost
	{
		Point Position { get; set; }
		void Close(bool minimize);
		PanelContextBase GetContext();
		void Activate();

		void Resize(double deltaX, double deltaY);
	}

	public ServiceManager Services => ServiceManager.Instance;
	public Configuration Settings => this.Services.Settings.Current;

	public bool RememberWindowState { get; set; } = true;
	public Persistence Persistence { get; init; }

	public Point Position
	{
		get => this.Host != null ? this.Host.Position : default;
		set
		{
			if (this.Host == null)
				return;

			this.Host.Position = value;
		}
	}

	protected IHost? Host { get; private set; }

	public T? GetPersistence<T>([CallerMemberName] string id = "", T? defaultValue = default) => this.Persistence.GetPersistence<T>(id, defaultValue);

	public override string ToString()
	{
		return this.GetType().Name;
	}

	public void SetPersistence(object? value, [CallerMemberName] string id = "")
	{
		this.Persistence.SetPersistence(value, id);
		this.NotifyPropertyChanged(new(id));
	}

	public void SetPersistence(string id, object? value)
	{
		this.Persistence.SetPersistence(id, value);
		this.NotifyPropertyChanged(new(id));
	}

	public virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
	{
		this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	public virtual bool ShouldTickAutoProperties()
	{
		return this.isVisible;
	}

	public void SetHost(IHost host)
	{
		this.Host = host;
	}

	public PanelContextBase GetContext()
	{
		if (this.Host == null)
			throw new InvalidOperationException();

		return this.Host.GetContext();
	}

	public void Close(bool minimize = false)
	{
		if (this.Host == null)
			return;

		this.Host.Close(minimize);
	}

	public void SetIsOpen(IHost sender, bool isOpen, bool isMinimized)
	{
		if (this.Host != sender)
			throw new InvalidOperationException();

		if (isOpen)
		{
			this.isMinimized = false;
			this.OnOpened();
		}
		else
		{
			this.isMinimized = isMinimized;
			this.OnClosed();
		}
	}

	public void Activate()
	{
		this.Host?.Activate();
	}

	public virtual void OnActivated()
	{
	}

	public virtual void OnDeactivated()
	{
	}

	protected virtual void OnOpened()
	{
		this.Services.Tick.Add(TickService.Channels.GameTick, this.OnGameTickSafe);

		AutoPropertyNotifyService.Register(this);
		this.GetContext().OnPanelOpened(this);
	}

	protected virtual void OnClosed()
	{
		if (!ServiceManager.ShutdownRequested)
			this.Services.Tick.Remove(TickService.Channels.GameTick, this.OnGameTickSafe);

		AutoPropertyNotifyService.Remove(this);
		this.GetContext().OnPanelClosed(this, this.isMinimized);
	}

	protected virtual void OnGameTick()
	{
	}

	private void OnGameTickSafe()
	{
		if (ServiceManager.ShutdownRequested)
			return;

		if (!this.Services.Studio.IsOpen)
			return;

		if (this.frameworkException != null)
			return;

		try
		{
			this.OnGameTick();
		}
		catch (Exception ex)
		{
			this.frameworkException = ex;
			this.Log.Error(ex, "Error in panel game tick");
		}
	}
}
