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

namespace StudioFourteen.Gizmos.Rotation;

using System;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

using Vector = System.Windows.Vector;

public class RotationGizmoAxis : GizmoAxisBase
{
	private const int NumPoints = 144;

	private readonly Line[] segments = new Line[NumPoints];
	private readonly Vector3[] points3d = new Vector3[NumPoints];
	private int strokeThickness = 3;

	private Point? dragStartToPos;
	private Point? dragStartFromPos;

	public RotationGizmoAxis(GizmoAxes axis, float radius, Canvas canvas)
	{
		this.Axis = axis;

		for (int i = 0; i < this.points3d.Length; i++)
		{
			float p = i / (float)(this.points3d.Length - 1);
			float r = p * (MathF.PI * 2);

			if (axis == GizmoAxes.Z)
			{
				this.points3d[i] = new Vector3(radius * MathF.Cos(r), radius * MathF.Sin(r), 0);
			}
			else if (axis == GizmoAxes.X)
			{
				this.points3d[i] = new Vector3(0, radius * MathF.Cos(r), radius * MathF.Sin(r));
			}
			else if (axis == GizmoAxes.Y)
			{
				this.points3d[i] = new Vector3(radius * MathF.Cos(r), 0, radius * MathF.Sin(r));
			}
		}

		for (int i = 1; i < this.segments.Length; i++)
		{
			this.segments[i] = new();
			this.segments[i].StrokeThickness = this.strokeThickness;
			this.segments[i].Stroke = this.ForegroundBrush;
			this.segments[i].StrokeEndLineCap = PenLineCap.Round;
			this.segments[i].StrokeStartLineCap = PenLineCap.Round;
			canvas.Children.Add(this.segments[i]);
		}
	}

	public int StrokeThickness
	{
		get => this.strokeThickness;
		set
		{
			this.strokeThickness = value;
			foreach (Line line in this.segments)
			{
				line.StrokeThickness = value;
			}
		}
	}

	public override void StartDrag(Point mousePos)
	{
		base.StartDrag(mousePos);

		double closestDist = double.MaxValue;

		for (int i = 1; i < this.points3d.Length; i++)
		{
			Line segment = this.segments[i];

			if (!segment.IsEnabled)
				continue;

			Point fromPos = new Point(segment.X1, segment.Y1);
			Point toPos = new Point(segment.X2, segment.Y2);

			double distance = Point.Subtract(mousePos, toPos).Length;

			if (distance <= closestDist)
			{
				closestDist = distance;
				this.dragStartToPos = toPos;
				this.dragStartFromPos = fromPos;
			}
		}
	}

	public override void UpdateDrag(Vector mouseDelta, ref Posing.Transform transform)
	{
		if (this.dragStartToPos == null || this.dragStartFromPos == null)
			return;

		Vector normal = (Point)this.dragStartToPos - (Point)this.dragStartFromPos;
		normal.Normalize();

		double mag = mouseDelta.Length;
		mouseDelta.Normalize();

		double dot = Vector.Multiply(mouseDelta, normal);
		float dragDelta = (float)(mag * dot);

		if (double.IsNaN(dragDelta))
			return;

		double angleChange = dragDelta / 50;

		if (Keyboard.Modifiers == ModifierKeys.Shift)
			angleChange *= 10;

		if (Keyboard.Modifiers == ModifierKeys.Control)
			angleChange /= 10;

		if (this.Services.Tablet.PenPressure > 0)
		{
			angleChange *= this.Services.Tablet.PenPressure;
		}

		Quaternion rot = Quaternion.Identity;
		if (this.Axis == GizmoAxes.X)
		{
			rot = Quaternion.CreateFromAxisAngle(Vector3.UnitX, (float)angleChange);
		}
		else if (this.Axis == GizmoAxes.Y)
		{
			rot = Quaternion.CreateFromAxisAngle(Vector3.UnitY, (float)-angleChange);
		}
		else if (this.Axis == GizmoAxes.Z)
		{
			rot = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, (float)angleChange);
		}

		transform.Rotation *= rot;
	}

	public override void EndDrag()
	{
		base.EndDrag();
		this.dragStartToPos = null;
		this.dragStartFromPos = null;
	}

	public override void Transform(Matrix4x4 transformMatrix, Matrix4x4 viewMatrix, Vector2 center)
	{
		float zClip = 0f;
		for (int i = 1; i < this.points3d.Length; i++)
		{
			Vector3 fromPoint = Vector3.Transform(this.points3d[i - 1], transformMatrix);
			fromPoint = Vector3.Transform(fromPoint, viewMatrix);

			Vector3 toPoint = Vector3.Transform(this.points3d[i], transformMatrix);
			toPoint = Vector3.Transform(toPoint, viewMatrix);

			bool isVisible = toPoint.Z < zClip;
			Vector2 fromPos = center + new Vector2(fromPoint.X, fromPoint.Y);
			Vector2 toPos = center + new Vector2(toPoint.X, toPoint.Y);

			Line line = this.segments[i];
			line.X1 = fromPos.X;
			line.Y1 = fromPos.Y;
			line.X2 = toPos.X;
			line.Y2 = toPos.Y;
			line.IsEnabled = isVisible;

			Panel.SetZIndex(line, isVisible ? 200 : 100);

			line.StrokeThickness = this.IsAxisHovered ? this.StrokeThickness + 1 : this.StrokeThickness;
			line.Stroke = isVisible ? this.ForegroundBrush : this.BackgroundBrush;
		}
	}

	public void CheckAxisForMouseHover(
		Point mousePos,
		ref double closestAxisPointToMouseDistance,
		ref RotationGizmoAxis? closestMouseAxis)
	{
		for (int i = 1; i < this.points3d.Length; i++)
		{
			Line segment = this.segments[i];

			if (!segment.IsEnabled)
				continue;

			Point fromPos = new Point(segment.X1, segment.Y1);
			Point toPos = new Point(segment.X2, segment.Y2);

			double distance = Point.Subtract(mousePos, toPos).Length;

			if (distance <= closestAxisPointToMouseDistance)
			{
				closestAxisPointToMouseDistance = distance;
				closestMouseAxis = this;
			}
		}
	}

	public override int GetDepthAtCursor(Point mousePos)
	{
		// Rotation gizmo uses CheckAxisForMouseHover instead for more accurate
		// loop grabbing.
		throw new NotSupportedException();
	}
}