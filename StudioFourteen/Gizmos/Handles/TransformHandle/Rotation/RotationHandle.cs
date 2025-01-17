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

using StudioFourteen.Gizmos.Handles;
using StudioFourteen.Gizmos.Handles.TransformHandle;
using System.Windows;

public partial class RotationHandle : TransformHandleBase
{
	private readonly RotationHandleAxis xAxis;
	private readonly RotationHandleAxis yAxis;
	private readonly RotationHandleAxis zAxis;

	public RotationHandle()
	{
		this.xAxis = new(TransformHandleAxes.X);
		this.xAxis.Foreground = this.XAxisForeground;
		this.xAxis.Background = this.XAxisBackground;
		this.AddChild(this.xAxis);

		this.yAxis = new(TransformHandleAxes.Y);
		this.yAxis.Foreground = this.YAxisForeground;
		this.yAxis.Background = this.YAxisBackground;
		this.AddChild(this.yAxis);

		this.zAxis = new(TransformHandleAxes.Z);
		this.zAxis.Foreground = this.ZAxisForeground;
		this.zAxis.Background = this.ZAxisBackground;
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
