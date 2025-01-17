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

namespace StudioFourteen.Gizmos.Handles.TransformHandle.Scale;

using StudioFourteen.Extensions;
using StudioFourteen.Gizmos.Handles.TransformHandle;
using StudioFourteen.Structs;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;

using Transform = StudioFourteen.Posing.Transform;
using Vector = System.Windows.Vector;

public class UniformScaleHandleAxis : TransformHandleAxisBase
{
	private readonly float radius;

	private Ellipse? ellipse;

	private Point startDragPos;
	private Point centerPos;

	public UniformScaleHandleAxis(float radius)
	{
		this.radius = radius;
	}

	public override void Enable(Canvas canvas)
	{
		if (this.ellipse == null)
		{
			this.ellipse = this.AddChild<Ellipse>();
			this.ellipse.Fill = this.BackgroundBrush;
			this.ellipse.Stroke = this.ForegroundBrush;
			this.ellipse.Width = this.radius * 2;
			this.ellipse.Height = this.radius * 2;
		}

		base.Enable(canvas);
	}

	public override void Update()
	{
		base.Update();

		if (this.ellipse == null)
			return;

		Vector3 pos = this.LocalToScreen(Vector3.Zero);
		Canvas.SetLeft(this.ellipse, pos.X - (this.ellipse.ActualWidth / 2));
		Canvas.SetTop(this.ellipse, pos.Y - (this.ellipse.ActualHeight / 2));
		this.SetZIndex(this.ellipse, pos.Z);

		this.centerPos = pos.ToPoint();

		this.ellipse.Fill = this.BackgroundBrush;
		this.ellipse.Stroke = this.ForegroundBrush;

		if (this.IsCursorOver)
		{
			this.ellipse.StrokeThickness = 3;
		}
		else
		{
			this.ellipse.StrokeThickness = 0;
		}
	}

	public override int HitTest(Point mousePos)
	{
		if (this.ellipse == null)
			return int.MinValue;

		float l = (float)(Canvas.GetLeft(this.ellipse) + (this.ellipse.ActualWidth / 2));
		float t = (float)(Canvas.GetTop(this.ellipse) + (this.ellipse.ActualHeight / 2));
		Point pos = new(l, t);

		double distance = Point.Subtract(mousePos, pos).Length;

		if (distance < this.radius)
			return Panel.GetZIndex(this.ellipse);

		return int.MinValue;
	}

	public override void StartDrag(Point mousePos)
	{
		this.startDragPos = mousePos;
		base.StartDrag(mousePos);
	}

	public override Transform OnDrag(Vector mouseDelta, Transform transform)
	{
		Vector normal = this.startDragPos - this.centerPos;
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

		Transform scaleTransform = Transform.FromScale(Vector3.One + (Vector3.One * dragDelta));
		transform = scaleTransform * transform;

		return transform;
	}
}