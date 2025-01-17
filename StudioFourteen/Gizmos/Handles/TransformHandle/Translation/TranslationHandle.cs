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

namespace StudioFourteen.Gizmos.Handles.TransformHandle.Translation;

using StudioFourteen.Gizmos.Handles.TransformHandle;
using System.Windows.Media;

public partial class TranslationHandle : TransformHandleBase
{
	public float Radius = 0.35f;

	private readonly TranslationHandleAxis xAxis;
	private readonly TranslationHandleAxis yAxis;
	private readonly TranslationHandleAxis zAxis;
	private readonly TranslationHandleAxis xNegAxis;
	private readonly TranslationHandleAxis yNegAxis;
	private readonly TranslationHandleAxis zNegAxis;

	private readonly TranslationHandleDualAxis xDualAxis;
	private readonly TranslationHandleDualAxis yDualAxis;
	private readonly TranslationHandleDualAxis zDualAxis;

	public TranslationHandle()
	{
		this.xAxis = new(TransformHandleAxes.X, this.Radius, false);
		this.xAxis.Foreground = this.XAxisForeground;
		this.xAxis.Background = this.XAxisBackground;
		this.xAxis.DraggingBackground = this.DraggingBackground;
		this.AddChild(this.xAxis);

		this.yAxis = new(TransformHandleAxes.Y, this.Radius, false);
		this.yAxis.Foreground = this.YAxisForeground;
		this.yAxis.Background = this.YAxisBackground;
		this.yAxis.DraggingBackground = this.DraggingBackground;
		this.AddChild(this.yAxis);

		this.zAxis = new(TransformHandleAxes.Z, this.Radius, false);
		this.zAxis.Foreground = this.ZAxisForeground;
		this.zAxis.Background = this.ZAxisBackground;
		this.zAxis.DraggingBackground = this.DraggingBackground;
		this.AddChild(this.zAxis);

		this.xNegAxis = new(TransformHandleAxes.X, this.Radius, true);
		this.xNegAxis.Foreground = this.XAxisForeground;
		this.xNegAxis.Background = this.XAxisBackground;
		this.xNegAxis.DraggingBackground = this.DraggingBackground;
		this.AddChild(this.xNegAxis);

		this.yNegAxis = new(TransformHandleAxes.Y, this.Radius, true);
		this.yNegAxis.Foreground = this.YAxisForeground;
		this.yNegAxis.Background = this.YAxisBackground;
		this.yNegAxis.DraggingBackground = this.DraggingBackground;
		this.AddChild(this.yNegAxis);

		this.zNegAxis = new(TransformHandleAxes.Z, this.Radius, true);
		this.zNegAxis.Foreground = this.ZAxisForeground;
		this.zNegAxis.Background = this.ZAxisBackground;
		this.zNegAxis.DraggingBackground = this.DraggingBackground;
		this.AddChild(this.zNegAxis);

		this.xDualAxis = new(TransformHandleAxes.X, this.Radius * 0.4f);
		this.xDualAxis.Foreground = this.XAxisForeground;
		this.AddChild(this.xDualAxis);

		this.yDualAxis = new(TransformHandleAxes.Y, this.Radius * 0.4f);
		this.yDualAxis.Foreground = this.YAxisForeground;
		this.AddChild(this.yDualAxis);

		this.zDualAxis = new(TransformHandleAxes.Z, this.Radius * 0.4f);
		this.zDualAxis.Foreground = this.ZAxisForeground;
		this.AddChild(this.zDualAxis);
	}

	public bool Invert
	{
		set
		{
			this.xAxis.Invert = value;
			this.yAxis.Invert = value;
			this.zAxis.Invert = value;
			this.xNegAxis.Invert = value;
			this.yNegAxis.Invert = value;
			this.zNegAxis.Invert = value;
			this.xDualAxis.Invert = value;
			this.yDualAxis.Invert = value;
			this.zDualAxis.Invert = value;
		}
	}
}