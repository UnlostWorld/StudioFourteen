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

namespace StudioFourteen.Overlays.Primitives;

using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

public class LinePrimitive : PrimitiveBase
{
	private Line? line;

	public LinePrimitive()
	{
	}

	public LinePrimitive(Vector3 from, Vector3 to)
		: this()
	{
		this.From = from;
		this.To = to;
	}

	public Vector3 From { get; set; }
	public Vector3 To { get; set; }

	public Color Foreground { get; set; } = Colors.White;
	public int Thickness { get; set; } = 1;

	public override void Enable(Canvas canvas)
	{
		if (this.line == null)
			this.line = this.AddChild<Line>();

		base.Enable(canvas);
	}

	public override void Update()
	{
		base.Update();

		if (this.line == null)
			return;

		this.line.StrokeThickness = this.Thickness;

		if (this.line.Stroke is not SolidColorBrush scb || scb.Color != this.Foreground)
			this.line.Stroke = new SolidColorBrush(this.Foreground);

		bool visible = this.Transform(this.From, out Vector3 fromPos);
		visible |= this.Transform(this.To, out Vector3 toPos);

		this.line.X1 = fromPos.X;
		this.line.Y1 = fromPos.Y;
		this.line.X2 = toPos.X;
		this.line.Y2 = toPos.Y;
		this.line.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;

		this.SetZIndex(this.line, toPos.Z);
	}
}