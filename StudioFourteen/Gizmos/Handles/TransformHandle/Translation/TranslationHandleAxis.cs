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
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

using Transform = StudioFourteen.Posing.Transform;
using Vector = System.Windows.Vector;

public class TranslationHandleAxis : TransformHandleAxisBase
{
	public float Radius;
	public bool Flip = false;

	private Line? lineOne;
	private Line? lineTwo;
	private Line? arrowOne;
	private Line? arrowTwo;
	private Vector3 segmentOneEnd;
	private Vector3 segmentTwoEnd;
	private Vector3 arrowOneStart;
	private Vector3 arrowTwoStart;

	public TranslationHandleAxis(TransformHandleAxes axis, float radius)
	{
		this.Axis = axis;
		this.Radius = radius;

		if (this.Axis == TransformHandleAxes.X)
		{
			this.segmentOneEnd = -Vector3.UnitX * this.Radius;
			this.segmentTwoEnd = Vector3.UnitX * this.Radius;
			this.arrowOneStart = -Vector3.UnitX * (this.Radius * 0.85f);
			this.arrowTwoStart = Vector3.UnitX * (this.Radius * 0.85f);
		}
		else if (this.Axis == TransformHandleAxes.Y)
		{
			this.segmentOneEnd = -Vector3.UnitY * this.Radius;
			this.segmentTwoEnd = Vector3.UnitY * this.Radius;
			this.arrowOneStart = -Vector3.UnitY * (this.Radius * 0.85f);
			this.arrowTwoStart = Vector3.UnitY * (this.Radius * 0.85f);
		}
		else if (this.Axis == TransformHandleAxes.Z)
		{
			this.segmentOneEnd = -Vector3.UnitZ * this.Radius;
			this.segmentTwoEnd = Vector3.UnitZ * this.Radius;
			this.arrowOneStart = -Vector3.UnitZ * (this.Radius * 0.85f);
			this.arrowTwoStart = Vector3.UnitZ * (this.Radius * 0.85f);
		}
	}

	public override void Enable(Canvas canvas)
	{
		if (this.lineOne == null)
		{
			this.lineOne = this.AddChild<Line>();
			this.lineOne.IsHitTestVisible = false;
			this.lineOne.StrokeThickness = 3;
			this.lineOne.Stroke = this.ForegroundBrush;
			this.lineOne.StrokeEndLineCap = PenLineCap.Triangle;
			this.lineOne.StrokeStartLineCap = PenLineCap.Round;
		}

		if (this.lineTwo == null)
		{
			this.lineTwo = this.AddChild<Line>();
			this.lineTwo.IsHitTestVisible = false;
			this.lineTwo.StrokeThickness = 3;
			this.lineTwo.Stroke = this.ForegroundBrush;
			this.lineTwo.StrokeEndLineCap = PenLineCap.Triangle;
			this.lineTwo.StrokeStartLineCap = PenLineCap.Round;
		}

		if (this.arrowOne == null)
		{
			this.arrowOne = this.AddChild<Line>();
			this.arrowOne.IsHitTestVisible = false;
			this.arrowOne.StrokeThickness = 15;
			this.arrowOne.Stroke = this.ForegroundBrush;
			this.arrowOne.StrokeEndLineCap = PenLineCap.Triangle;
			this.arrowOne.StrokeStartLineCap = PenLineCap.Round;
		}

		if (this.arrowTwo == null)
		{
			this.arrowTwo = this.AddChild<Line>();
			this.arrowTwo.IsHitTestVisible = false;
			this.arrowTwo.StrokeThickness = 15;
			this.arrowTwo.Stroke = this.ForegroundBrush;
			this.arrowTwo.StrokeEndLineCap = PenLineCap.Triangle;
			this.arrowTwo.StrokeStartLineCap = PenLineCap.Round;
		}

		base.Enable(canvas);
	}

	public override void Update()
	{
		base.Update();

		if (this.lineOne == null
			|| this.lineTwo == null
			|| this.arrowOne == null
			|| this.arrowTwo == null)
			return;

		Vector3 originPos = this.LocalToScreen(Vector3.Zero);
		Vector3 toPosOne = this.LocalToScreen(this.segmentOneEnd);
		Vector3 arrowOneFromPos = this.LocalToScreen(this.arrowOneStart);

		this.lineOne.X1 = originPos.X;
		this.lineOne.Y1 = originPos.Y;
		this.lineOne.X2 = arrowOneFromPos.X;
		this.lineOne.Y2 = arrowOneFromPos.Y;
		this.SetZIndex(this.lineOne, toPosOne.Z);

		this.arrowOne.X1 = arrowOneFromPos.X;
		this.arrowOne.Y1 = arrowOneFromPos.Y;
		this.arrowOne.X2 = toPosOne.X;
		this.arrowOne.Y2 = toPosOne.Y;
		this.SetZIndex(this.arrowOne, toPosOne.Z);

		// Two
		Vector3 toPosTwo = this.LocalToScreen(this.segmentTwoEnd);
		Vector3 arrowTwoFromPos = this.LocalToScreen(this.arrowTwoStart);
		this.lineTwo.X1 = originPos.X;
		this.lineTwo.Y1 = originPos.Y;
		this.lineTwo.X2 = arrowTwoFromPos.X;
		this.lineTwo.Y2 = arrowTwoFromPos.Y;
		this.SetZIndex(this.lineTwo, toPosTwo.Z);

		this.arrowTwo.X1 = arrowTwoFromPos.X;
		this.arrowTwo.Y1 = arrowTwoFromPos.Y;
		this.arrowTwo.X2 = toPosTwo.X;
		this.arrowTwo.Y2 = toPosTwo.Y;
		this.SetZIndex(this.arrowTwo, toPosTwo.Z);

		if (this.IsDragging)
		{
			this.lineOne.Stroke = toPosOne.Z < originPos.Z ? this.DraggingBrush : this.DraggingBackgroundBrush;
			this.arrowOne.Stroke = toPosOne.Z < originPos.Z ? this.DraggingBrush : this.DraggingBackgroundBrush;
			this.lineTwo.Stroke = toPosTwo.Z < originPos.Z ? this.DraggingBrush : this.DraggingBackgroundBrush;
			this.arrowTwo.Stroke = toPosTwo.Z < originPos.Z ? this.DraggingBrush : this.DraggingBackgroundBrush;
		}
		else
		{
			this.lineOne.Stroke = toPosOne.Z < originPos.Z ? this.ForegroundBrush : this.BackgroundBrush;
			this.arrowOne.Stroke = toPosOne.Z < originPos.Z ? this.ForegroundBrush : this.BackgroundBrush;
			this.lineTwo.Stroke = toPosTwo.Z < originPos.Z ? this.ForegroundBrush : this.BackgroundBrush;
			this.arrowTwo.Stroke = toPosTwo.Z < originPos.Z ? this.ForegroundBrush : this.BackgroundBrush;
		}

		if (this.IsCursorOver)
		{
			this.arrowOne.StrokeThickness = 15;
			this.arrowTwo.StrokeThickness = 15;
		}
		else
		{
			this.arrowOne.StrokeThickness = 10;
			this.arrowTwo.StrokeThickness = 10;
		}
	}

	public override int HitTest(Point mousePos)
	{
		if (this.lineOne == null
			|| this.lineTwo == null
			|| this.arrowOne == null
			|| this.arrowTwo == null)
			return int.MinValue;

		Point fromPos = new Point(this.arrowOne.X1, this.arrowOne.Y1);
		Point toPos = new Point(this.arrowOne.X2, this.arrowOne.Y2);
		double distance = Point.Subtract(mousePos, toPos).Length;

		if (distance < 20)
			return Panel.GetZIndex(this.arrowOne);

		fromPos = new Point(this.arrowTwo.X1, this.arrowTwo.Y1);
		toPos = new Point(this.arrowTwo.X2, this.arrowTwo.Y2);
		distance = Point.Subtract(mousePos, toPos).Length;

		if (distance < 20)
			return Panel.GetZIndex(this.arrowTwo);

		return int.MinValue;
	}

	public override Transform OnDrag(Vector mouseDelta, Transform transform)
	{
		if (this.lineOne == null
			|| this.lineTwo == null
			|| this.arrowOne == null
			|| this.arrowTwo == null)
			return transform;

		Point a = new Point(this.arrowOne.X2, this.arrowOne.Y2);
		Point b = new Point(this.arrowOne.X1, this.arrowOne.Y1);
		Vector normal = a - b;
		normal.Normalize();

		double mag = mouseDelta.Length;
		mouseDelta.Normalize();

		double dot = Vector.Multiply(mouseDelta, normal);
		float dragDelta = (float)(mag * dot);
		dragDelta /= 100;
		dragDelta *= (float)this.Sensitivity;

		if (Keyboard.Modifiers == ModifierKeys.Shift)
			dragDelta *= 10;

		if (Keyboard.Modifiers == ModifierKeys.Control)
			dragDelta /= 10;

		if (this.Services.Tablet.PenPressure > 0)
		{
			dragDelta *= (float)this.Services.Tablet.PenPressure;
		}

		if (this.Flip)
			dragDelta = -dragDelta;

		Vector3 delta = Vector3.Zero;
		if (this.Axis == TransformHandleAxes.X)
		{
			delta = Vector3.UnitX * dragDelta;
		}
		else if (this.Axis == TransformHandleAxes.Y)
		{
			delta = Vector3.UnitY * dragDelta;
		}
		else if (this.Axis == TransformHandleAxes.Z)
		{
			delta = Vector3.UnitZ * dragDelta;
		}

		delta = Vector3.Transform(delta, transform.Rotation);
		transform.Translation += delta;
		return transform;
	}
}