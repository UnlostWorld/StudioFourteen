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

namespace StudioFourteen.Overlays.Gizmos.Translation;

using StudioFourteen.Overlays.Gizmos;
using System.Windows.Media;

public partial class TranslationGizmo : GizmoBase
{
	public float Radius = 0.35f;

	private readonly TranslationGizmoAxis xAxis;
	private readonly TranslationGizmoAxis yAxis;
	private readonly TranslationGizmoAxis zAxis;

	private readonly TranslationGizmoDualAxis xDualAxis;
	private readonly TranslationGizmoDualAxis yDualAxis;
	private readonly TranslationGizmoDualAxis zDualAxis;

	public TranslationGizmo()
	{
		this.xAxis = new(GizmoAxes.X, this.Radius);
		this.xAxis.Foreground = Color.FromArgb(0xFF, 0x33, 0x33, 0xFF);
		this.xAxis.Background = Color.FromArgb(0xFF, 0x33, 0x33, 0x4D);
		this.AddAxis(this.xAxis);

		this.yAxis = new(GizmoAxes.Y, this.Radius);
		this.yAxis.Foreground = Color.FromArgb(0xFF, 0x33, 0xFF, 0x33);
		this.yAxis.Background = Color.FromArgb(0xFF, 0x33, 0x4D, 0x33);
		this.AddAxis(this.yAxis);

		this.zAxis = new(GizmoAxes.Z, this.Radius);
		this.zAxis.Foreground = Color.FromArgb(0xFF, 0xFF, 0x33, 0x33);
		this.zAxis.Background = Color.FromArgb(0xFF, 0x4D, 0x33, 0x33);
		this.AddAxis(this.zAxis);

		this.xDualAxis = new(GizmoAxes.X, this.Radius * 0.4f);
		this.xDualAxis.Foreground = Color.FromArgb(0xFF, 0x33, 0x33, 0xFF);
		this.AddAxis(this.xDualAxis);

		this.yDualAxis = new(GizmoAxes.Y, this.Radius * 0.4f);
		this.yDualAxis.Foreground = Color.FromArgb(0xFF, 0x33, 0xFF, 0x33);
		this.AddAxis(this.yDualAxis);

		this.zDualAxis = new(GizmoAxes.Z, this.Radius * 0.4f);
		this.zDualAxis.Foreground = Color.FromArgb(0xFF, 0xFF, 0x33, 0x33);
		this.AddAxis(this.zDualAxis);
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