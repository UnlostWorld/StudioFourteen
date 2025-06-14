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

namespace StudioFourteen.Gizmos.Handles.TransformHandle;

using DependencyPropertyGenerator;
using System.Numerics;
using StudioFourteen.Gizmos;
using StudioFourteen.Gizmos.Handles.TransformHandle.Rotation;
using StudioFourteen.Gizmos.Handles.TransformHandle.Scale;
using StudioFourteen.Gizmos.Handles.TransformHandle.Translation;

using Transform = StudioFourteen.Transform;

[DependencyProperty<Transform>("Transform", DefaultBindingMode = DefaultBindingMode.TwoWay)]
[DependencyProperty<double>("Sensitivity")]
[DependencyProperty<TransformHandleTypes>("GizmoType")]
[DependencyProperty<int>("GizmoIndex")]
public partial class TransformHandleControl : GizmoRenderer
{
	private readonly TranslationHandle translation;
	private readonly RotationHandle rotation;
	private readonly ScaleHandle scale;

	private Transform currentTransform;

	public TransformHandleControl()
	{
		this.translation = new();
		this.translation.TransformChanged += this.OnGizmoTransformChanged;
		this.translation.KeepScreenSize = false;
		this.AddGizmo(this.translation);

		this.rotation = new();
		this.rotation.TransformChanged += this.OnGizmoTransformChanged;
		this.rotation.KeepScreenSize = false;
		this.AddGizmo(this.rotation);

		this.scale = new();
		this.scale.TransformChanged += this.OnGizmoTransformChanged;
		this.scale.KeepScreenSize = false;
		this.AddGizmo(this.scale);

		this.OnGizmoTypeChanged(this.GizmoType);
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

	private void OnGizmoTransformChanged(Transform newTransform)
	{
		this.Transform = newTransform;
	}

	partial void OnTransformChanged(Transform newValue)
	{
		this.currentTransform = newValue;
		this.rotation.Transform = this.currentTransform;
		this.translation.Transform = this.currentTransform;
		this.scale.Transform = this.currentTransform;
	}

	partial void OnSensitivityChanged(double newValue)
	{
		this.translation.Sensitivity = newValue;
		this.rotation.Sensitivity = newValue;
		this.scale.Sensitivity = newValue;
	}

	partial void OnGizmoTypeChanged(TransformHandleTypes newValue)
	{
		this.translation.IsVisible = newValue == TransformHandleTypes.Translation;
		this.rotation.IsVisible = newValue == TransformHandleTypes.Rotation;
		this.scale.IsVisible = newValue == TransformHandleTypes.Scale;
	}

	partial void OnGizmoIndexChanged(int newValue)
	{
		this.GizmoType = (TransformHandleTypes)newValue;
	}
}
