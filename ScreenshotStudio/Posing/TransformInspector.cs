namespace ScreenshotStudio.Posing;

using DependencyPropertyGenerator;
using ScreenshotStudio.Mvm;
using ScreenshotStudio.Settings;
using ScreenshotStudio.Structs.Extensions;
using System;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

[DependencyProperty<TransformSelectionBase>("Selection")]
[DependencyProperty<Persistence>("Persistence")]
public partial class TransformInspector : View
{
	private Vector3? trackingEuler;
	private Quaternion lastWorldRotation = Quaternion.Identity;

	public PoseEditModes[] EditModes => Enum.GetValues<PoseEditModes>();

	[AutoNotify]
	public int DecimalPlacesDisplay => this.Selection?.DecimalPlacesToDisplay ?? 2;

	[AutoNotify]
	public bool ExpandTranslationSliders
	{
		get
		{
			return this.Persistence?.GetPersistence<bool>(
				$"ExpandTranslationSliders_{this.Services.Pose.EditMode}",
				this.Services.Pose.EditMode == PoseEditModes.Translation) ?? false;
		}

		set => this.Persistence?.SetPersistence(value, $"ExpandTranslationSliders_{this.Services.Pose.EditMode}");
	}

	[AutoNotify]
	public bool ExpandRotationSliders
	{
		get
		{
			return this.Persistence?.GetPersistence<bool>(
				$"ExpandRotationSliders_{this.Services.Pose.EditMode}",
				this.Services.Pose.EditMode == PoseEditModes.Rotation) ?? false;
		}

		set => this.Persistence?.SetPersistence(value, $"ExpandRotationSliders_{this.Services.Pose.EditMode}");
	}

	[AutoNotify]
	public bool ExpandScaleSliders
	{
		get
		{
			return this.Persistence?.GetPersistence<bool>(
				$"ExpandScaleSliders_{this.Services.Pose.EditMode}",
				this.Services.Pose.EditMode == PoseEditModes.Scale) ?? false;
		}

		set => this.Persistence?.SetPersistence(value, $"ExpandScaleSliders_{this.Services.Pose.EditMode}");
	}

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
		get
		{
			if (this.Selection != null && this.Selection.IsReady)
				this.lastWorldRotation = this.Selection.WorldRotation;

			return this.lastWorldRotation;
		}
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

	private void OnEulerDown(object sender, MouseButtonEventArgs e)
	{
		this.trackingEuler = this.LocalRotation.ToEuler();
	}

	private void OnEulerUp(object sender, MouseButtonEventArgs e)
	{
		this.trackingEuler = null;
	}
}
