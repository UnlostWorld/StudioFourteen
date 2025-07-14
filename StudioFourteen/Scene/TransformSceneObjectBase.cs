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

namespace StudioFourteen.Scene;

using PropertyChanged.SourceGenerator;
using StudioFourteen.Rendering.Draw.Gizmos.Transforms;

public abstract partial class TransformSceneObjectBase : SceneObjectBase
{
	private Transform? initialWorldTransform = null;

	[Notify] private Transform worldTransform;
	[Notify] private Transform localTransform;
	[Notify] private bool lockTransform;

	public virtual double TranslationChange => 0.1;
	public virtual int DecimalPlacesToDisplay => 2;
	public virtual TransformHandleTypes DefaultGizmo => TransformHandleTypes.Translation;
	public virtual double GizmoSensitivity => 1.0;

	public override void Reset()
	{
		if (this.initialWorldTransform != null)
		{
			this.WorldTransform = (Transform)this.initialWorldTransform;
		}

		base.Reset();
	}

	protected virtual void OnWorldTransformChanged(Transform oldValue, Transform newValue)
	{
		if (this.initialWorldTransform == null && oldValue != Transform.Identity && oldValue != default)
		{
			this.initialWorldTransform = oldValue;
		}
	}

	protected virtual void OnLocalTransformChanged(Transform oldValue, Transform newValue)
	{
	}

	protected virtual void OnLockTransformChanged(bool oldValue, bool newValue)
	{
	}
}