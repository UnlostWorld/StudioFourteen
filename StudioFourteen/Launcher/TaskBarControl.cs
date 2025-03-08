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

using DependencyPropertyGenerator;
using System.Windows.Controls;
using System.Windows.Input;
using WpfUtils.Extensions;
using StudioFourteen.Panels;
using System.Collections.Generic;

using Panel = StudioFourteen.Panels.Panel;

[DependencyProperty<bool>("IsOpen")]
[DependencyProperty<bool>("IsInGPose")]
public partial class TaskBarControl : Control
{
	private readonly Dictionary<Panel, TaskBarEntry> panelEntries = new();

	public TaskBarControl()
	{
		this.Services.Studio.Opening += this.OnStudioOpening;
		this.Services.Studio.Closing += this.OnStudioClosing;
		this.Services.GroupPose.StateChanged += this.OnGroupPoseStateChanged;
		this.Services.Panels.PanelOpened += this.OnPanelOpened;
		this.Services.Panels.PanelClosed += this.OnPanelClosed;
		this.Services.Panels.PanelMinimized += this.OnPanelMinimized;

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
		this.IsInGPose = newState;
	}

	partial void OnIsInGPoseChanged(bool newValue)
	{
		this.Services.GroupPose.SetGroupPose(newValue);
	}

	private void OnPanelOpened(Panel panel)
	{
		TaskBarEntry? entry;
		if (this.panelEntries.TryGetValue(panel, out entry))
			return;
		entry = new(panel.TitleIcon.ToString(), panel.Title ?? string.Empty);
		this.panelEntries.Add(panel, entry);

		this.Dispatcher.Invoke(() => this.Entries.Add(entry));
	}

	private void OnPanelClosed(Panel panel)
	{
		TaskBarEntry? entry;
		if (!this.panelEntries.TryGetValue(panel, out entry) || entry == null)
			return;

		this.panelEntries.Remove(panel);
		this.Dispatcher.Invoke(() => this.Entries.Remove(entry));
	}

	private void OnPanelMinimized(Panel panel)
	{
	}
}

public class TaskBarEntry(string icon, string title)
{
	public string? Icon { get; set; } = icon;
	public string? ToolTip { get; set; } = title;
	public bool IsMinimized { get; set; } = false;
}

[DependencyProperty<bool>("IsMinimized")]
[DependencyProperty<object>("Icon")]
public partial class TaskBarButtonControl : Control
{
	protected override void OnMouseUp(MouseButtonEventArgs e)
	{
		base.OnMouseUp(e);

		if (e.ChangedButton == MouseButton.Left)
		{
		}
	}
}