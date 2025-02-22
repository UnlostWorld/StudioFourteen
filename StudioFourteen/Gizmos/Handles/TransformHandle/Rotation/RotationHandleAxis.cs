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

namespace StudioFourteen.Gizmos.Handles.TransformHandle.Rotation;

using StudioFourteen.Gizmos.Handles.TransformHandle;

using System;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

using Transform = StudioFourteen.Posing.Transform;
using Vector = System.Windows.Vector;

public class RotationHandleAxis : TransformHandleAxisBase
{
	public int StrokeThickness = 3;

	private const int NumPoints = 72;

	private readonly Line[] segments = new Line[NumPoints];
	private readonly Vector3[] points3d = new Vector3[NumPoints];

	private Point? dragStartToPos;
	private Point? dragStartFromPos;

	public RotationHandleAxis(TransformHandleAxes axis)
	{
		this.Axis = axis;
	}

	public override void Enable(Canvas canvas)
	{
		base.Enable(canvas);

		for (int i = 0; i < this.points3d.Length; i++)
		{
			float p = i / (float)(this.points3d.Length - 1);
			float r = p * (MathF.PI * 2);

			if (this.Axis == TransformHandleAxes.Z)
			{
				this.points3d[i] = new Vector3(MathF.Cos(r), MathF.Sin(r), 0);
			}
			else if (this.Axis == TransformHandleAxes.X)
			{
				this.points3d[i] = new Vector3(0, MathF.Cos(r), MathF.Sin(r));
			}
			else if (this.Axis == TransformHandleAxes.Y)
			{
				this.points3d[i] = new Vector3(MathF.Cos(r), 0, MathF.Sin(r));
			}

			this.points3d[i] *= 0.43f;
		}

		for (int i = 1; i < this.segments.Length; i++)
		{
			this.segments[i] = this.AddChild<Line>();
			this.segments[i].StrokeThickness = this.StrokeThickness;
			this.segments[i].Stroke = new SolidColorBrush(this.Foreground);
			this.segments[i].StrokeEndLineCap = PenLineCap.Round;
			this.segments[i].StrokeStartLineCap = PenLineCap.Round;
			this.segments[i].IsHitTestVisible = false;
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

	public override Transform OnDrag(Vector mouseDelta, Transform transform)
	{
		if (this.dragStartToPos == null || this.dragStartFromPos == null)
			return default;

		Vector normal = (Point)this.dragStartToPos - (Point)this.dragStartFromPos;
		normal.Normalize();

		double mag = mouseDelta.Length;
		mouseDelta.Normalize();

		double dot = Vector.Multiply(mouseDelta, normal);
		float dragDelta = (float)(mag * dot);
		double angleChange = dragDelta / 50;
		angleChange *= this.Sensitivity;

		if (Keyboard.Modifiers == ModifierKeys.Shift)
			angleChange *= 10;

		if (Keyboard.Modifiers == ModifierKeys.Control)
			angleChange /= 10;

		if (this.Services.Tablet.PenPressure > 0)
		{
			angleChange *= this.Services.Tablet.PenPressure;
		}

		Quaternion rot = Quaternion.Identity;
		if (this.Axis == TransformHandleAxes.X)
		{
			rot = Quaternion.CreateFromAxisAngle(Vector3.UnitX, (float)angleChange);
		}
		else if (this.Axis == TransformHandleAxes.Y)
		{
			rot = Quaternion.CreateFromAxisAngle(Vector3.UnitY, (float)-angleChange);
		}
		else if (this.Axis == TransformHandleAxes.Z)
		{
			rot = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, (float)angleChange);
		}

		if (transform.ToTRS(out Vector3 translation, out Quaternion rotation, out Vector3 scale))
		{
			rotation *= rot;
			transform = Transform.FromTRS(translation, rotation, scale);
		}

		return transform;
	}

	public override void EndDrag()
	{
		base.EndDrag();
		this.dragStartToPos = null;
		this.dragStartFromPos = null;
	}

	public override void Update()
	{
		base.Update();

		float zClip = this.LocalToScreen(Vector3.Zero).Z;
		for (int i = 1; i < this.points3d.Length; i++)
		{
			Vector3 fromPos = this.LocalToScreen(this.points3d[i - 1]);
			Vector3 toPos = this.LocalToScreen(this.points3d[i]);

			Line line = this.segments[i];
			line.X1 = fromPos.X;
			line.Y1 = fromPos.Y;
			line.X2 = toPos.X;
			line.Y2 = toPos.Y;
			line.IsEnabled = toPos.Z < zClip;

			this.SetZIndex(line, toPos.Z + (this.IsCursorOver ? -0.0001f : 0));

			line.StrokeThickness = this.IsCursorOver ? this.StrokeThickness + 3 : this.StrokeThickness;

			if (this.IsDragging)
			{
				line.Stroke = toPos.Z < zClip ? this.DraggingBrush : this.DraggingBackgroundBrush;
			}
			else
			{
				line.Stroke = toPos.Z < zClip ? this.ForegroundBrush : this.BackgroundBrush;
			}
		}
	}

	public void CheckAxisForMouseHover(
		Point mousePos,
		ref double closestAxisPointToMouseDistance,
		ref RotationHandleAxis? closestMouseAxis)
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

	public override int HitTest(Point mousePos)
	{
		// Rotation gizmo uses CheckAxisForMouseHover instead for more accurate
		// loop grabbing.
		throw new NotSupportedException();
	}

	public override Transform OnScrollWheel(float delta, Transform transform)
	{
		float mouseWheel = delta / 10.0f;

		Quaternion rot = Quaternion.Identity;
		if (this.Axis == TransformHandleAxes.X)
		{
			rot = Quaternion.CreateFromAxisAngle(Vector3.UnitX, mouseWheel);
		}
		else if (this.Axis == TransformHandleAxes.Y)
		{
			rot = Quaternion.CreateFromAxisAngle(Vector3.UnitY, -mouseWheel);
		}
		else if (this.Axis == TransformHandleAxes.Z)
		{
			rot = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, mouseWheel);
		}

		if (transform.ToTRS(out Vector3 translation, out Quaternion rotation, out Vector3 scale))
		{
			rotation *= rot;
			transform = Transform.FromTRS(translation, rotation, scale);
		}

		return transform;
	}
}