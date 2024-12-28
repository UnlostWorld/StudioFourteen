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

namespace StudioFourteen.Gizmos;

using StudioFourteen.Gizmos.Rotation;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

public partial class RotationGizmo : GizmoBase
{
	private readonly Ellipse sphere;
	private readonly RotationGizmoAxis xAxis;
	private readonly RotationGizmoAxis yAxis;
	private readonly RotationGizmoAxis zAxis;
	private readonly Ellipse mousePrompt;

	public RotationGizmo()
	{
		this.sphere = new();
		this.sphere.Width = this.Radius * 2;
		this.sphere.Height = this.Radius * 2;
		this.sphere.Fill = new SolidColorBrush(Color.FromArgb(0x50, 0, 0, 0));
		this.Canvas.Children.Add(this.sphere);
		Panel.SetZIndex(this.sphere, 0);

		this.xAxis = new(GizmoAxes.X, this.Radius, this.Canvas);
		this.xAxis.ForegroundBrush = new SolidColorBrush(Color.FromArgb(0xFF, 0x33, 0x33, 0xFF));
		this.xAxis.BackgroundBrush = new SolidColorBrush(Color.FromArgb(0x10, 0x33, 0x33, 0xFF));
		this.AddAxis(this.xAxis);

		this.yAxis = new(GizmoAxes.Y, this.Radius, this.Canvas);
		this.yAxis.ForegroundBrush = new SolidColorBrush(Color.FromArgb(0xFF, 0x33, 0xFF, 0x33));
		this.yAxis.BackgroundBrush = new SolidColorBrush(Color.FromArgb(0x10, 0x33, 0xFF, 0x33));
		this.AddAxis(this.yAxis);

		this.zAxis = new(GizmoAxes.Z, this.Radius, this.Canvas);
		this.zAxis.ForegroundBrush = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0x33, 0x33));
		this.zAxis.BackgroundBrush = new SolidColorBrush(Color.FromArgb(0x10, 0xFF, 0x33, 0x33));
		this.AddAxis(this.zAxis);

		this.mousePrompt = new();
		this.mousePrompt.Width = 10;
		this.mousePrompt.Height = 10;
		this.Canvas.Children.Add(this.mousePrompt);
		Panel.SetZIndex(this.mousePrompt, 10000);
	}

	public float Radius { get; set; } = 70;

	/*protected override void OnMouseWheel(MouseWheelEventArgs e)
	{
		base.OnMouseWheel(e);

		if (this.closestAxisMousePos != null && this.closestMouseAxis != null)
		{
			float mouseWheel = e.Delta / 1000.0f;

			if (Keyboard.Modifiers == ModifierKeys.Shift)
				mouseWheel *= 10;

			if (Keyboard.Modifiers == ModifierKeys.Control)
				mouseWheel /= 10;

			Quaternion rot = Quaternion.Identity;
			if (this.closestMouseAxis.Axis == GizmoAxes.X)
			{
				rot = Quaternion.CreateFromAxisAngle(Vector3.UnitX, mouseWheel);
			}
			else if (this.closestMouseAxis.Axis == GizmoAxes.Y)
			{
				rot = Quaternion.CreateFromAxisAngle(Vector3.UnitY, -mouseWheel);
			}
			else if (this.closestMouseAxis.Axis == GizmoAxes.Z)
			{
				rot = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, mouseWheel);
			}

			this.Rotation = this.Rotation * rot;
			e.Handled = true;
		}
	}*/

	protected override void OnDraw(Vector2 center)
	{
		base.OnDraw(center);

		Canvas.SetLeft(this.sphere, center.X - (this.sphere.Width / 2));
		Canvas.SetTop(this.sphere, center.Y - (this.sphere.Height / 2));
	}

	protected override GizmoAxisBase? GetHoverAxis(Point mousePos)
	{
		double closestAxisPointToMouseDistance = 20;
		RotationGizmoAxis? closestMouseAxis = null;

		this.xAxis.CheckAxisForMouseHover(
			mousePos,
			ref closestAxisPointToMouseDistance,
			ref closestMouseAxis);

		this.yAxis.CheckAxisForMouseHover(
			mousePos,
			ref closestAxisPointToMouseDistance,
			ref closestMouseAxis);

		this.zAxis.CheckAxisForMouseHover(
			mousePos,
			ref closestAxisPointToMouseDistance,
			ref closestMouseAxis);

		return closestMouseAxis;
	}
}
