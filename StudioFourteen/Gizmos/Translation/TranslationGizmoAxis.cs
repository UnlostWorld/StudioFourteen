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

using Transform = StudioFourteen.Posing.Transform;
using Vector = System.Windows.Vector;

public class TranslationGizmoAxis : GizmoAxisBase
{
	private readonly Line segment;
	private readonly Line arrow;
	private readonly Vector3 segmentEnd;
	private readonly Vector3 arrowStart;

	private int strokeThickness = 3;
	private int depth = -1;

	public TranslationGizmoAxis(GizmoAxes axis, float radius, Canvas canvas)
	{
		this.Axis = axis;

		this.segment = new();
		this.segment.StrokeThickness = this.strokeThickness;
		this.segment.Stroke = this.ForegroundBrush;
		this.segment.StrokeEndLineCap = PenLineCap.Triangle;
		this.segment.StrokeStartLineCap = PenLineCap.Round;
		canvas.Children.Add(this.segment);

		if (this.Axis == GizmoAxes.X)
		{
			this.segmentEnd = -Vector3.UnitX * radius;
			this.arrowStart = -Vector3.UnitX * (radius * 0.85f);
		}
		else if (this.Axis == GizmoAxes.Y)
		{
			this.segmentEnd = -Vector3.UnitY * radius;
			this.arrowStart = -Vector3.UnitY * (radius * 0.85f);
		}
		else if (this.Axis == GizmoAxes.Z)
		{
			this.segmentEnd = -Vector3.UnitZ * radius;
			this.arrowStart = -Vector3.UnitZ * (radius * 0.85f);
		}

		this.arrow = new();
		this.arrow.StrokeThickness = 15;
		this.arrow.Stroke = this.ForegroundBrush;
		this.arrow.StrokeEndLineCap = PenLineCap.Triangle;
		this.arrow.StrokeStartLineCap = PenLineCap.Round;
		canvas.Children.Add(this.arrow);
	}

	public int StrokeThickness
	{
		get => this.strokeThickness;
		set
		{
			this.strokeThickness = value;
			this.segment.StrokeThickness = value;
		}
	}

	public override void Transform(Matrix4x4 transformMatrix, Matrix4x4 viewMatrix, Vector2 center)
	{
		Vector3 fromPoint = Vector3.Transform(Vector3.Zero, transformMatrix);
		fromPoint = Vector3.Transform(fromPoint, viewMatrix);

		Vector3 toPoint = Vector3.Transform(this.segmentEnd, transformMatrix);
		toPoint = Vector3.Transform(toPoint, viewMatrix);

		Vector2 fromPos = center + new Vector2(fromPoint.X, fromPoint.Y);
		Vector2 toPos = center + new Vector2(toPoint.X, toPoint.Y);

		this.segment.X1 = fromPos.X;
		this.segment.Y1 = fromPos.Y;
		this.segment.X2 = toPos.X;
		this.segment.Y2 = toPos.Y;
		this.segment.StrokeThickness = this.StrokeThickness;
		this.segment.Stroke = this.BackgroundBrush;

		Vector3 arrowFromPoint = Vector3.Transform(this.arrowStart, transformMatrix);
		arrowFromPoint = Vector3.Transform(arrowFromPoint, viewMatrix);
		Vector2 arrowFromPos = center + new Vector2(arrowFromPoint.X, arrowFromPoint.Y);
		this.arrow.X1 = arrowFromPos.X;
		this.arrow.Y1 = arrowFromPos.Y;
		this.arrow.X2 = toPos.X;
		this.arrow.Y2 = toPos.Y;
		this.arrow.Stroke = this.ForegroundBrush;

		this.depth = 200 - (int)(toPoint.Z * 100);
		Panel.SetZIndex(this.arrow, this.depth);
		Panel.SetZIndex(this.segment, this.depth);

		if (this.IsAxisHovered)
		{
			this.arrow.StrokeThickness = 15;
		}
		else
		{
			this.arrow.StrokeThickness = 10;
		}
	}

	public override int GetDepthAtCursor(Point mousePos)
	{
		Point fromPos = new Point(this.arrow.X1, this.arrow.Y1);
		Point toPos = new Point(this.arrow.X2, this.arrow.Y2);

		double distance = Point.Subtract(mousePos, toPos).Length;

		if (distance > 20)
			return int.MinValue;

		return this.depth;
	}

	public override void UpdateDrag(Vector mouseDelta, ref Transform deltaTransform)
	{
		Point a = new Point(this.arrow.X2, this.arrow.Y2);
		Point b = new Point(this.arrow.X1, this.arrow.Y1);
		Vector normal = a - b;
		normal.Normalize();

		double mag = mouseDelta.Length;
		mouseDelta.Normalize();

		double dot = Vector.Multiply(mouseDelta, normal);
		float dragDelta = (float)(mag * dot);

		if (double.IsNaN(dragDelta))
			return;

		dragDelta /= 50;

		if (Keyboard.Modifiers == ModifierKeys.Shift)
			dragDelta *= 10;

		if (Keyboard.Modifiers == ModifierKeys.Control)
			dragDelta /= 10;

		if (this.Services.Tablet.PenPressure > 0)
		{
			dragDelta *= (float)this.Services.Tablet.PenPressure;
		}

		Vector3 delta = Vector3.Zero;
		if (this.Axis == GizmoAxes.X)
		{
			delta = Vector3.UnitX * dragDelta;
		}
		else if (this.Axis == GizmoAxes.Y)
		{
			delta = Vector3.UnitY * dragDelta;
		}
		else if (this.Axis == GizmoAxes.Z)
		{
			delta = Vector3.UnitZ * dragDelta;
		}

		delta = Vector3.Transform(delta, deltaTransform.Rotation);
		deltaTransform.Translation += delta;
	}
}
