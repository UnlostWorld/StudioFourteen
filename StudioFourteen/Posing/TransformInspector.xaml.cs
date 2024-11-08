namespace StudioFourteen.Posing;

using DependencyPropertyGenerator;
using StudioFourteen.Mvm;
using StudioFourteen.Settings;
using StudioFourteen.Structs.Extensions;
using System.Numerics;
using System.Windows.Controls;
using System.Windows.Input;

[DependencyProperty<TransformSelectionBase>("Selection")]
[DependencyProperty<Persistence>("Persistence")]
public partial class TransformInspector : View
{
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
	public Transform WorldTransform
	{
		get => this.Selection?.WorldTransform ?? default;
		set
		{
			if (this.Selection == null)
				return;

			this.Selection.WorldTransform = value;
		}
	}

	[AutoNotify]
	public Transform LocalTransform
	{
		get => this.Selection?.LocalTransform ?? default;
		set
		{
			if (this.Selection == null)
				return;

			this.Selection.LocalTransform = value;
		}
	}

	// TODO: Move this to a custom control (Like TransformControl) for the gizmos.
	[AutoNotify]
	public Quaternion WorldRotation
	{
		get => this.WorldTransform.Rotation;
		set
		{
			Transform t = this.WorldTransform;
			t.Rotation = value;
			this.WorldTransform = t;
		}
	}
}