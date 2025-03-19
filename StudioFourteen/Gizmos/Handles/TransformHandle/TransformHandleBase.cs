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
using System.Windows.Media;

using Transform = StudioFourteen.Transform;

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

	public Color XAxisForeground => Color.FromArgb(0xFF, 0x33, 0x33, 0xFF);
	public Color XAxisBackground => Color.FromArgb(0xFF, 0x1C, 0x1C, 0x31);
	public Color YAxisForeground => Color.FromArgb(0xFF, 0x33, 0xFF, 0x33);
	public Color YAxisBackground => Color.FromArgb(0xFF, 0x1C, 0x31, 0x1C);
	public Color ZAxisForeground => Color.FromArgb(0xFF, 0xFF, 0x33, 0x33);
	public Color ZAxisBackground => Color.FromArgb(0xFF, 0x31, 0x1C, 0x1C);

	public Color DraggingBackground => Color.FromArgb(0xFF, 0x31, 0x31, 0x1C);

	public void OnAxisBeginDrag(TransformHandleAxisBase axis)
	{
	}

	public void OnAxisEndDrag(TransformHandleAxisBase axis)
	{
	}

	public void OnAxisDrag(Transform newTransform)
	{
		if (this.WriteTransform)
			this.Transform = newTransform;

		this.TransformChanged?.Invoke(newTransform);
	}
}