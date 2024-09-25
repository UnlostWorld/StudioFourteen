namespace ScreenshotStudio.Posing;

using ScreenshotStudio.Files;
using ScreenshotStudio.Mvm;
using ScreenshotStudio.Panels;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

public partial class PoseWindow : CharacterPanelBase
{
	[AutoNotify] public string RevertTooltip => ScreenshotStudio.Resources.Format("LOC_Pose_RevertPose", this.CharacterName);

	[AutoNotify]
	public int SelectedTab
	{
		get => this.GetPersistence<int>();
		set => this.SetPersistence(value);
	}

	[AutoNotify]
	public int SelectedFaceTab
	{
		get => this.GetPersistence<int>();
		set => this.SetPersistence(value);
	}

	[AutoNotify]
	public bool FlipSides
	{
		get => this.GetPersistence<bool>();
		set => this.SetPersistence(value);
	}

	[AutoNotify] public SelectionBase? Selection => this.Services.Pose.Selection;
	[AutoNotify] public bool IsSelectionTransform => this.Selection is TransformSelectionBase;
	[AutoNotify] public bool IsSelectionBlend => this.Selection is BlendSelection;

	protected override void OnOpened()
	{
		base.OnOpened();

		if (this.Services.Pose.Selection == null && this.TargetObjectIndex >= 0)
		{
			this.Services.Pose.Selection = new GameObjectSelection((ushort)this.TargetObjectIndex);
		}
	}

	protected override void OnTargetChanged()
	{
		base.OnTargetChanged();
	}

	private void OnRevertClicked(object sender, RoutedEventArgs e)
	{
		if (this.TargetObjectIndex < 0)
			return;

		this.Services.Pose.FlushBoneReferences((ushort)this.TargetObjectIndex);
		this.Services.Pose.Selection = new GameObjectSelection((ushort)this.TargetObjectIndex);
	}

	private void OnBackgroundMouseDown(object sender, MouseButtonEventArgs e)
	{
		if (this.TargetObjectIndex < 0)
			return;

		this.Services.Pose.Selection = new GameObjectSelection((ushort)this.TargetObjectIndex);
	}

	private void OnClearClicked(object sender, RoutedEventArgs e)
	{
		this.Services.Pose.Selection = new GameObjectSelection((ushort)this.TargetObjectIndex);
	}

	private async void OnReferenceClicked(object sender, RoutedEventArgs e)
	{
		await this.Services.Pose.SetToReferencePose(this.TargetObjectIndex);
	}

	private void OnImportClicked(object sender, RoutedEventArgs e)
	{
	}

	private async void OnExportClicked(object sender, RoutedEventArgs e)
	{
		// Mouth
		/*HashSet<string> mouthBones = new()
		{
			"j_f_umlip_01_l",
			"j_f_umlip_02_l",
			"j_f_ulip_01_l",
			"j_f_ulip_02_l",
			"j_f_uslip_l",
			"j_f_dmlip_01_l",
			"j_f_dmlip_02_l",
			"j_f_dlip_01_l",
			"j_f_dlip_02_l",
			"j_f_dslip_l",
			"j_f_dmemoto_l",
			"j_f_shoho_l",

			"j_f_umlip_01_r",
			"j_f_umlip_02_r",
			"j_f_ulip_01_r",
			"j_f_ulip_02_r",
			"j_f_uslip_r",
			"j_f_dmlip_01_r",
			"j_f_dmlip_02_r",
			"j_f_dlip_01_r",
			"j_f_dlip_02_r",
			"j_f_dslip_r",
			"j_f_dmemoto_r",
			"j_f_shoho_r",

			"j_f_ago",

			"j_f_bero_01",
			"j_f_bero_02",
			"j_f_bero_03",
		};*/

		PoseFile file = new();
		await file.Save(this.TargetObjectIndex);
		this.Services.Files.SaveFile(file, $"{this.CharacterName}'s Pose");
	}
}