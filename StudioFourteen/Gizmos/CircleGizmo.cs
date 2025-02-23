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

using System;
using System.Numerics;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

public class CircleGizmo : GizmoBase
{
	public Color Foreground = Colors.White;
	public int Thickness = 1;
	public float Radius;

	private const int NumPoints = 144;

	private readonly Line[] segments = new Line[NumPoints];
	private readonly Vector3[] points3d = new Vector3[NumPoints];

	public override void Enable(GizmoRenderer renderer)
	{
		for (int i = 0; i < this.points3d.Length; i++)
		{
			float p = i / (float)(this.points3d.Length - 1);
			float r = p * (MathF.PI * 2);

			this.points3d[i] = new Vector3(MathF.Cos(r), 0, MathF.Sin(r));
		}

		for (int i = 1; i < this.segments.Length; i++)
		{
			this.segments[i] = new();
			this.segments[i].StrokeThickness = this.Thickness;
			this.segments[i].Stroke = new SolidColorBrush(this.Foreground);
			this.segments[i].StrokeEndLineCap = PenLineCap.Round;
			this.segments[i].StrokeStartLineCap = PenLineCap.Round;
			renderer.Children.Add(this.segments[i]);
		}

		base.Enable(renderer);
	}

	public override void Disable(GizmoRenderer renderer)
	{
		base.Disable(renderer);

		for (int i = 1; i < this.segments.Length; i++)
		{
			renderer.Children.Remove(this.segments[i]);
		}
	}

	public override void Update()
	{
		for (int i = 1; i < this.points3d.Length; i++)
		{
			Line line = this.segments[i];

			line.StrokeThickness = this.Thickness;

			if (line.Stroke is not SolidColorBrush scb || scb.Color != this.Foreground)
				line.Stroke = new SolidColorBrush(this.Foreground);

			Matrix4x4 radiusScaleMatrix = Matrix4x4.CreateScale(this.Radius);

			Vector3 from = this.LocalToScreen(Vector3.Transform(this.points3d[i - 1], radiusScaleMatrix));
			Vector3 to = this.LocalToScreen(Vector3.Transform(this.points3d[i], radiusScaleMatrix));

			line.X1 = from.X;
			line.Y1 = from.Y;
			line.X2 = to.X;
			line.Y2 = to.Y;

			this.SetZIndex(line, to.Z);
		}
	}
}