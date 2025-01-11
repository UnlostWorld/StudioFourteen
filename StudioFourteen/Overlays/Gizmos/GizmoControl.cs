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

namespace StudioFourteen.Overlays.Gizmos;

using DependencyPropertyGenerator;
using StudioFourteen.Overlays.Gizmos.Rotation;
using StudioFourteen.Overlays.Primitives;
using StudioFourteen.Posing;
using System.Numerics;

[DependencyProperty<Posing.Transform>("Transform", DefaultBindingMode=DefaultBindingMode.TwoWay)]
[DependencyProperty<double>("Sensitivity")]
public partial class GizmoControl : PrimitiveRenderer
{
	private readonly RotationGizmo rotation;

	private Transform currentTransform;

	public GizmoControl()
	{
		this.rotation = new();
		this.rotation.TransformChanged += this.OnRotationTransformChanged;
		this.rotation.KeepScreenSize = false;
		this.AddPrimitive(this.rotation);
	}

	protected override Matrix4x4 GetProjectionMatrix()
	{
		return Matrix4x4.CreateOrthographic(1, 1, 0.1f, 100);
	}

	protected override Matrix4x4 GetViewMatrix()
	{
		if (Matrix4x4.Decompose(this.Services.Camera.CurrentView, out Vector3 cameraScale, out Quaternion cameraRotation, out Vector3 cameraTranslation))
		{
			Vector3 center = Vector3.Transform(Vector3.Zero, this.currentTransform.ToMatrix());
			Matrix4x4 translate = Matrix4x4.CreateTranslation(-center);
			Matrix4x4 rot = Matrix4x4.CreateFromQuaternion(new Quaternion(-cameraRotation.X, -cameraRotation.Y, -cameraRotation.Z, -cameraRotation.W));
			Matrix4x4 view = Matrix4x4.CreateLookAt(Vector3.UnitZ, Vector3.Zero, -Vector3.UnitY);
			view = translate * rot * view;
			return view;
		}

		return Matrix4x4.Identity;
	}

	private void OnRotationTransformChanged(Posing.Transform newTransform)
	{
		this.Transform = newTransform;
	}

	partial void OnTransformChanged(Transform newValue)
	{
		this.currentTransform = newValue;
		this.rotation.Transform = this.currentTransform;
	}

	partial void OnSensitivityChanged(double newValue)
	{
		this.rotation.Sensitivity = newValue;
	}
}
