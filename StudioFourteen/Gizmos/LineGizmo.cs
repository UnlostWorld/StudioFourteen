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

using System.Numerics;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

public class LineGizmo : GizmoBase
{
	public Vector3 From;
	public Vector3 To;
	public Color Foreground = Colors.White;
	public int Thickness = 1;

	private Line? line;

	public LineGizmo()
	{
	}

	public LineGizmo(Vector3 from, Vector3 to)
		: this()
	{
		this.From = from;
		this.To = to;
	}

	public override void Enable(GizmoRenderer renderer)
	{
		if (this.line == null)
			this.line = this.AddChild<Line>();

		base.Enable(renderer);
	}

	public override void Update()
	{
		if (this.line == null)
			return;

		this.line.StrokeThickness = this.Thickness;

		if (this.line.Stroke is not SolidColorBrush scb || scb.Color != this.Foreground)
			this.line.Stroke = new SolidColorBrush(this.Foreground);

		Vector3 fromPos = this.LocalToScreen(this.From);
		Vector3 toPos = this.LocalToScreen(this.To);

		this.line.X1 = fromPos.X;
		this.line.Y1 = fromPos.Y;
		this.line.X2 = toPos.X;
		this.line.Y2 = toPos.Y;

		this.SetZIndex(this.line, toPos.Z);
	}
}