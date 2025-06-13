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
using PropertyChanged.SourceGenerator;
using StudioFourteen.Gizmos.Handles.TransformHandle;
using StudioFourteen.History;
using StudioFourteen.Mvm;
using StudioFourteen.Scene;
using StudioFourteen.Selection;
using StudioFourteen.Settings;

[DependencyProperty<SceneObjectBase>("Selection")]
[DependencyProperty<Persistence>("Persistence")]
public partial class TransformInspector : View
{
	[Notify] private int gizmoIndex = 0;
	[Notify] private TransformHandleTypes gizmo = TransformHandleTypes.Translation;

	public TransformSceneObjectBase? TransformSelection => this.Selection as TransformSceneObjectBase;

	[AutoNotify]
	public int DecimalPlacesDisplay => this.TransformSelection?.DecimalPlacesToDisplay ?? 2;

	[AutoNotify]
	public bool ExpandTranslationSliders
	{
		get
		{
			return this.Persistence?.GetPersistence<bool>(
				$"ExpandTranslationSliders_{this.Gizmo}",
				this.Gizmo == TransformHandleTypes.Translation) ?? false;
		}

		set => this.Persistence?.SetPersistence(value, $"ExpandTranslationSliders_{this.Gizmo}");
	}

	[AutoNotify]
	public bool ExpandRotationSliders
	{
		get
		{
			return this.Persistence?.GetPersistence<bool>(
				$"ExpandRotationSliders_{this.Gizmo}",
				this.Gizmo == TransformHandleTypes.Rotation) ?? false;
		}

		set => this.Persistence?.SetPersistence(value, $"ExpandRotationSliders_{this.Gizmo}");
	}

	[AutoNotify]
	public bool ExpandScaleSliders
	{
		get
		{
			return this.Persistence?.GetPersistence<bool>(
				$"ExpandScaleSliders_{this.Gizmo}",
				this.Gizmo == TransformHandleTypes.Scale) ?? false;
		}

		set => this.Persistence?.SetPersistence(value, $"ExpandScaleSliders_{this.Gizmo}");
	}

	[AutoNotify]
	public Transform WorldTransform
	{
		get => this.TransformSelection?.WorldTransform ?? default;
		set
		{
			if (this.TransformSelection == null)
				return;

			HistoryService.Record(this.TransformSelection, "Change World Transform");
			this.TransformSelection.WorldTransform = value;
		}
	}

	[AutoNotify]
	public Transform LocalTransform
	{
		get => this.TransformSelection?.LocalTransform ?? default;
		set
		{
			if (this.TransformSelection == null)
				return;

			HistoryService.Record(this.TransformSelection, "Change Local Transform");
			this.TransformSelection.LocalTransform = value;
		}
	}

	protected void OnGizmoIndexChanged(int oldIndex, int newIndex)
	{
		this.Gizmo = (TransformHandleTypes)newIndex;
	}
}