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

using System.Collections.Generic;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

public abstract class WireframePrimitive : PrimitiveBase
{
	private readonly List<(Vector3 From, Vector3 To)> lines = new();
	private readonly List<Vector3[]> polyLines = new();

	private readonly List<Line> lineVisuals = new();
	private readonly List<Polyline> polyLineVisuals = new();

	public Vector3 Position { get; set; } = Vector3.Zero;
	public Quaternion Rotation { get; set; } = Quaternion.Identity;
	public Vector3 Scale { get; set; } = Vector3.One;

	public override void Enable(Canvas canvas)
	{
		for (int i = 0; i < this.lines.Count; i++)
		{
			Line line = this.AddChild<Line>();
			line.StrokeThickness = 1;
			line.Stroke = new SolidColorBrush(Colors.White);
			line.IsHitTestVisible = false;
			this.lineVisuals.Add(line);
		}

		for (int i = 0; i < this.polyLines.Count; i++)
		{
			Polyline line = this.AddChild<Polyline>();
			line.StrokeThickness = 1;
			line.Stroke = new SolidColorBrush(Colors.White);
			line.IsHitTestVisible = false;

			foreach (Vector3 worldPos in this.polyLines[i])
			{
				line.Points.Add(new Point(0, 0));
			}

			this.polyLineVisuals.Add(line);
		}

		base.Enable(canvas);
	}

	public override void Disable(Canvas canvas)
	{
		base.Disable(canvas);

		this.lineVisuals.Clear();
		this.polyLineVisuals.Clear();
	}

	/*public override void Transform(Matrix4x4 matrix)
	{
		base.Transform(matrix);

		if (this.lines.Count != this.lineVisuals.Count)
			return;

		if (this.polyLines.Count != this.polyLineVisuals.Count)
			return;

		for (int i = 0; i < this.lines.Count; i++)
		{
			Vector3 from = Vector3.Transform(this.lines[i].From * this.Scale, this.Rotation);
			Vector3 to = Vector3.Transform(this.lines[i].To * this.Scale, this.Rotation);
			this.Services.Camera.WorldToCamera(from + this.Position, out Vector3 fromScreenPos);
			this.Services.Camera.WorldToCamera(to + this.Position, out Vector3 toScreenPos);

			Line line = this.lineVisuals[i];

			Canvas.SetLeft(line, 0);
			Canvas.SetTop(line, 0);

			line.X1 = fromScreenPos.X * canvas.ActualWidth;
			line.Y1 = fromScreenPos.Y * canvas.ActualHeight;
			line.X2 = toScreenPos.X * canvas.ActualWidth;
			line.Y2 = toScreenPos.Y * canvas.ActualHeight;
		}

		for (int i = 0; i < this.polyLines.Count; i++)
		{
			Polyline line = this.polyLineVisuals[i];
			Canvas.SetLeft(line, 0);
			Canvas.SetTop(line, 0);

			for (int j = 0; j < this.polyLines[i].Length; j++)
			{
				Vector3 worldPos = Vector3.Transform(this.polyLines[i][j] * this.Scale, this.Rotation);
				this.Services.Camera.WorldToCamera(worldPos + this.Position, out Vector3 screenPos);

				Point p = line.Points[j];
				p.X = screenPos.X * canvas.ActualWidth;
				p.Y = screenPos.Y * canvas.ActualHeight;
				line.Points[j] = p;
			}
		}
	}*/

	protected void AddLine(Vector3 from, Vector3 to)
	{
		this.lines.Add((from, to));
	}

	protected void AddLine(params Vector3[] points)
	{
		this.polyLines.Add(points);
	}
}
