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
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

using Vector = System.Windows.Vector;

public class TranslationGizmoDualAxis : GizmoAxisBase
{
	private readonly Polygon square;
	private readonly Vector3[] corners = new Vector3[4];
	private int depth = -1;

	public TranslationGizmoDualAxis(GizmoAxes axis, float radius, Canvas canvas)
	{
		this.Axis = axis;
		this.square = new();
		this.square.Fill = this.ForegroundBrush;
		this.square.StrokeThickness = 3;
		this.square.Stroke = this.ForegroundBrush;
		this.square.StrokeLineJoin = PenLineJoin.Bevel;
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

	public override void UpdateDrag(Vector mouseDelta, ref Posing.Transform transform)
	{
		double mag = mouseDelta.Length;
		mouseDelta.Normalize();

		Point a = new Point(this.square.Points[1].X, this.square.Points[1].Y);
		Point b = new Point(this.square.Points[0].X, this.square.Points[0].Y);
		Vector normal = a - b;
		normal.Normalize();
		double dot = Vector.Multiply(mouseDelta, normal);
		float dragDeltaAxis1 = (float)(mag * dot);

		a = new Point(this.square.Points[3].X, this.square.Points[3].Y);
		b = new Point(this.square.Points[0].X, this.square.Points[0].Y);
		normal = a - b;
		normal.Normalize();
		dot = Vector.Multiply(mouseDelta, normal);
		float dragDeltaAxis2 = (float)(mag * dot);

		if (double.IsNaN(dragDeltaAxis1) || double.IsNaN(dragDeltaAxis2))
			return;

		dragDeltaAxis1 /= 50;
		dragDeltaAxis2 /= 50;

		if (Keyboard.Modifiers == ModifierKeys.Shift)
		{
			dragDeltaAxis1 *= 10;
			dragDeltaAxis2 *= 10;
		}

		if (Keyboard.Modifiers == ModifierKeys.Control)
		{
			dragDeltaAxis1 /= 10;
			dragDeltaAxis2 /= 10;
		}

		if (this.Services.Tablet.PenPressure > 0)
		{
			dragDeltaAxis1 *= (float)this.Services.Tablet.PenPressure;
			dragDeltaAxis2 *= (float)this.Services.Tablet.PenPressure;
		}

		Vector3 delta = Vector3.Zero;
		if (this.Axis == GizmoAxes.X)
		{
			delta += Vector3.UnitY * dragDeltaAxis1;
			delta += Vector3.UnitZ * dragDeltaAxis2;
		}
		else if (this.Axis == GizmoAxes.Y)
		{
			delta += Vector3.UnitX * dragDeltaAxis1;
			delta += Vector3.UnitZ * dragDeltaAxis2;
		}
		else if (this.Axis == GizmoAxes.Z)
		{
			delta += Vector3.UnitX * dragDeltaAxis1;
			delta += Vector3.UnitY * dragDeltaAxis2;
		}

		delta = Vector3.Transform(delta, transform.Rotation);
		transform.Translation += delta;
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
		this.depth = 200 - (int)(z * 100);
		Panel.SetZIndex(this.square, this.depth);

		this.square.Fill = this.ForegroundBrush;

		if (this.IsAxisHovered)
		{
			this.square.Stroke = this.ForegroundBrush;
		}
		else
		{
			this.square.Stroke = new SolidColorBrush(Colors.Transparent);
		}
	}

	public override int GetDepthAtCursor(Point p)
	{
		if (!this.square.IsPointWithin(p))
			return int.MinValue;

		return this.depth;
	}
}
