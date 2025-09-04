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

namespace StudioFourteen.Launcher;

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DependencyPropertyGenerator;
using PropertyChanged.SourceGenerator;
using StudioFourteen.Panels;
using StudioFourteen.Settings;
using WpfUtils.Commands;
using Panel = StudioFourteen.Panels.Panel;

[DependencyProperty<bool>("IsOpen")]
[DependencyProperty<PanelContextBase>("Context")]
public partial class LauncherMenu : Control
{
	[Notify] private bool isStudioOpen;
	[Notify] private bool isInGPose = false;
	[Notify] private bool isGPoseSettingsOpen = false;

	public LauncherMenu()
	{
		this.Loaded += this.OnLoaded;
		this.Unloaded += this.OnUnloaded;
	}

	protected ServiceManager Services => ServiceManager.Instance;
	protected SettingsService.Configuration Configuration => this.Services.Settings.Current;

	public override void OnApplyTemplate()
	{
		base.OnApplyTemplate();
	}

	private void OnLoaded(object sender, RoutedEventArgs e)
	{
		this.Services.Studio.Opening += this.OnStudioStateChanged;
		this.Services.Studio.Closing += this.OnStudioStateChanged;
		this.Services.GroupPose.StateChanged += this.OnGroupPoseStateChanged;
		this.Services.GroupPose.SettingsStateChanged += this.OnGroupPoseSettingsStateChanged;

		this.IsInGPose = this.Services.GroupPose.IsGroupPosing;
		this.IsGPoseSettingsOpen = this.Services.GroupPose.IsGroupPoseSettingsWindowVisible;

		this.OnStudioStateChanged();
	}

	private void OnUnloaded(object sender, RoutedEventArgs e)
	{
		this.Services.Studio.Opening -= this.OnStudioStateChanged;
		this.Services.Studio.Closing -= this.OnStudioStateChanged;
		this.Services.GroupPose.StateChanged -= this.OnGroupPoseStateChanged;
		this.Services.GroupPose.SettingsStateChanged -= this.OnGroupPoseSettingsStateChanged;
	}

	private void OnStudioStateChanged()
	{
		this.Dispatcher.Invoke(() => this.IsStudioOpen = this.Services.Studio.IsOpen);
	}

	private void OnIsStudioOpenChanged(bool oldValue, bool newValue)
	{
		if (newValue)
		{
			this.Services.Studio.OpenStudio();
		}
		else
		{
			this.Services.Studio.CloseStudio();
		}
	}

	private void OnGroupPoseSettingsStateChanged(bool settingsState)
	{
		this.Dispatcher.Invoke(() => this.IsGPoseSettingsOpen = settingsState);
	}

	private void OnGroupPoseStateChanged(bool newState)
	{
		this.Dispatcher.Invoke(() => this.IsInGPose = newState);
	}

	private void OnIsInGPoseChanged(bool oldValue, bool newValue)
	{
		this.Services.GroupPose.SetGroupPose(newValue);
	}

	private void OnIsGPoseSettingsOpenChanged(bool oldValue, bool newValue)
	{
		this.Services.GroupPose.SetGroupPoseSettingsWindowVisible(newValue);
	}
}

[DependencyProperty<Type>("PanelType")]
[DependencyProperty<PanelContextBase>("Context")]
[DependencyProperty<string>("DisplayName")]
[DependencyProperty<string>("Description")]
[DependencyProperty<object>("Icon")]
[DependencyProperty<bool>("IsPanelOpen")]
[DependencyProperty<ICommand>("OpenPanel")]
public partial class LauncherEntry : Control
{
	public LauncherEntry()
	{
		this.OpenPanel = new SimpleCommand(this.OpenPanelCallback);
	}

	public ServiceManager Services => ServiceManager.Instance;

	protected override void OnTemplateChanged(ControlTemplate oldTemplate, ControlTemplate newTemplate)
	{
		base.OnTemplateChanged(oldTemplate, newTemplate);
	}

	partial void OnContextChanged(PanelContextBase? oldValue, PanelContextBase? newValue)
	{
		if (oldValue != null)
		{
			oldValue.PanelOpened -= this.OnPanelChanged;
			oldValue.PanelClosed -= this.OnPanelChanged;
			oldValue.PanelMinimized -= this.OnPanelChanged;
		}

		if (newValue != null)
		{
			newValue.PanelOpened += this.OnPanelChanged;
			newValue.PanelClosed += this.OnPanelChanged;
			newValue.PanelMinimized += this.OnPanelChanged;
		}
	}

	partial void OnPanelTypeChanged(Type? newValue)
	{
		if (newValue == null)
			return;

		this.DisplayName = this.Services.Panels.GetPanelTitle(newValue);
		this.Description = this.Services.Panels.GetPanelDescription(newValue);
		this.Icon = this.Services.Panels.GetPanelIcon(newValue);

		this.IsPanelOpen = this.Context?.GetOpenPanel(newValue) != null;
	}

	private void OnPanelChanged(Panel panel)
	{
		this.Dispatcher.BeginInvoke(() =>
		{
			if (this.PanelType == null)
				return;

			this.IsPanelOpen = this.Context?.GetOpenPanel(this.PanelType) != null;
		});
	}

	private void OpenPanelCallback()
	{
		if (this.Context == null || this.PanelType == null)
			return;

		this.Context.SetIsOpen(this.PanelType, true, true);
		this.CloseMenu();
	}

	private void CloseMenu()
	{
		LauncherMenu? menu = this.FindParent<LauncherMenu>();
		if (menu != null)
		{
			menu.IsOpen = false;
		}
	}
}