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

namespace StudioFourteen.Posing;

using StudioFourteen.Files;
using StudioFourteen.Library;
using StudioFourteen.Mvm;
using StudioFourteen.Panels;
using StudioFourteen.Selection;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

public partial class PoseWindow : CharacterPanelBase
{
	[AutoNotify] public string RevertTooltip => StudioFourteen.Resources.Format("LOC_Pose_RevertPose", this.CharacterName);

	public int SelectedTab
	{
		get => this.GetPersistence<int>();
		set => this.SetPersistence(value);
	}

	public int SelectedBodyTab
	{
		get => this.GetPersistence<int>();
		set => this.SetPersistence(value);
	}

	public int SelectedHandsTab
	{
		get => this.GetPersistence<int>();
		set => this.SetPersistence(value);
	}

	public int SelectedFaceTab
	{
		get => this.GetPersistence<int>();
		set => this.SetPersistence(value);
	}

	public bool FlipSides
	{
		get => this.GetPersistence<bool>();
		set => this.SetPersistence(value);
	}

	[AutoNotify] public SelectionBase? Selection => this.Services.Selection.Current;
	[AutoNotify] public bool IsSelectionTransform => this.Selection is TransformSelectionBase;
	[AutoNotify] public bool IsSelectionBlend => this.Selection is BlendSelection;
	[AutoNotify] public bool IsSelectionEye => this.Selection is EyeSelection;

	protected override void OnOpened()
	{
		base.OnOpened();

		if (this.Services.Selection.Current == null && this.TargetObjectIndex >= 0)
		{
			this.Services.Selection.Current = new GameObjectSelection((ushort)this.TargetObjectIndex);
		}
	}

	private void OnRevertClicked(object sender, RoutedEventArgs e)
	{
		if (this.TargetObjectIndex < 0)
			return;

		this.Services.Pose.FlushBoneReferences((ushort)this.TargetObjectIndex);
		this.Services.Selection.Current = new GameObjectSelection((ushort)this.TargetObjectIndex);
	}

	private void OnBackgroundMouseDown(object sender, MouseButtonEventArgs e)
	{
		if (this.TargetObjectIndex < 0)
			return;

		this.Services.Selection.Current = new GameObjectSelection((ushort)this.TargetObjectIndex);
	}

	private void OnClearClicked(object sender, RoutedEventArgs e)
	{
		this.Services.Selection.Current = new GameObjectSelection((ushort)this.TargetObjectIndex);
	}

	private async void OnReferenceClicked(object sender, RoutedEventArgs e)
	{
		await this.Services.Pose.SetToReferencePose(this.TargetObjectIndex);
	}

	private void OnResetSelectionClicked(object sender, RoutedEventArgs e)
	{
		if (this.Selection == null)
			return;

		this.Selection.Reset();
	}

	private void OnImportClicked(object sender, RoutedEventArgs e)
	{
		LibraryWindow.Open(LibraryWindow.PosesTab);
	}

	private async void OnExportClicked(object sender, RoutedEventArgs e)
	{
		await this.Services.Pose.ExportPose(this.TargetObjectIndex);
	}

	private void OnFlipPoseClicked(object sender, RoutedEventArgs e)
	{
		this.Services.Pose.Flip(this.TargetObjectIndex);
	}
}