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
using System.Windows.Controls;
using System.Windows.Input;
using DependencyPropertyGenerator;
using PropertyChanged.SourceGenerator;
using StudioFourteen.Mvm;
using StudioFourteen.Panels;
using StudioFourteen.Settings;
using WpfUtils;
using WpfUtils.Extensions;

using Panel = StudioFourteen.Panels.Panel;

[DependencyProperty<bool>("IsOpen")]
[DependencyProperty<bool>("IsInGPose")]
public partial class TaskBarControl : Control
{
	private readonly Dictionary<Type, TaskBarEntry> panelEntries = new();
	private bool hasRestoredSaves = false;

	public TaskBarControl()
	{
		this.Services.Studio.Opening += this.OnStudioOpening;
		this.Services.Studio.Closing += this.OnStudioClosing;
		this.Services.GroupPose.StateChanged += this.OnGroupPoseStateChanged;
		this.Services.Panels.PanelOpened += this.OnPanelOpened;
		this.Services.Panels.PanelClosed += this.OnPanelClosed;
		this.Services.Panels.PanelMinimized += this.OnPanelMinimized;
		this.Services.Panels.PanelActivated += this.OnPanelActivated;
		this.Services.Panels.PanelDeactivated += this.OnPanelDeactivated;

		this.IsInGPose = this.Services.GroupPose.IsGroupPosing;

		if (this.Services.Studio.IsOpen)
		{
			this.OnStudioOpening();
		}
	}

	public FastObservableCollection<TaskBarEntry> Entries { get; init; } = new();

	/*public unsafe bool IsGPoseSettingsOpen
	{
		get => this.Services.GroupPose.IsGroupPoseSettingsWindowVisible();
		set => this.Services.GroupPose.SetGroupPoseSettingsWindowVisible(value);
	}*/

	protected ServiceManager Services => ServiceManager.Instance;
	protected SettingsService.Configuration Settings => this.Services.Settings.Current;

	private void OnStudioOpening()
	{
		if (!this.hasRestoredSaves)
		{
			foreach((string typeName, TaskBarEntrySave save) in this.Settings.MinimizedTaskBarEntries)
			{
				if (save.Icon == null || save.Title == null)
					continue;

				Type? panelType = Type.GetType(typeName);
				if (panelType == null)
					continue;

				if (this.panelEntries.ContainsKey(panelType))
					continue;

				TaskBarEntry? entry = new(save.Icon, save.Title, panelType);
				entry.IsMinimized = true;
				this.panelEntries.Add(panelType, entry);

				this.Dispatcher.Invoke(() => this.Entries.Add(entry));
			}

			this.hasRestoredSaves = true;
		}

		this.Dispatcher.Invoke(() => this.IsOpen = true);
	}

	private void OnStudioClosing()
	{
		this.Dispatcher.Invoke(() => this.IsOpen = false);
	}

	private void OnGroupPoseStateChanged(bool newState)
	{
		this.Dispatcher.Invoke(() => this.IsInGPose = newState);
	}

	partial void OnIsInGPoseChanged(bool newValue)
	{
		this.Services.GroupPose.SetGroupPose(newValue);
	}

	private void OnPanelOpened(Panel panel)
	{
		TaskBarEntry? entry;
		this.panelEntries.TryGetValue(panel.GetType(), out entry);

		if (entry == null)
		{
			if (string.IsNullOrEmpty(panel.TitleIcon) || string.IsNullOrEmpty(panel.Title))
				return;

			entry = new(panel.TitleIcon, panel.Title, panel.GetType());
			this.panelEntries.Add(panel.GetType(), entry);

			this.Dispatcher.Invoke(() => this.Entries.Add(entry));
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

		string? typeName = entry.Type?.FullName;
		if (panel.RememberWindowState && typeName != null)
		{
			this.Settings.MinimizedTaskBarEntries.Add(typeName, entry.Save);
		}
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

	public TaskBarEntry(string icon, string title, Type panelType)
	{
		this.Icon = icon;
		this.Title = title;
		this.Type = panelType;
	}

	public string? Icon
	{
		get => this.Save.Icon;
		set => this.Save.Icon = value;
	}

	public string? Title
	{
		get => this.Save.Title;
		set => this.Save.Title = value;
	}

	public Type? Type { get; set; }
}

[DependencyProperty<bool>("IsMinimized")]
[DependencyProperty<object>("Icon")]
[DependencyProperty<bool>("IsTaskVisible")]
[DependencyProperty<Type>("PanelType")]
[DependencyProperty<bool>("IsActive")]
public partial class TaskBarButtonControl : Control
{
	protected override void OnMouseUp(MouseButtonEventArgs e)
	{
		base.OnMouseUp(e);

		if (e.ChangedButton == MouseButton.Left)
		{
			if (this.PanelType == null)
				return;

			ServiceManager.Instance.Panels.SetIsOpen(this.PanelType, true);
		}
	}
}