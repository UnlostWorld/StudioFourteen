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

namespace StudioFourteen.Rendering.Draw.Gizmos.Transforms;

using System.Numerics;
using StudioFourteen.Rendering.Draw.Gizmos;
using StudioFourteen.Scene;

public abstract class TransformGizmoBase : SceneObjectGizmoBase<TransformSceneObjectBase>
{
	private Transform targetTransform;

	public Transform TargetTransform
	{
		get => this.targetTransform;
		set
		{
			this.targetTransform = value;
			this.Transform = Transform.FromTRS(this.TargetTransform.Translation, this.TargetTransform.Rotation, Vector3.One);
		}
	}

	public override void OnGameTick()
	{
		base.OnGameTick();

		if (this.SceneObject == null)
			return;

		if (this.IsBeingManipulated)
		{
			this.SceneObject.WorldTransform = this.TargetTransform;
		}
		else
		{
			this.TargetTransform = this.SceneObject.WorldTransform;
		}
	}
}