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

namespace StudioFourteen.Selection;

using PropertyChanged.SourceGenerator;
using StudioFourteen.Gizmos.Handles.TransformHandle;
using StudioFourteen.Rendering.Scene.Gizmos.Transforms;

public abstract partial class TransformSelectionBase : SelectionBase
{
	private readonly TransformGizmo gizmo = new();

	[Notify][PropertyAttribute("StudioFourteen.History.History")] private Transform worldTransform;
	[Notify][PropertyAttribute("StudioFourteen.History.History")] private Transform localTransform;
	[Notify][PropertyAttribute("StudioFourteen.History.History")] private bool lockTransform;

	public virtual bool CanLockTransform => true;

	public virtual double TranslationChange => 0.1;
	public virtual int DecimalPlacesToDisplay => 2;
	public virtual TransformHandleTypes DefaultGizmo => TransformHandleTypes.Translation;
	public virtual double GizmoSensitivity => 1.0;

	public override void Activate()
	{
		base.Activate();
		this.gizmo.Enable();
	}

	public override void Deactivate()
	{
		base.Deactivate();
		this.gizmo.Disable();
	}

	public override void OnGameTick()
	{
		this.gizmo.Transform = this.WorldTransform;
		base.OnGameTick();
	}

	protected virtual void OnWorldTransformChanged(Transform oldValue, Transform newValue)
	{
	}

	protected virtual void OnLocalTransformChanged(Transform oldValue, Transform newValue)
	{
	}

	protected virtual void OnLockTransformChanged(bool oldValue, bool newValue)
	{
	}
}