namespace ScreenshotStudio.Posing;
using ScreenshotStudio.Mvm;
using ScreenshotStudio.Panels;
using ScreenshotStudio.Structs.Extensions;
using System;
using System.Numerics;
using System.Windows;
using System.Windows.Input;

public partial class PoseWindow : CharacterPanelBase
{
	private Vector3? trackingEuler;

	public PoseEditModes[] EditModes => Enum.GetValues<PoseEditModes>();

	[AutoNotify] public string RevertTooltip => ScreenshotStudio.Resources.Format("LOC_Pose_RevertPose", this.CharacterName);

	[AutoNotify]
	public bool ExpandTranslationSliders
	{
		get
		{
			return this.GetPersistence<bool>(
				$"ExpandTranslationSliders_{this.Services.Pose.EditMode}",
				this.Services.Pose.EditMode == PoseEditModes.Translation);
		}

		set => this.SetPersistence(value, $"ExpandTranslationSliders_{this.Services.Pose.EditMode}");
	}

	[AutoNotify]
	public bool ExpandRotationSliders
	{
		get
		{
			return this.GetPersistence<bool>(
				$"ExpandRotationSliders_{this.Services.Pose.EditMode}",
				this.Services.Pose.EditMode == PoseEditModes.Rotation);
		}

		set => this.SetPersistence(value, $"ExpandRotationSliders_{this.Services.Pose.EditMode}");
	}

	[AutoNotify]
	public bool ExpandScaleSliders
	{
		get
		{
			return this.GetPersistence<bool>(
				$"ExpandScaleSliders_{this.Services.Pose.EditMode}",
				this.Services.Pose.EditMode == PoseEditModes.Scale);
		}

		set => this.SetPersistence(value, $"ExpandScaleSliders_{this.Services.Pose.EditMode}");
	}

	[AutoNotify]
	public bool FlipSides
	{
		get => this.GetPersistence<bool>();
		set => this.SetPersistence(value);
	}

	[AutoNotify] public SelectionBase? Selection => this.Services.Pose.Selection;

	[AutoNotify]
	public double TranslationX
	{
		get => this.LocalTranslation.X;
		set
		{
			Vector3 translation = this.LocalTranslation;
			translation.X = (float)value;
			this.LocalTranslation = translation;
		}
	}

	[AutoNotify]
	public double TranslationY
	{
		get => this.LocalTranslation.Y;
		set
		{
			Vector3 translation = this.LocalTranslation;
			translation.Y = (float)value;
			this.LocalTranslation = translation;
		}
	}

	[AutoNotify]
	public double TranslationZ
	{
		get => this.LocalTranslation.Z;
		set
		{
			Vector3 translation = this.LocalTranslation;
			translation.Z = (float)value;
			this.LocalTranslation = translation;
		}
	}

	[AutoNotify]
	public double EulerRotationX
	{
		get => this.EulerRotation.X;
		set
		{
			Vector3 euler = this.EulerRotation;
			euler.X = (float)value;
			this.EulerRotation = euler;
		}
	}

	[AutoNotify]
	public double EulerRotationY
	{
		get => this.EulerRotation.Y;
		set
		{
			Vector3 euler = this.EulerRotation;
			euler.Y = (float)value;
			this.EulerRotation = euler;
		}
	}

	[AutoNotify]
	public double EulerRotationZ
	{
		get => this.EulerRotation.Z;
		set
		{
			Vector3 euler = this.EulerRotation;
			euler.Z = (float)value;
			this.EulerRotation = euler;
		}
	}

	public Vector3 EulerRotation
	{
		get
		{
			if (this.trackingEuler != null)
				return (Vector3)this.trackingEuler;

			return this.LocalRotation.ToEuler();
		}

		set
		{
			this.trackingEuler = value;

			Quaternion rotation = this.LocalRotation;
			rotation.FromEuler(value);
			this.LocalRotation = rotation;
		}
	}

	[AutoNotify]
	public double ScaleX
	{
		get => this.LocalScale.X;
		set
		{
			Vector3 scale = this.LocalScale;
			scale.X = (float)value;
			this.LocalScale = scale;
		}
	}

	[AutoNotify]
	public double ScaleY
	{
		get => this.LocalScale.Y;
		set
		{
			Vector3 scale = this.LocalScale;
			scale.Y = (float)value;
			this.LocalScale = scale;
		}
	}

	[AutoNotify]
	public double ScaleZ
	{
		get => this.LocalScale.Z;
		set
		{
			Vector3 scale = this.LocalScale;
			scale.Z = (float)value;
			this.LocalScale = scale;
		}
	}

	[AutoNotify]
	public Vector3 LocalTranslation
	{
		get => this.Selection?.LocalTranslation ?? default;
		set
		{
			if (this.Selection == null)
				return;

			this.Selection.LocalTranslation = value;
		}
	}

	[AutoNotify]
	public Quaternion LocalRotation
	{
		get => this.Selection?.LocalRotation ?? default;
		set
		{
			if (this.Selection == null)
				return;

			this.Selection.LocalRotation = value;
		}
	}

	[AutoNotify]
	public Vector3 LocalScale
	{
		get => this.Selection?.LocalScale ?? default;
		set
		{
			if (this.Selection == null)
				return;

			this.Selection.LocalScale = value;
		}
	}

	[AutoNotify]
	public Vector3 WorldTranslation
	{
		get => this.Selection?.WorldTranslation ?? default;
		set
		{
			if (this.Selection == null)
				return;

			this.Selection.WorldTranslation = value;
		}
	}

	[AutoNotify]
	public Quaternion WorldRotation
	{
		get => this.Selection?.WorldRotation ?? default;
		set
		{
			if (this.Selection == null)
				return;

			this.Selection.WorldRotation = value;
		}
	}

	[AutoNotify]
	public Vector3 WorldScale
	{
		get => this.Selection?.WorldScale ?? default;
		set
		{
			if (this.Selection == null)
				return;

			this.Selection.WorldScale = value;
		}
	}

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

	private void OnEulerDown(object sender, MouseButtonEventArgs e)
	{
		this.trackingEuler = this.LocalRotation.ToEuler();
	}

	private void OnEulerUp(object sender, MouseButtonEventArgs e)
	{
		this.trackingEuler = null;
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

	private void OnExportClicked(object sender, RoutedEventArgs e)
	{
	}
}