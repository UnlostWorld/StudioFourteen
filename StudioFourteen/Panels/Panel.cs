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

using Dalamud.Plugin.Services;
using DependencyPropertyGenerator;
using FontAwesome.Sharp;
using Serilog;
using StudioFourteen.Icons;
using StudioFourteen.Mvm;
using StudioFourteen.Plugin;
using StudioFourteen.Services;
using StudioFourteen.Settings;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using WpfUtils.Extensions;

public enum PanelVisibility
{
	Always,
	WithUI,
	PhotoMode,
}

[DependencyProperty<IconDefinitionBase>("TitleIcon")]
[DependencyProperty<string>("Title")]
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
	private IHost? host;

	private bool isVisible;
	private bool isMinimized;

	public Panel()
	{
		this.panelId = this.GetType().Name;
		this.Log = Logging.ForContext(this.GetType());
		this.Persistence = new($"Panel_{this.panelId}");

		// Load a new copy of the resources. Each panel needs its own instance for threading reasons.
		this.Resources = StudioFourteen.Resources.Load();

		this.GetType().GetMethod("InitializeComponent")?.Invoke(this, null);
		this.DataContext = this;
		this.Focusable = false;

		this.IsVisibleChanged += (s, e) => this.isVisible = this.IsVisible;
	}

	public event PropertyChangedEventHandler? PropertyChanged;

	public interface IHost
	{
		Task CloseAsync(bool minimize);
		PanelContextBase GetContext();
	}

	public ServiceManager Services => ServiceManager.Instance;
	public SettingsService.Configuration Settings => this.Services.Settings.Current;

	public bool RememberWindowState { get; set; } = true;
	public Persistence Persistence { get; init; }

	public T? GetPersistence<T>([CallerMemberName] string id = "", T? defaultValue = default) => this.Persistence.GetPersistence<T>(id, defaultValue);

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
		this.host = host;
	}

	public PanelContextBase GetContext()
	{
		if (this.host == null)
			throw new InvalidOperationException();

		return this.host.GetContext();
	}

	public void Close(bool minimize = false)
	{
		this.CloseAsync(minimize).Run();
	}

	public Task CloseAsync(bool minimize = false)
	{
		if (this.host == null)
			return Task.CompletedTask;

		return this.host.CloseAsync(minimize);
	}

	public void SetIsOpen(IHost sender, bool isOpen, bool isMinimized)
	{
		if (this.host != sender)
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

	protected virtual void OnOpened()
	{
		if (DalamudServices.Framework != null)
			DalamudServices.Framework.Update += this.OnFrameworkUpdateSafe;

		AutoPropertyNotifyService.Register(this);
		this.GetContext().OnPanelOpened(this);
	}

	protected virtual void OnClosed()
	{
		if (DalamudServices.Framework != null)
			DalamudServices.Framework.Update -= this.OnFrameworkUpdateSafe;

		AutoPropertyNotifyService.Remove(this);
		this.GetContext().OnPanelClosed(this, this.isMinimized);
	}

	protected virtual void OnFrameworkUpdate(IFramework framework)
	{
	}

	private void OnFrameworkUpdateSafe(IFramework framework)
	{
		if (ServiceManager.ShutdownRequested)
		{
			if (DalamudServices.Framework != null)
				DalamudServices.Framework.Update -= this.OnFrameworkUpdateSafe;

			return;
		}

		if (!this.Services.Studio.IsOpen)
			return;

		if (this.frameworkException != null)
			return;

		try
		{
			this.OnFrameworkUpdate(framework);
		}
		catch (Exception ex)
		{
			this.frameworkException = ex;
			this.Log.Error(ex, "Error in framework update");
		}
	}
}
