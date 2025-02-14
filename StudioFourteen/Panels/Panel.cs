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
using StudioFourteen.Mvm;
using StudioFourteen.Plugin;
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

[DependencyProperty<IconChar>("TitleIcon")]
[DependencyProperty<string>("Title")]
[DependencyProperty<string>("Subtitle")]
[DependencyProperty<SizeToContent>("SizeToContent", DefaultValue =SizeToContent.Manual)]
[DependencyProperty<ResizeMode>("ResizeMode", DefaultValue =ResizeMode.CanResizeWithGrip)]
[DependencyProperty<PanelVisibility>("VisibilityMode", DefaultValue = PanelVisibility.WithUI)]
[DependencyProperty<Style>("HostStyle")]
[DependencyProperty<Point>("DefaultPosition", DefaultValueExpression= "new System.Windows.Point(0.5, 0.5)")]
public partial class Panel : ContentControl, IAutoNotify
{
	protected readonly ILogger Log;

	private readonly string panelId;

	private Exception? frameworkException;
	private IHost? host;

	private bool isVisible;

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
		Task CloseAsync();
	}

	public ServiceManager Services => ServiceManager.Instance;
	public bool RememberWindowState { get; set; } = true;
	public Persistence Persistence { get; init; }

	public T? GetPersistence<T>([CallerMemberName] string id = "", T? defaultValue = default) => this.Persistence.GetPersistence<T>(id, defaultValue);
	public void SetPersistence(object? value, [CallerMemberName] string id = "") => this.Persistence.SetPersistence(value, id);
	public void SetPersistence(string id, object? value) => this.Persistence.SetPersistence(id, value);

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

	public void Close()
	{
		this.CloseAsync().Run();
	}

	public Task CloseAsync()
	{
		if (this.host == null)
			return Task.CompletedTask;

		return this.host.CloseAsync();
	}

	public void SetIsOpen(IHost sender, bool isOpen)
	{
		if (this.host != sender)
			throw new InvalidOperationException();

		if (isOpen)
		{
			this.OnOpened();
		}
		else
		{
			this.OnClosed();
		}
	}

	protected virtual void OnOpened()
	{
		this.Services.Panels.OnPanelOpened(this);

		if (DalamudServices.Framework != null)
			DalamudServices.Framework.Update += this.OnFrameworkUpdateSafe;

		AutoPropertyNotifyService.Register(this);
	}

	protected virtual void OnClosed()
	{
		this.Services.Panels.OnPanelClosed(this);

		if (DalamudServices.Framework != null)
			DalamudServices.Framework.Update -= this.OnFrameworkUpdateSafe;

		AutoPropertyNotifyService.Remove(this);
	}

	protected virtual void OnFrameworkUpdate(IFramework framework)
	{
	}

	private void OnFrameworkUpdateSafe(IFramework framework)
	{
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
