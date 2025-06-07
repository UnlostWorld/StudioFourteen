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

namespace StudioFourteen.Rendering.Scene.Gizmos.Transforms;

using System.Numerics;
using StudioFourteen.Rendering.Scene.Gizmos;
using StudioFourteen.Selection;

public abstract class TransformGizmoBase : SelectionGizmoBase<TransformSelectionBase>
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

		if (this.Selection == null)
			return;

		if (this.IsBeingManipulated)
		{
			this.Selection.WorldTransform = this.TargetTransform;
		}
		else
		{
			this.TargetTransform = this.Selection.WorldTransform;
		}
	}
}