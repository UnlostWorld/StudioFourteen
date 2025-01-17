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

namespace StudioFourteen.Gizmos.Handles.TransformHandle;

using StudioFourteen.Gizmos;

using Transform = StudioFourteen.Posing.Transform;

public abstract class TransformHandleBase : GizmoGroup
{
	public double Sensitivity = 1;
	public bool WriteTransform = true;

	public TransformHandleBase()
	{
		this.KeepScreenSize = true;
	}

	public delegate void TransformChangedDelegate(Transform newTransform);

	public event TransformChangedDelegate? TransformChanged;

	public Transform OnAxisBeginDrag()
	{
		return this.Transform;
	}

	public void OnAxisDrag(Transform newTransform)
	{
		if (this.WriteTransform)
			this.Transform = newTransform;

		this.TransformChanged?.Invoke(newTransform);
	}
}