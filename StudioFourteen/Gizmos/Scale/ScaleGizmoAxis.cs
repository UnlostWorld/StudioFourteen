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

public class ScaleGizmoAxis : GizmoAxisBase
{
	private readonly Line line;
	private readonly Line cap;
	private readonly Vector3 segmentEnd;
	private readonly Vector3 capStart;

	public ScaleGizmoAxis(GizmoAxes axis, float radius, Canvas canvas, bool flip)
	{
		this.Axis = axis;

		this.line = new();
		this.line.StrokeThickness = 3;
		this.line.Stroke = this.ForegroundBrush;
		this.line.StrokeEndLineCap = PenLineCap.Triangle;
		this.line.StrokeStartLineCap = PenLineCap.Round;
		canvas.Children.Add(this.line);

		if (this.Axis == GizmoAxes.X)
		{
			this.segmentEnd = flip ? Vector3.UnitX * radius : -Vector3.UnitX * radius;
			this.capStart = flip ? Vector3.UnitX * (radius * 0.85f) : -Vector3.UnitX * (radius * 0.85f);
		}
		else if (this.Axis == GizmoAxes.Y)
		{
			this.segmentEnd = flip ? Vector3.UnitY * radius : -Vector3.UnitY * radius;
			this.capStart = flip ? Vector3.UnitY * (radius * 0.85f) : -Vector3.UnitY * (radius * 0.85f);
		}
		else if (this.Axis == GizmoAxes.Z)
		{
			this.segmentEnd = flip ? Vector3.UnitZ * radius : -Vector3.UnitZ * radius;
			this.capStart = flip ? Vector3.UnitZ * (radius * 0.85f) : -Vector3.UnitZ * (radius * 0.85f);
		}

		this.cap = new();
		this.cap.StrokeThickness = 15;
		this.cap.Stroke = this.ForegroundBrush;
		this.cap.StrokeEndLineCap = PenLineCap.Round;
		this.cap.StrokeStartLineCap = PenLineCap.Round;
		canvas.Children.Add(this.cap);
	}

	public override void Transform(Matrix4x4 transformMatrix, Matrix4x4 viewMatrix, Vector2 center)
	{
		Vector3 originPos = this.Transform(Vector3.Zero, center, transformMatrix, viewMatrix);

		Vector3 toPosOne = this.Transform(this.segmentEnd, center, transformMatrix, viewMatrix);
		Vector3 arrowOneFromPos = this.Transform(this.capStart, center, transformMatrix, viewMatrix);

		this.line.X1 = originPos.X;
		this.line.Y1 = originPos.Y;
		this.line.X2 = arrowOneFromPos.X;
		this.line.Y2 = arrowOneFromPos.Y;
		this.line.Stroke = toPosOne.Z < 0 ? this.ForegroundBrush : this.BackgroundBrush;
		Panel.SetZIndex(this.line, 200 - (int)(toPosOne.Z * 100));

		this.cap.X1 = arrowOneFromPos.X;
		this.cap.Y1 = arrowOneFromPos.Y;
		this.cap.X2 = toPosOne.X;
		this.cap.Y2 = toPosOne.Y;
		this.cap.Stroke = toPosOne.Z < 0 ? this.ForegroundBrush : this.BackgroundBrush;
		Panel.SetZIndex(this.cap, 200 - (int)(toPosOne.Z * 100));

		if (this.IsAxisHovered)
		{
			this.cap.StrokeThickness = 15;
		}
		else
		{
			this.cap.StrokeThickness = 10;
		}
	}

	public override int GetDepthAtCursor(Point mousePos)
	{
		Point fromPos = new Point(this.cap.X1, this.cap.Y1);
		Point toPos = new Point(this.cap.X2, this.cap.Y2);
		double distance = Point.Subtract(mousePos, toPos).Length;

		if (distance < 20)
			return Panel.GetZIndex(this.cap);

		return int.MinValue;
	}

	public override Transform UpdateDrag(Vector mouseDelta, Transform transform)
	{
		Point a = new Point(this.cap.X2, this.cap.Y2);
		Point b = new Point(this.cap.X1, this.cap.Y1);
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

		Transform scaleTransform = Posing.Transform.FromScale(Vector3.One + delta);
		transform = scaleTransform * transform;

		return transform;
	}
}
