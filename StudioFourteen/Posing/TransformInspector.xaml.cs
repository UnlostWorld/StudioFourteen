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
			BoneTransform transform = new();
			transform.Translation = value.Translation;
			transform.Rotation = value.Rotation;
			transform.Scale = value.Scale;
			this.Selection?.SetWorldTransform(transform);
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
			if (this.Selection == null)
				return;

			BoneTransform transform = new();
			transform.Rotation = value;
			this.Selection.SetWorldTransform(transform);
		}
	}
}