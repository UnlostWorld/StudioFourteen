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
using PropertyChanged.SourceGenerator;
using StudioFourteen.Panels;
using StudioFourteen.Rendering.Scene.Gizmos;
using StudioFourteen.Selection;
using WpfUtils.Extensions;

public partial class ToolBarPanel : Panel
{
	[Notify] private bool isInGPose = false;
	[Notify] private bool isGPoseSettingsOpen = false;
	[Notify] private bool allowMouseCapture = true;
	[Notify] private SelectionGizmoBase? selectedGizmo = null;

	public FastObservableCollection<SelectionGizmoBase> Gizmos { get; init; } = new();

	protected override void OnOpened()
	{
		base.OnOpened();

		this.Services.GroupPose.StateChanged += this.OnGroupPoseStateChanged;
		this.Services.GroupPose.SettingsStateChanged += this.OnGroupPoseSettingsStateChanged;
		this.Services.Settings.SettingChanged += this.OnSettingsOpenChanged;
		this.Services.Selection.SelectionChanged += this.OnSelectionChanged;
		this.Services.Selection.GizmoChanged += this.OnSelectionGizmoChanged;

		this.IsInGPose = this.Services.GroupPose.IsGroupPosing;
		this.IsGPoseSettingsOpen = this.Services.GroupPose.IsGroupPoseSettingsWindowVisible;
		this.AllowMouseCapture = this.Settings.AllowMouseCapture;

		this.OnSelectionChanged(null, this.Services.Selection.Current);
		this.OnSelectionGizmoChanged(null, this.Services.Selection.Gizmo);
	}

	protected override void OnClosed()
	{
		base.OnClosed();

		this.Services.GroupPose.StateChanged -= this.OnGroupPoseStateChanged;
		this.Services.GroupPose.SettingsStateChanged -= this.OnGroupPoseSettingsStateChanged;
		this.Services.Settings.SettingChanged -= this.OnSettingsOpenChanged;
		this.Services.Selection.SelectionChanged -= this.OnSelectionChanged;
		this.Services.Selection.GizmoChanged -= this.OnSelectionGizmoChanged;
	}

	private void OnSelectionChanged(SelectionBase? oldSelection, SelectionBase? newSelection)
	{
		List<SelectionGizmoBase> gizmos = this.Services.Selection.GetValidGizmos();

		this.Dispatcher.Invoke(() =>
		{
			this.Gizmos.Replace(gizmos);
		});
	}

	private void OnSelectionGizmoChanged(SelectionGizmoBase? oldGizmo, SelectionGizmoBase? newGizmo)
	{
		this.SelectedGizmo = newGizmo;
	}

	private void OnSelectedGizmoChanged(SelectionGizmoBase? oldGizmo, SelectionGizmoBase? newGizmo)
	{
		this.Services.Selection.Gizmo = newGizmo;
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

	private void OnAllowMouseCaptureChanged(bool oldValue, bool newValue)
	{
		this.Settings.AllowMouseCapture = newValue;
	}

	private void OnSettingsOpenChanged(string settingName, object? newValue)
	{
		this.Dispatcher.Invoke(() =>
		{
			this.AllowMouseCapture = this.Settings.AllowMouseCapture;
		});
	}
}