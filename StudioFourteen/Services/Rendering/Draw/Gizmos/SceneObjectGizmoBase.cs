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

namespace StudioFourteen.Services.Rendering.Draw.Gizmos;

using StudioFourteen.Services.Rendering.Passes;
using StudioFourteen.Services.Scene;

public abstract class SceneObjectGizmoBase : GizmoBase
{
	protected SceneObjectBase? sceneObject;

	public void Enable(SceneObjectBase sceneObject, ForwardPass? pass = null)
	{
		if (pass == null)
			pass = Studio.Rendering.OverlayRenderer.Forward;

		this.sceneObject = sceneObject;
		this.Enable(pass);
	}

	public void SetTarget(SceneObjectBase sceneObject)
	{
		this.sceneObject = sceneObject;
	}
}

public abstract class SceneObjectGizmoBase<TObjectType> : SceneObjectGizmoBase
	where TObjectType : SceneObjectBase
{
	public TObjectType? SceneObject
	{
		get
		{
			if (this.sceneObject is TObjectType tSelection)
				return tSelection;

			return null;
		}
	}
}