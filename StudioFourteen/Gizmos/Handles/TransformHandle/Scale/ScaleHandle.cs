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

namespace StudioFourteen.Gizmos.Handles.TransformHandle.Scale;

using StudioFourteen.Gizmos.Handles.TransformHandle;
using System.Windows.Media;

public partial class ScaleHandle : TransformHandleBase
{
	private readonly ScaleHandleAxis xAxis;
	private readonly ScaleHandleAxis yAxis;
	private readonly ScaleHandleAxis zAxis;
	private readonly ScaleHandleAxis xNegAxis;
	private readonly ScaleHandleAxis yNegAxis;
	private readonly ScaleHandleAxis zNegAxis;
	private readonly UniformScaleHandleAxis uniformAxis;

	public ScaleHandle()
	{
		this.xAxis = new(TransformHandleAxes.X, this.Radius, false);
		this.xAxis.Foreground = this.XAxisForeground;
		this.xAxis.Background = this.XAxisBackground;
		this.AddChild(this.xAxis);

		this.yAxis = new(TransformHandleAxes.Y, this.Radius, false);
		this.yAxis.Foreground = this.YAxisForeground;
		this.yAxis.Background = this.YAxisBackground;
		this.AddChild(this.yAxis);

		this.zAxis = new(TransformHandleAxes.Z, this.Radius, false);
		this.zAxis.Foreground = this.ZAxisForeground;
		this.zAxis.Background = this.ZAxisBackground;
		this.AddChild(this.zAxis);

		this.xNegAxis = new(TransformHandleAxes.X, this.Radius, true);
		this.xNegAxis.Foreground = this.XAxisForeground;
		this.xNegAxis.Background = this.XAxisBackground;
		this.AddChild(this.xNegAxis);

		this.yNegAxis = new(TransformHandleAxes.Y, this.Radius, true);
		this.yNegAxis.Foreground = this.YAxisForeground;
		this.yNegAxis.Background = this.YAxisBackground;
		this.AddChild(this.yNegAxis);

		this.zNegAxis = new(TransformHandleAxes.Z, this.Radius, true);
		this.zNegAxis.Foreground = this.ZAxisForeground;
		this.zNegAxis.Background = this.ZAxisBackground;
		this.AddChild(this.zNegAxis);

		this.uniformAxis = new(this.UniformRadius);
		this.uniformAxis.Foreground = Color.FromArgb(0xFF, 0x83, 0x83, 0x83);
		this.uniformAxis.Background = Color.FromArgb(0x83, 0x83, 0x83, 0x83);
		this.AddChild(this.uniformAxis);
	}

	public float UniformRadius { get; set; } = 30;
	public float Radius { get; set; } = 0.4f;
}