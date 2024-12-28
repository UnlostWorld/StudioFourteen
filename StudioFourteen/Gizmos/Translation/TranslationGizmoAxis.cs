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
	private readonly Line lineOne;
	private readonly Line lineTwo;
	private readonly Line arrowOne;
	private readonly Line arrowTwo;
	private readonly Vector3 segmentOneEnd;
	private readonly Vector3 segmentTwoEnd;
	private readonly Vector3 arrowOneStart;
	private readonly Vector3 arrowTwoStart;

	public TranslationGizmoAxis(GizmoAxes axis, float radius, Canvas canvas)
	{
		this.Axis = axis;

		this.lineOne = new();
		this.lineOne.StrokeThickness = 3;
		this.lineOne.Stroke = this.ForegroundBrush;
		this.lineOne.StrokeEndLineCap = PenLineCap.Triangle;
		this.lineOne.StrokeStartLineCap = PenLineCap.Round;
		canvas.Children.Add(this.lineOne);

		this.lineTwo = new();
		this.lineTwo.StrokeThickness = 3;
		this.lineTwo.Stroke = this.ForegroundBrush;
		this.lineTwo.StrokeEndLineCap = PenLineCap.Triangle;
		this.lineTwo.StrokeStartLineCap = PenLineCap.Round;
		canvas.Children.Add(this.lineTwo);

		if (this.Axis == GizmoAxes.X)
		{
			this.segmentOneEnd = -Vector3.UnitX * radius;
			this.segmentTwoEnd = Vector3.UnitX * radius;
			this.arrowOneStart = -Vector3.UnitX * (radius * 0.85f);
			this.arrowTwoStart = Vector3.UnitX * (radius * 0.85f);
		}
		else if (this.Axis == GizmoAxes.Y)
		{
			this.segmentOneEnd = -Vector3.UnitY * radius;
			this.segmentTwoEnd = Vector3.UnitY * radius;
			this.arrowOneStart = -Vector3.UnitY * (radius * 0.85f);
			this.arrowTwoStart = Vector3.UnitY * (radius * 0.85f);
		}
		else if (this.Axis == GizmoAxes.Z)
		{
			this.segmentOneEnd = -Vector3.UnitZ * radius;
			this.segmentTwoEnd = Vector3.UnitZ * radius;
			this.arrowOneStart = -Vector3.UnitZ * (radius * 0.85f);
			this.arrowTwoStart = Vector3.UnitZ * (radius * 0.85f);
		}

		this.arrowOne = new();
		this.arrowOne.StrokeThickness = 15;
		this.arrowOne.Stroke = this.ForegroundBrush;
		this.arrowOne.StrokeEndLineCap = PenLineCap.Triangle;
		this.arrowOne.StrokeStartLineCap = PenLineCap.Round;
		canvas.Children.Add(this.arrowOne);

		this.arrowTwo = new();
		this.arrowTwo.StrokeThickness = 15;
		this.arrowTwo.Stroke = this.ForegroundBrush;
		this.arrowTwo.StrokeEndLineCap = PenLineCap.Triangle;
		this.arrowTwo.StrokeStartLineCap = PenLineCap.Round;
		canvas.Children.Add(this.arrowTwo);
	}

	public override void Transform(Matrix4x4 transformMatrix, Matrix4x4 viewMatrix, Vector2 center)
	{
		Vector3 originPos = this.Transform(Vector3.Zero, center, transformMatrix, viewMatrix);

		Vector3 toPosOne = this.Transform(this.segmentOneEnd, center, transformMatrix, viewMatrix);
		Vector3 arrowOneFromPos = this.Transform(this.arrowOneStart, center, transformMatrix, viewMatrix);

		this.lineOne.X1 = originPos.X;
		this.lineOne.Y1 = originPos.Y;
		this.lineOne.X2 = arrowOneFromPos.X;
		this.lineOne.Y2 = arrowOneFromPos.Y;
		this.lineOne.Stroke = toPosOne.Z < 0 ? this.ForegroundBrush : this.BackgroundBrush;
		Panel.SetZIndex(this.lineOne, 200 - (int)(toPosOne.Z * 100));

		this.arrowOne.X1 = arrowOneFromPos.X;
		this.arrowOne.Y1 = arrowOneFromPos.Y;
		this.arrowOne.X2 = toPosOne.X;
		this.arrowOne.Y2 = toPosOne.Y;
		this.arrowOne.Stroke = toPosOne.Z < 0 ? this.ForegroundBrush : this.BackgroundBrush;
		Panel.SetZIndex(this.arrowOne, 200 - (int)(toPosOne.Z * 100));

		// Two
		Vector3 toPosTwo = this.Transform(this.segmentTwoEnd, center, transformMatrix, viewMatrix);
		Vector3 arrowTwoFromPos = this.Transform(this.arrowTwoStart, center, transformMatrix, viewMatrix);
		this.lineTwo.X1 = originPos.X;
		this.lineTwo.Y1 = originPos.Y;
		this.lineTwo.X2 = arrowTwoFromPos.X;
		this.lineTwo.Y2 = arrowTwoFromPos.Y;
		this.lineTwo.Stroke = toPosTwo.Z < 0 ? this.ForegroundBrush : this.BackgroundBrush;
		Panel.SetZIndex(this.lineTwo, 200 - (int)(toPosTwo.Z * 100));

		this.arrowTwo.X1 = arrowTwoFromPos.X;
		this.arrowTwo.Y1 = arrowTwoFromPos.Y;
		this.arrowTwo.X2 = toPosTwo.X;
		this.arrowTwo.Y2 = toPosTwo.Y;
		this.arrowTwo.Stroke = toPosTwo.Z < 0 ? this.ForegroundBrush : this.BackgroundBrush;
		Panel.SetZIndex(this.arrowTwo, 200 - (int)(toPosTwo.Z * 100));

		if (this.IsAxisHovered)
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

	public override int GetDepthAtCursor(Point mousePos)
	{
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

	public override void UpdateDrag(Vector mouseDelta, ref Transform deltaTransform)
	{
		Point a = new Point(this.arrowOne.X2, this.arrowOne.Y2);
		Point b = new Point(this.arrowOne.X1, this.arrowOne.Y1);
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
