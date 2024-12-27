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

namespace StudioFourteen.Gizmos.Translation;

using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

public class TranslationGizmoDualAxis : GizmoAxisBase
{
	private readonly Polygon square;
	private readonly Vector3[] corners = new Vector3[4];

	public TranslationGizmoDualAxis(GizmoAxes axis, float radius, Canvas canvas)
	{
		this.Axis = axis;
		this.square = new();
		this.square.Fill = this.ForegroundBrush;
		this.square.Points = new PointCollection()
		{
			new Point(0, 0),
			new Point(1, 0),
			new Point(1, 1),
			new Point(0, 1),
		};

		canvas.Children.Add(this.square);

		if (this.Axis == GizmoAxes.X)
		{
			this.corners[0] = Vector3.Zero;
			this.corners[1] = new Vector3(0, -radius, 0);
			this.corners[2] = new Vector3(0, -radius, -radius);
			this.corners[3] = new Vector3(0, 0, -radius);
		}
		else if (this.Axis == GizmoAxes.Y)
		{
			this.corners[0] = Vector3.Zero;
			this.corners[1] = new Vector3(-radius, 0, 0);
			this.corners[2] = new Vector3(-radius, 0, -radius);
			this.corners[3] = new Vector3(0, 0, -radius);
		}
		else if (this.Axis == GizmoAxes.Z)
		{
			this.corners[0] = Vector3.Zero;
			this.corners[1] = new Vector3(-radius, 0, 0);
			this.corners[2] = new Vector3(-radius, -radius, 0);
			this.corners[3] = new Vector3(0, -radius, 0);
		}
	}

	public override void UpdateDrag(System.Windows.Vector mouseDelta, ref Posing.Transform transform)
	{
	}

	public override void Transform(Matrix4x4 transformMatrix, Matrix4x4 viewMatrix, Vector2 center)
	{
		double z = 0;
		for(int i = 0; i < this.corners.Length; i++)
		{
			Vector3 cornerPoint = Vector3.Transform(this.corners[i], transformMatrix);
			cornerPoint = Vector3.Transform(cornerPoint, viewMatrix);
			z += cornerPoint.Z;
			Vector2 cornerPos = center + new Vector2(cornerPoint.X, cornerPoint.Y);

			this.square.Points[i] = new(cornerPos.X, cornerPos.Y);
		}

		z /= this.corners.Length;

		this.square.Fill = this.ForegroundBrush;

		if (this.IsAxisHovered)
		{
			this.square.Stroke = this.ForegroundBrush;
			this.square.StrokeThickness = 3;
			this.square.Fill = new SolidColorBrush(Colors.Transparent);
		}

		Panel.SetZIndex(this.square, 200 - (int)(z * 100));
	}

	public override bool IsMouseOver(Point mousePos)
	{
		return this.square.IsMouseOver;
	}
}
