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

namespace StudioFourteen.Gizmos.Handles.TransformHandle.Rotation;

using System.Windows;
using System.Windows.Media;
using StudioFourteen.Gizmos;
using StudioFourteen.Gizmos.Handles;
using StudioFourteen.Gizmos.Handles.TransformHandle;

public partial class RotationHandle : TransformHandleBase
{
	private readonly EllipseGizmo sphere;
	private readonly RotationHandleAxis xAxis;
	private readonly RotationHandleAxis yAxis;
	private readonly RotationHandleAxis zAxis;

	public RotationHandle()
	{
		this.sphere = new();
		this.sphere.Foreground = Color.FromArgb(0x80, 0, 0, 0);
		this.sphere.Radius = 75;
		this.AddChild(this.sphere);

		this.xAxis = new(TransformHandleAxes.X);
		this.xAxis.Foreground = Color.FromArgb(0xFF, 0x33, 0x33, 0xFF);
		this.xAxis.Background = Color.FromArgb(0xFF, 0x33, 0x33, 0x4D);
		this.AddChild(this.xAxis);

		this.yAxis = new(TransformHandleAxes.Y);
		this.yAxis.Foreground = Color.FromArgb(0xFF, 0x33, 0xFF, 0x33);
		this.yAxis.Background = Color.FromArgb(0xFF, 0x33, 0x4D, 0x33);
		this.AddChild(this.yAxis);

		this.zAxis = new(TransformHandleAxes.Z);
		this.zAxis.Foreground = Color.FromArgb(0xFF, 0xFF, 0x33, 0x33);
		this.zAxis.Background = Color.FromArgb(0xFF, 0x4D, 0x33, 0x33);
		this.AddChild(this.zAxis);
	}

	public override void HitTest(Point mousePos, ref HandleHitResult result)
	{
		double closestAxisPointToMouseDistance = 20;
		RotationHandleAxis? closestMouseAxis = null;

		this.xAxis.CheckAxisForMouseHover(
			mousePos,
			ref closestAxisPointToMouseDistance,
			ref closestMouseAxis);

		this.yAxis.CheckAxisForMouseHover(
			mousePos,
			ref closestAxisPointToMouseDistance,
			ref closestMouseAxis);

		this.zAxis.CheckAxisForMouseHover(
			mousePos,
			ref closestAxisPointToMouseDistance,
			ref closestMouseAxis);

		result.Handle = closestMouseAxis;
	}
}
