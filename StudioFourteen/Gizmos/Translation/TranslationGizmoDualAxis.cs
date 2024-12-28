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

using StudioFourteen.Structs;
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
	private readonly Vector3[] possibleCorners = new Vector3[4];
	private Vector3 dragAxisOne;
	private Vector3 dragAxisTwo;
	private Vector3 dragAxisOnePos;
	private Vector3 dragAxisTwoPos;

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
			this.dragAxisOne = Vector3.UnitY;
			this.dragAxisTwo = Vector3.UnitZ;

			this.possibleCorners[0] = new Vector3(0, radius, radius);
			this.possibleCorners[1] = new Vector3(0, -radius, radius);
			this.possibleCorners[2] = new Vector3(0, -radius, -radius);
			this.possibleCorners[3] = new Vector3(0, radius, -radius);
		}
		else if (this.Axis == GizmoAxes.Y)
		{
			this.dragAxisOne = Vector3.UnitX;
			this.dragAxisTwo = Vector3.UnitZ;

			this.possibleCorners[0] = new Vector3(radius, 0, radius);
			this.possibleCorners[1] = new Vector3(-radius, 0, radius);
			this.possibleCorners[2] = new Vector3(-radius, 0, -radius);
			this.possibleCorners[3] = new Vector3(radius, 0, -radius);
		}
		else if (this.Axis == GizmoAxes.Z)
		{
			this.dragAxisOne = Vector3.UnitX;
			this.dragAxisTwo = Vector3.UnitY;

			this.possibleCorners[0] = new Vector3(radius, radius, 0);
			this.possibleCorners[1] = new Vector3(-radius, radius, 0);
			this.possibleCorners[2] = new Vector3(-radius, -radius, 0);
			this.possibleCorners[3] = new Vector3(radius, -radius, 0);
		}
	}

	public override void UpdateDrag(Vector mouseDelta, ref Posing.Transform transform)
	{
		double mag = mouseDelta.Length;
		mouseDelta.Normalize();

		Point a = new Point(this.square.Points[0].X, this.square.Points[0].Y);
		Point b = this.dragAxisOnePos.ToPoint();
		Vector normal = a - b;
		normal.Normalize();
		double dot = Vector.Multiply(mouseDelta, normal);
		float dragDeltaAxis1 = (float)(mag * dot);

		a = new Point(this.square.Points[0].X, this.square.Points[0].Y);
		b = this.dragAxisTwoPos.ToPoint();
		normal = a - b;
		normal.Normalize();
		dot = Vector.Multiply(mouseDelta, normal);
		float dragDeltaAxis2 = (float)(mag * dot);

		if (double.IsNaN(dragDeltaAxis1) || double.IsNaN(dragDeltaAxis2))
			return;

		dragDeltaAxis1 /= 100;
		dragDeltaAxis2 /= 100;

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
		// Get the corner with the lowest depth;
		Vector3 bestCorner = Vector3.Zero;
		double bestCornerDepth = double.MaxValue;
		for (int i = 0; i < this.possibleCorners.Length; i++)
		{
			Vector3 possibleCornerPos = this.Transform(this.possibleCorners[i], center, transformMatrix, viewMatrix);
			if (possibleCornerPos.Z < bestCornerDepth)
			{
				bestCornerDepth = possibleCornerPos.Z;
				bestCorner = this.possibleCorners[i];
			}
		}

		// Generate a full quad with the best corner
		Vector3 originPos = Vector3.Zero;
		Vector3 onePoint = new Vector3(bestCorner.X, originPos.Y, originPos.Z);
		Vector3 twoPoint = bestCorner;
		Vector3 threePoint = new Vector3(originPos.X, bestCorner.Y, bestCorner.Z);
		if (this.Axis == GizmoAxes.X)
		{
			onePoint = new Vector3(bestCorner.X, bestCorner.Y, originPos.Z);
			twoPoint = bestCorner;
			threePoint = new Vector3(bestCorner.X, originPos.Y, bestCorner.Z);
		}

		this.square.Points[0] = this.Transform(originPos, center, transformMatrix, viewMatrix).ToPoint();
		this.square.Points[1] = this.Transform(onePoint, center, transformMatrix, viewMatrix).ToPoint();
		this.square.Points[2] = this.Transform(twoPoint, center, transformMatrix, viewMatrix).ToPoint();
		this.square.Points[3] = this.Transform(threePoint, center, transformMatrix, viewMatrix).ToPoint();

		this.square.Fill = this.ForegroundBrush;

		if (this.IsAxisHovered)
		{
			this.square.Stroke = this.ForegroundBrush;
		}
		else
		{
			this.square.Stroke = new SolidColorBrush(Colors.Transparent);
		}

		this.dragAxisOnePos = this.Transform(this.dragAxisOne, center, transformMatrix, viewMatrix);
		this.dragAxisTwoPos = this.Transform(this.dragAxisTwo, center, transformMatrix, viewMatrix);
	}

	public override int GetDepthAtCursor(Point p)
	{
		if (!this.square.IsPointWithin(p))
			return int.MinValue;

		return 0;
	}
}
