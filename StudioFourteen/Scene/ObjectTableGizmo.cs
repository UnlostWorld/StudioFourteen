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

using StudioFourteen.Posing;
using StudioFourteen.Rendering.Draw.Gizmos;
using StudioFourteen.Selection;

public class ObjectTableGizmo : GizmoGroup
{
	private readonly ObjectTableObject objectTableObject;

	public ObjectTableGizmo(ObjectTableObject objectTableObject)
	{
		this.objectTableObject = objectTableObject;

		SkeletonGizmo skeleton = new();
		skeleton.SetTarget(objectTableObject);
		this.Gizmos.Add(skeleton);

		this.Gizmos.Add(new BlankGizmo());

		this.Enable();
	}

	public override string Name => "Character";
}

public class BlankGizmo : GizmoBase
{
	public override string Name => "Blank";
}