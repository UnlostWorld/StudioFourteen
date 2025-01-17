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

	private readonly TranslationHandleDualAxis xDualAxis;
	private readonly TranslationHandleDualAxis yDualAxis;
	private readonly TranslationHandleDualAxis zDualAxis;

	public TranslationHandle()
	{
		this.xAxis = new(TransformHandleAxes.X, this.Radius);
		this.xAxis.Foreground = Color.FromArgb(0xFF, 0x33, 0x33, 0xFF);
		this.xAxis.Background = Color.FromArgb(0xFF, 0x33, 0x33, 0x4D);
		this.AddChild(this.xAxis);

		this.yAxis = new(TransformHandleAxes.Y, this.Radius);
		this.yAxis.Foreground = Color.FromArgb(0xFF, 0x33, 0xFF, 0x33);
		this.yAxis.Background = Color.FromArgb(0xFF, 0x33, 0x4D, 0x33);
		this.AddChild(this.yAxis);

		this.zAxis = new(TransformHandleAxes.Z, this.Radius);
		this.zAxis.Foreground = Color.FromArgb(0xFF, 0xFF, 0x33, 0x33);
		this.zAxis.Background = Color.FromArgb(0xFF, 0x4D, 0x33, 0x33);
		this.AddChild(this.zAxis);

		this.xDualAxis = new(TransformHandleAxes.X, this.Radius * 0.4f);
		this.xDualAxis.Foreground = Color.FromArgb(0xFF, 0x33, 0x33, 0xFF);
		this.AddChild(this.xDualAxis);

		this.yDualAxis = new(TransformHandleAxes.Y, this.Radius * 0.4f);
		this.yDualAxis.Foreground = Color.FromArgb(0xFF, 0x33, 0xFF, 0x33);
		this.AddChild(this.yDualAxis);

		this.zDualAxis = new(TransformHandleAxes.Z, this.Radius * 0.4f);
		this.zDualAxis.Foreground = Color.FromArgb(0xFF, 0xFF, 0x33, 0x33);
		this.AddChild(this.zDualAxis);
	}

	public bool Flip
	{
		set
		{
			this.xAxis.Flip = value;
			this.yAxis.Flip = value;
			this.zAxis.Flip = value;
			this.xDualAxis.Flip = value;
			this.yDualAxis.Flip = value;
			this.zDualAxis.Flip = value;
		}
	}
}