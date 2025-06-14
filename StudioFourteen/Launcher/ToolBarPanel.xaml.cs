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
using System.Numerics;
using PropertyChanged.SourceGenerator;
using StudioFourteen.Panels;
using StudioFourteen.Rendering.Draw.Gizmos;
using StudioFourteen.Scene;
using StudioFourteen.Selection;
using StudioFourteen.Utilities;
using WpfUtils.Extensions;

public partial class ToolBarPanel : Panel
{
	[Notify] private bool allowMouseCapture = true;
	[Notify] private ObjectGizmoBase? selectedGizmo = null;

	public FastObservableCollection<ObjectGizmoBase> Gizmos { get; init; } = new();

	protected override void OnOpened()
	{
		base.OnOpened();

		this.Services.Settings.SettingChanged += this.OnSettingsOpenChanged;
		this.Services.Selection.SelectionChanged += this.OnSelectionChanged;
		this.Services.Selection.GizmoChanged += this.OnSelectionGizmoChanged;
		this.AllowMouseCapture = this.Settings.AllowMouseCapture;

		this.OnSelectionChanged(null, this.Services.Selection.Current, null);
		this.OnSelectionGizmoChanged(null, this.Services.Selection.Gizmo);
	}

	protected override void OnClosed()
	{
		base.OnClosed();

		this.Services.Settings.SettingChanged -= this.OnSettingsOpenChanged;
		this.Services.Selection.SelectionChanged -= this.OnSelectionChanged;
		this.Services.Selection.GizmoChanged -= this.OnSelectionGizmoChanged;
	}

	private void OnSelectionChanged(SceneObjectBase? oldSelection, SceneObjectBase? newSelection, object? source)
	{
		List<ObjectGizmoBase> gizmos = this.Services.Selection.GetValidGizmos();

		this.Dispatcher.Invoke(() =>
		{
			this.Gizmos.Replace(gizmos);
		});
	}

	private void OnSelectionGizmoChanged(ObjectGizmoBase? oldGizmo, ObjectGizmoBase? newGizmo)
	{
		this.SelectedGizmo = newGizmo;
	}

	private void OnSelectedGizmoChanged(ObjectGizmoBase? oldGizmo, ObjectGizmoBase? newGizmo)
	{
		this.Services.Selection.Gizmo = newGizmo;
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