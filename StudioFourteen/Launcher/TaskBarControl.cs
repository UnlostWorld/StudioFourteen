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
using WpfUtils;
using WpfUtils.Extensions;

using Panel = StudioFourteen.Panels.Panel;

[DependencyProperty<bool>("IsOpen")]
[DependencyProperty<bool>("IsInGPose")]
public partial class TaskBarControl : Control
{
	private readonly Dictionary<Type, TaskBarEntry> panelEntries = new();

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

		this.IsOpen = this.Services.Studio.IsOpen;
		this.IsInGPose = this.Services.GroupPose.IsGroupPosing;
	}

	public FastObservableCollection<TaskBarEntry> Entries { get; init; } = new();

	/*public unsafe bool IsGPoseSettingsOpen
	{
		get => this.Services.GroupPose.IsGroupPoseSettingsWindowVisible();
		set => this.Services.GroupPose.SetGroupPoseSettingsWindowVisible(value);
	}*/

	protected ServiceManager Services => ServiceManager.Instance;

	private void OnStudioOpening()
	{
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

			entry = new(panel.TitleIcon, panel.Title);
			entry.Type = panel.GetType();
			this.panelEntries.Add(panel.GetType(), entry);

			this.Dispatcher.Invoke(() => this.Entries.Add(entry));
		}
		else
		{
			entry.IsMinimized = false;
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
}

public partial class TaskBarEntry(string icon, string title)
: ViewModel
{
	[Notify] private bool isMinimized = false;
	[Notify] private bool isActive = true;
	[Notify] private bool isVisible = true;

	public string? Icon { get; set; } = icon;
	public string? ToolTip { get; set; } = title;
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