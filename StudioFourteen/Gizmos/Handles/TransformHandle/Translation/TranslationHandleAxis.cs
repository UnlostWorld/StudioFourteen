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
	public bool Invert = false;

	private readonly bool flip;
	private Line? line;
	private Line? arrow;
	private Vector3 segmentEnd;
	private Vector3 arrowStart;

	public TranslationHandleAxis(TransformHandleAxes axis, float radius, bool flip)
	{
		this.Axis = axis;
		this.Radius = radius;
		this.flip = flip;

		if (this.Axis == TransformHandleAxes.X)
		{
			this.segmentEnd = (flip ? Vector3.UnitX : -Vector3.UnitX) * this.Radius;
			this.arrowStart = (flip ? Vector3.UnitX : -Vector3.UnitX) * (this.Radius * 0.85f);
		}
		else if (this.Axis == TransformHandleAxes.Y)
		{
			this.segmentEnd = (flip ? Vector3.UnitY : -Vector3.UnitY) * this.Radius;
			this.arrowStart = (flip ? Vector3.UnitY : -Vector3.UnitY) * (this.Radius * 0.85f);
		}
		else if (this.Axis == TransformHandleAxes.Z)
		{
			this.segmentEnd = (flip ? Vector3.UnitZ : -Vector3.UnitZ) * this.Radius;
			this.arrowStart = (flip ? Vector3.UnitZ : -Vector3.UnitZ) * (this.Radius * 0.85f);
		}
	}

	public override void Enable(Canvas canvas)
	{
		if (this.line == null)
		{
			this.line = this.AddChild<Line>();
			this.line.IsHitTestVisible = false;
			this.line.StrokeThickness = 3;
			this.line.Stroke = this.ForegroundBrush;
			this.line.StrokeEndLineCap = PenLineCap.Triangle;
			this.line.StrokeStartLineCap = PenLineCap.Round;
		}

		if (this.arrow == null)
		{
			this.arrow = this.AddChild<Line>();
			this.arrow.IsHitTestVisible = false;
			this.arrow.StrokeThickness = 15;
			this.arrow.Stroke = this.ForegroundBrush;
			this.arrow.StrokeEndLineCap = PenLineCap.Triangle;
			this.arrow.StrokeStartLineCap = PenLineCap.Round;
		}

		base.Enable(canvas);
	}

	public override void Update()
	{
		base.Update();

		if (this.line == null || this.arrow == null)
			return;

		Vector3 originPos = this.LocalToScreen(Vector3.Zero);
		Vector3 toPosOne = this.LocalToScreen(this.segmentEnd);
		Vector3 arrowOneFromPos = this.LocalToScreen(this.arrowStart);

		this.line.X1 = originPos.X;
		this.line.Y1 = originPos.Y;
		this.line.X2 = arrowOneFromPos.X;
		this.line.Y2 = arrowOneFromPos.Y;
		this.SetZIndex(this.line, toPosOne.Z);

		this.arrow.X1 = arrowOneFromPos.X;
		this.arrow.Y1 = arrowOneFromPos.Y;
		this.arrow.X2 = toPosOne.X;
		this.arrow.Y2 = toPosOne.Y;
		this.arrow.IsEnabled = toPosOne.Z < originPos.Z;
		this.SetZIndex(this.arrow, toPosOne.Z);

		if (this.IsDragging)
		{
			this.line.Stroke = toPosOne.Z < originPos.Z ? this.DraggingBrush : this.DraggingBackgroundBrush;
			this.arrow.Stroke = toPosOne.Z < originPos.Z ? this.DraggingBrush : this.DraggingBackgroundBrush;
		}
		else
		{
			this.line.Stroke = toPosOne.Z < originPos.Z ? this.ForegroundBrush : this.BackgroundBrush;
			this.arrow.Stroke = toPosOne.Z < originPos.Z ? this.ForegroundBrush : this.BackgroundBrush;
		}

		this.arrow.StrokeThickness = this.IsCursorOver ? 15 : 10;
	}

	public override int HitTest(Point mousePos)
	{
		if (this.line == null || this.arrow == null || !this.arrow.IsEnabled)
			return int.MinValue;

		Point fromPos = new Point(this.arrow.X1, this.arrow.Y1);
		Point toPos = new Point(this.arrow.X2, this.arrow.Y2);
		double distance = Point.Subtract(mousePos, toPos).Length;

		if (distance < 20)
			return Panel.GetZIndex(this.arrow);

		return int.MinValue;
	}

	public override Transform OnDrag(Vector mouseDelta, Transform transform)
	{
		if (this.line == null || this.arrow == null)
			return transform;

		Point a = new Point(this.arrow.X2, this.arrow.Y2);
		Point b = new Point(this.arrow.X1, this.arrow.Y1);
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

		if (this.Invert != this.flip)
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