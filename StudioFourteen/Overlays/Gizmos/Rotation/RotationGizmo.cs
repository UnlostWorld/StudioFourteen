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

namespace StudioFourteen.Overlays.Gizmos.Rotation;

using StudioFourteen.Overlays.Gizmos;
using StudioFourteen.Overlays.Primitives;
using System.Windows;
using System.Windows.Media;

public partial class RotationGizmo : GizmoBase
{
	private readonly EllipsePrimitive sphere;
	private readonly RotationGizmoAxis xAxis;
	private readonly RotationGizmoAxis yAxis;
	private readonly RotationGizmoAxis zAxis;

	public RotationGizmo()
	{
		this.sphere = new();
		this.sphere.Foreground = Color.FromArgb(0x80, 0, 0, 0);
		this.sphere.Radius = 80;
		this.AddChild(this.sphere);

		this.xAxis = new(GizmoAxes.X);
		this.xAxis.Foreground = Color.FromArgb(0xFF, 0x33, 0x33, 0xFF);
		this.xAxis.Background = Color.FromArgb(0xFF, 0x33, 0x33, 0x4D);
		this.AddChild(this.xAxis);

		this.yAxis = new(GizmoAxes.Y);
		this.yAxis.Foreground = Color.FromArgb(0xFF, 0x33, 0xFF, 0x33);
		this.yAxis.Background = Color.FromArgb(0xFF, 0x33, 0x4D, 0x33);
		this.AddChild(this.yAxis);

		this.zAxis = new(GizmoAxes.Z);
		this.zAxis.Foreground = Color.FromArgb(0xFF, 0xFF, 0x33, 0x33);
		this.zAxis.Background = Color.FromArgb(0xFF, 0x4D, 0x33, 0x33);
		this.AddChild(this.zAxis);
	}

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
	}

	protected override void OnDraw(Vector2 center)
	{
		base.OnDraw(center);

		Canvas.SetLeft(this.sphere, center.X - (this.sphere.Width / 2));
		Canvas.SetTop(this.sphere, center.Y - (this.sphere.Height / 2));
	}*/

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
