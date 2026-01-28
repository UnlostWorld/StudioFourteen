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

namespace StudioFourteen.Services.Rendering.Draw.Gizmos.Transforms;

using StudioFourteen.Services.Rendering.Passes;
using StudioFourteen.Services.Scene;

public class TransformGizmo : GizmoGroup
{
	public TransformGizmo()
	{
		this.AddGizmo<TranslationGizmo>();
		this.AddGizmo<RotationGizmo>();
		this.AddGizmo<ScaleGizmo>();
	}

	public override bool KeepScreenSize => true;

	public void Enable(SceneObjectBase sceneObject, ForwardPass pass)
	{
		foreach (SceneObjectGizmoBase gizmo in this.Gizmos)
		{
			gizmo.SetTarget(sceneObject);
		}

		this.Enable(pass);
	}
}