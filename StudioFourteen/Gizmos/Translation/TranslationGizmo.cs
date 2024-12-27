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

namespace StudioFourteen.Gizmos;

using DependencyPropertyGenerator;
using StudioFourteen.Gizmos.Translation;
using System.Numerics;
using System.Windows;
using System.Windows.Media;

[DependencyProperty("Translation", typeof(Vector3))]
public partial class TranslationGizmo : GizmoBase
{
	private readonly TranslationGizmoAxis xAxis;
	private readonly TranslationGizmoAxis yAxis;
	private readonly TranslationGizmoAxis zAxis;

	private readonly TranslationGizmoDualAxis xDualAxis;
	private readonly TranslationGizmoDualAxis yDualAxis;
	private readonly TranslationGizmoDualAxis zDualAxis;

	public TranslationGizmo()
	{
		this.xAxis = new(GizmoAxes.X, this.Radius, this.Canvas);
		this.xAxis.ForegroundBrush = new SolidColorBrush(Color.FromArgb(0xFF, 0x33, 0x33, 0xFF));
		this.xAxis.BackgroundBrush = new SolidColorBrush(Color.FromArgb(0x10, 0x33, 0x33, 0xFF));
		this.AddAxis(this.xAxis);

		this.yAxis = new(GizmoAxes.Y, this.Radius, this.Canvas);
		this.yAxis.ForegroundBrush = new SolidColorBrush(Color.FromArgb(0xFF, 0x33, 0xFF, 0x33));
		this.yAxis.BackgroundBrush = new SolidColorBrush(Color.FromArgb(0x10, 0x33, 0xFF, 0x33));
		this.AddAxis(this.yAxis);

		this.zAxis = new(GizmoAxes.Z, this.Radius, this.Canvas);
		this.zAxis.ForegroundBrush = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0x33, 0x33));
		this.zAxis.BackgroundBrush = new SolidColorBrush(Color.FromArgb(0x10, 0xFF, 0x33, 0x33));
		this.AddAxis(this.zAxis);

		this.xDualAxis = new(GizmoAxes.X, this.Radius * 0.4f, this.Canvas);
		this.xDualAxis.ForegroundBrush = new SolidColorBrush(Color.FromArgb(0xFF, 0x33, 0x33, 0xFF));
		this.AddAxis(this.xDualAxis);

		this.yDualAxis = new(GizmoAxes.Y, this.Radius * 0.4f, this.Canvas);
		this.yDualAxis.ForegroundBrush = new SolidColorBrush(Color.FromArgb(0xFF, 0x33, 0xFF, 0x33));
		this.AddAxis(this.yDualAxis);

		this.zDualAxis = new(GizmoAxes.Z, this.Radius * 0.4f, this.Canvas);
		this.zDualAxis.ForegroundBrush = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0x33, 0x33));
		this.AddAxis(this.zDualAxis);
	}

	public float Radius { get; set; } = 70;
}