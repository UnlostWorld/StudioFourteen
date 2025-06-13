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

namespace StudioFourteen.Rendering.Draw.Gizmos;

using StudioFourteen.Scene;

public abstract class ObjectGizmoBase : GizmoBase
{
	protected SceneObjectBase? sceneObject;

	public virtual object? Icon => null;

	public void Enable(SceneObjectBase sceneObject)
	{
		this.sceneObject = sceneObject;
		this.Enable();
	}

	public abstract bool SupportsObject(SceneObjectBase sceneObject);

	public virtual void OnGameTick()
	{
	}
}

public abstract class ObjectGizmoBase<TObjectType> : ObjectGizmoBase
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

	public override bool SupportsObject(SceneObjectBase sceneObject)
	{
		return typeof(TObjectType).IsAssignableFrom(sceneObject.GetType());
	}
}