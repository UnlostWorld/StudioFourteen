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
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DependencyPropertyGenerator;
using PropertyChanged.SourceGenerator;
using StudioFourteen.Icons;
using StudioFourteen.Mvm;
using StudioFourteen.Panels;
using StudioFourteen.Settings;
using WpfUtils;
using WpfUtils.Extensions;

using Panel = StudioFourteen.Panels.Panel;

[DependencyProperty<bool>("IsOpen")]
[DependencyProperty<bool>("IsInGPose")]
[DependencyProperty<bool>("IsGPoseSettingsOpen")]
[DependencyProperty<PanelContextBase>("Context")]
[DependencyProperty<bool>("HideBackground")]
[DependencyProperty<bool>("AllowMouseCapture")]
public partial class TaskBarControl : Control
{
	private readonly Dictionary<Type, TaskBarEntry> panelEntries = new();

	public TaskBarControl()
	{
		this.Services.Studio.Opening += this.OnStudioOpening;
		this.Services.Studio.Closing += this.OnStudioClosing;
		this.Services.GroupPose.StateChanged += this.OnGroupPoseStateChanged;
		this.Services.GroupPose.SettingsStateChanged += this.OnGroupPoseSettingsStateChanged;
		this.Services.Settings.SettingChanged += this.OnSettingsOpenChanged;

		this.IsInGPose = this.Services.GroupPose.IsGroupPosing;
		this.IsGPoseSettingsOpen = this.Services.GroupPose.IsGroupPoseSettingsWindowVisible;
		this.Loaded += this.OnLoaded;
	}

	public FastObservableCollection<TaskBarEntry> Entries { get; init; } = new();

	protected ServiceManager Services => ServiceManager.Instance;
	protected SettingsService.Configuration Settings => this.Services.Settings.Current;

	partial void OnContextChanged(PanelContextBase? oldValue, PanelContextBase? newValue)
	{
		if (oldValue != null)
		{
			oldValue.PanelOpened += this.OnPanelOpened;
			oldValue.PanelClosed += this.OnPanelClosed;
			oldValue.PanelMinimized += this.OnPanelMinimized;
			oldValue.PanelActivated += this.OnPanelActivated;
			oldValue.PanelDeactivated += this.OnPanelDeactivated;
		}

		if (newValue != null)
		{
			newValue.PanelOpened += this.OnPanelOpened;
			newValue.PanelClosed += this.OnPanelClosed;
			newValue.PanelMinimized += this.OnPanelMinimized;
			newValue.PanelActivated += this.OnPanelActivated;
			newValue.PanelDeactivated += this.OnPanelDeactivated;
		}
	}

	private void OnLoaded(object sender, RoutedEventArgs e)
	{
		if (this.Services.Studio.IsOpen)
		{
			this.OnStudioOpening();
		}

		this.AllowMouseCapture = this.Settings.AllowMouseCapture;
	}

	private void OnStudioOpening()
	{
		this.Dispatcher.Invoke(() => this.IsOpen = true);
	}

	private void OnStudioClosing()
	{
		this.Dispatcher.Invoke(() => this.IsOpen = false);
	}

	private void OnGroupPoseSettingsStateChanged(bool settingsState)
	{
		this.Dispatcher.Invoke(() => this.IsGPoseSettingsOpen = settingsState);
	}

	private void OnGroupPoseStateChanged(bool newState)
	{
		this.Dispatcher.Invoke(() => this.IsInGPose = newState);
	}

	partial void OnIsInGPoseChanged(bool newValue)
	{
		this.Services.GroupPose.SetGroupPose(newValue);
	}

	partial void OnIsGPoseSettingsOpenChanged(bool newValue)
	{
		this.Services.GroupPose.SetGroupPoseSettingsWindowVisible(newValue);
	}

	private void OnPanelOpened(Panel panel)
	{
		this.OnPanelOpenedAsync(panel).Run();
	}

	private async Task OnPanelOpenedAsync(Panel panel)
	{
		await this.MainThread();

		PanelContextBase? context = this.Context;
		if (panel.GetContext() != context)
			return;

		TaskBarEntry? entry;
		this.panelEntries.TryGetValue(panel.GetType(), out entry);

		if (entry == null)
		{
			await panel.MainThread();
			if (panel.TitleIcon == null || string.IsNullOrEmpty(panel.Title))
				return;

			entry = new(context, panel.TitleIcon, panel.Title, panel.GetType());
			this.panelEntries.Add(panel.GetType(), entry);

			await this.MainThread();
			this.Entries.Add(entry);
		}
		else
		{
			entry.IsMinimized = false;
		}

		string? typeName = entry.Type?.FullName;
		if (typeName != null)
		{
			this.Settings.MinimizedTaskBarEntries.Remove(typeName);
		}
	}

	private void OnPanelClosed(Panel panel)
	{
		TaskBarEntry? entry;
		if (!this.panelEntries.TryGetValue(panel.GetType(), out entry) || entry == null)
			return;

		this.panelEntries.Remove(panel.GetType());
		this.RemoveEntry(entry).Run();
	}

	private void OnPanelMinimized(Panel panel)
	{
		TaskBarEntry? entry;
		if (!this.panelEntries.TryGetValue(panel.GetType(), out entry) || entry == null)
			return;

		entry.IsMinimized = true;
	}

	private void OnPanelDeactivated(Panel panel)
	{
		TaskBarEntry? entry;
		if (!this.panelEntries.TryGetValue(panel.GetType(), out entry) || entry == null)
			return;

		entry.IsActive = false;
	}

	private void OnPanelActivated(Panel panel)
	{
		TaskBarEntry? entry;
		if (!this.panelEntries.TryGetValue(panel.GetType(), out entry) || entry == null)
			return;

		entry.IsActive = true;
	}

	private async Task RemoveEntry(TaskBarEntry entry)
	{
		await this.MainThread();
		entry.IsVisible = false;
		await Task.Delay(250);
		this.Entries.Remove(entry);
	}

	private void OnSettingsOpenChanged(string settingName, object? newValue)
	{
		this.AllowMouseCapture = this.Settings.AllowMouseCapture;
	}

	partial void OnAllowMouseCaptureChanged(bool oldValue, bool newValue)
	{
		this.Settings.AllowMouseCapture = newValue;
	}
}

public partial class TaskBarEntrySave
{
	public string? Icon { get; set; }
	public string? Title { get; set; }
}

public partial class TaskBarEntry : ViewModel
{
	public readonly TaskBarEntrySave Save = new();

	[Notify] private bool isMinimized = false;
	[Notify] private bool isActive = true;
	[Notify] private bool isVisible = true;

	public TaskBarEntry(PanelContextBase context, IconDefinitionBase icon, string title, Type panelType)
	{
		this.Icon = icon;
		this.Title = title;
		this.Type = panelType;
		this.Context = context;
	}

	public IconDefinitionBase? Icon
	{
		get => IconDefinitionBase.Parse(this.Save.Icon);
		set => this.Save.Icon = value?.ToString();
	}

	public string? Title
	{
		get => this.Save.Title;
		set => this.Save.Title = value;
	}

	public Type? Type { get; set; }
	public PanelContextBase Context { get; init; }
}

[DependencyProperty<bool>("IsMinimized")]
[DependencyProperty<object>("Icon")]
[DependencyProperty<bool>("IsTaskVisible")]
[DependencyProperty<Type>("PanelType")]
[DependencyProperty<bool>("IsActive")]
[DependencyProperty<PanelContextBase>("Context")]
public partial class TaskBarButtonControl : Control
{
	protected ServiceManager Services => ServiceManager.Instance;

	protected override void OnMouseUp(MouseButtonEventArgs e)
	{
		base.OnMouseUp(e);

		if (e.ChangedButton == MouseButton.Left)
		{
			this.HandleClickAsync().Run();
		}
	}

	private async Task HandleClickAsync()
	{
		if (this.PanelType == null || this.Context == null)
			return;

		await this.Context.TogglePanel(this.PanelType);
	}
}