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
using StudioFourteen.Structs;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

using Vector = System.Windows.Vector;

public class TranslationHandleDualAxis : TransformHandleAxisBase
{
	public bool Invert = false;

	private readonly Vector3[] possibleCorners = new Vector3[4];
	private Vector3 dragAxisOne;
	private Vector3 dragAxisTwo;
	private Vector3 dragAxisOnePos;
	private Vector3 dragAxisTwoPos;
	private Polygon? square;

	public TranslationHandleDualAxis(TransformHandleAxes axis, float radius)
	{
		this.Axis = axis;

		if (this.Axis == TransformHandleAxes.X)
		{
			this.dragAxisOne = Vector3.UnitY;
			this.dragAxisTwo = Vector3.UnitZ;

			this.possibleCorners[0] = new Vector3(0, radius, radius);
			this.possibleCorners[1] = new Vector3(0, -radius, radius);
			this.possibleCorners[2] = new Vector3(0, -radius, -radius);
			this.possibleCorners[3] = new Vector3(0, radius, -radius);
		}
		else if (this.Axis == TransformHandleAxes.Y)
		{
			this.dragAxisOne = Vector3.UnitX;
			this.dragAxisTwo = Vector3.UnitZ;

			this.possibleCorners[0] = new Vector3(radius, 0, radius);
			this.possibleCorners[1] = new Vector3(-radius, 0, radius);
			this.possibleCorners[2] = new Vector3(-radius, 0, -radius);
			this.possibleCorners[3] = new Vector3(radius, 0, -radius);
		}
		else if (this.Axis == TransformHandleAxes.Z)
		{
			this.dragAxisOne = Vector3.UnitX;
			this.dragAxisTwo = Vector3.UnitY;

			this.possibleCorners[0] = new Vector3(radius, radius, 0);
			this.possibleCorners[1] = new Vector3(-radius, radius, 0);
			this.possibleCorners[2] = new Vector3(-radius, -radius, 0);
			this.possibleCorners[3] = new Vector3(radius, -radius, 0);
		}
	}

	public override void Enable(GizmoRenderer renderer)
	{
		this.square = this.AddChild<Polygon>();
		this.square.IsHitTestVisible = false;
		this.square.Fill = this.ForegroundBrush;
		this.square.Stroke = this.ForegroundBrush;
		this.square.StrokeThickness = 0;
		this.square.StrokeLineJoin = PenLineJoin.Bevel;
		this.square.Points = new PointCollection()
		{
			new Point(0, 0),
			new Point(1, 0),
			new Point(1, 1),
			new Point(0, 1),
		};

		base.Enable(renderer);
	}

	public override Posing.Transform OnDrag(Vector mouseDelta, Posing.Transform transform)
	{
		if (this.square == null)
			return transform;

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
			return default;

		dragDeltaAxis1 /= 100;
		dragDeltaAxis1 *= (float)this.Sensitivity;
		dragDeltaAxis2 /= 100;
		dragDeltaAxis2 *= (float)this.Sensitivity;

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

		if (this.Invert)
		{
			dragDeltaAxis1 = -dragDeltaAxis1;
			dragDeltaAxis2 = -dragDeltaAxis2;
		}

		Vector3 delta = Vector3.Zero;
		if (this.Axis == TransformHandleAxes.X)
		{
			delta += Vector3.UnitY * dragDeltaAxis1;
			delta += Vector3.UnitZ * dragDeltaAxis2;
		}
		else if (this.Axis == TransformHandleAxes.Y)
		{
			delta += Vector3.UnitX * dragDeltaAxis1;
			delta += Vector3.UnitZ * dragDeltaAxis2;
		}
		else if (this.Axis == TransformHandleAxes.Z)
		{
			delta += Vector3.UnitX * dragDeltaAxis1;
			delta += Vector3.UnitY * dragDeltaAxis2;
		}

		delta = Vector3.Transform(delta, transform.Rotation);
		transform.Translation += delta;
		return transform;
	}

	public override void Update()
	{
		base.Update();

		if (this.square == null)
			return;

		// Get the corner with the lowest depth;
		Vector3 bestCorner = Vector3.Zero;
		float bestCornerDepth = float.MaxValue;
		for (int i = 0; i < this.possibleCorners.Length; i++)
		{
			Vector3 possibleCornerPos = this.LocalToScreen(this.possibleCorners[i]);
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
		if (this.Axis == TransformHandleAxes.X)
		{
			onePoint = new Vector3(bestCorner.X, bestCorner.Y, originPos.Z);
			twoPoint = bestCorner;
			threePoint = new Vector3(bestCorner.X, originPos.Y, bestCorner.Z);
		}

		this.square.Points[0] = this.LocalToScreen(originPos).ToPoint();
		this.square.Points[1] = this.LocalToScreen(onePoint).ToPoint();
		this.square.Points[2] = this.LocalToScreen(twoPoint).ToPoint();
		this.square.Points[3] = this.LocalToScreen(threePoint).ToPoint();

		this.square.Fill = this.IsDragging ? this.DraggingBrush : this.ForegroundBrush;
		this.square.Stroke = this.IsDragging ? this.DraggingBrush : this.ForegroundBrush;

		if (this.IsCursorOver)
		{
			this.square.StrokeThickness = 3;
		}
		else
		{
			this.square.StrokeThickness = 0;
		}

		this.SetZIndex(this.square, bestCornerDepth);

		this.dragAxisOnePos = this.LocalToScreen(this.dragAxisOne);
		this.dragAxisTwoPos = this.LocalToScreen(this.dragAxisTwo);
	}

	public override int HitTest(Point p)
	{
		if (this.square == null)
			return int.MinValue;

		if (!this.square.IsPointWithin(p))
			return int.MinValue;

		return Panel.GetZIndex(this.square);
	}
}