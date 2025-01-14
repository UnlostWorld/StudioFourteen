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

namespace StudioFourteen.Overlays.Gizmos.Scale;

using StudioFourteen.Overlays.Gizmos;
using System.Windows.Media;

public partial class ScaleGizmo : GizmoBase
{
	private readonly ScaleGizmoAxis xAxis;
	private readonly ScaleGizmoAxis yAxis;
	private readonly ScaleGizmoAxis zAxis;
	private readonly ScaleGizmoAxis xNegAxis;
	private readonly ScaleGizmoAxis yNegAxis;
	private readonly ScaleGizmoAxis zNegAxis;
	private readonly UniformScaleGizmoAxis uniformAxis;

	public ScaleGizmo()
	{
		this.xAxis = new(GizmoAxes.X, this.Radius, false);
		this.xAxis.Foreground = Color.FromArgb(0xFF, 0x33, 0x33, 0xFF);
		this.xAxis.Background = Color.FromArgb(0x10, 0x33, 0x33, 0xFF);
		this.AddAxis(this.xAxis);

		this.yAxis = new(GizmoAxes.Y, this.Radius, false);
		this.yAxis.Foreground = Color.FromArgb(0xFF, 0x33, 0xFF, 0x33);
		this.yAxis.Background = Color.FromArgb(0x10, 0x33, 0xFF, 0x33);
		this.AddAxis(this.yAxis);

		this.zAxis = new(GizmoAxes.Z, this.Radius, false);
		this.zAxis.Foreground = Color.FromArgb(0xFF, 0xFF, 0x33, 0x33);
		this.zAxis.Background = Color.FromArgb(0x10, 0xFF, 0x33, 0x33);
		this.AddAxis(this.zAxis);

		this.xNegAxis = new(GizmoAxes.X, this.Radius, true);
		this.xNegAxis.Foreground = Color.FromArgb(0xFF, 0x33, 0x33, 0xFF);
		this.xNegAxis.Background = Color.FromArgb(0x10, 0x33, 0x33, 0xFF);
		this.AddAxis(this.xNegAxis);

		this.yNegAxis = new(GizmoAxes.Y, this.Radius, true);
		this.yNegAxis.Foreground = Color.FromArgb(0xFF, 0x33, 0xFF, 0x33);
		this.yNegAxis.Background = Color.FromArgb(0x10, 0x33, 0xFF, 0x33);
		this.AddAxis(this.yNegAxis);

		this.zNegAxis = new(GizmoAxes.Z, this.Radius, true);
		this.zNegAxis.Foreground = Color.FromArgb(0xFF, 0xFF, 0x33, 0x33);
		this.zNegAxis.Background = Color.FromArgb(0x10, 0xFF, 0x33, 0x33);
		this.AddAxis(this.zNegAxis);

		this.uniformAxis = new(this.UniformRadius);
		this.uniformAxis.Foreground = Color.FromArgb(0xFF, 0x83, 0x83, 0x83);
		this.uniformAxis.Background = Color.FromArgb(0x83, 0x83, 0x83, 0x83);
		this.AddAxis(this.uniformAxis);
	}

	public float UniformRadius { get; set; } = 30;
	public float Radius { get; set; } = 0.4f;
}