namespace StudioFourteen.Cameras;

using PropertyChanged.SourceGenerator;
using StudioFourteen.Mvm;
using StudioFourteen.Structs.Extensions;
using System.Numerics;

public struct CameraState
{
	public Matrix4x4 ViewMatrix;
	public float FieldOfView;
}

public abstract partial class StudioCameraBase : ViewModel
{
	[Notify] private string name = "Default";

	public unsafe abstract void Calculate(ref CameraState state, StudioCameraBase? blend = null, float blendWeight = 0);
}

public partial class OrbitCamera : StudioCameraBase
{
	[Notify] private Vector3 target;
	[Notify] private float distance = 3;
	[Notify] private Vector3 angle;
	[Notify] private float fieldOfView = 75.0f;

	[Notify(Setter.Private)] private float groupPoseFovAdjust;
	[Notify(Setter.Private)] private float groupPoseRollAdjust;

	public override void Calculate(ref CameraState state, StudioCameraBase? blend, float blendWeight)
	{
		state.FieldOfView = (this.fieldOfView + this.GroupPoseFovAdjust) / 100.0f;

		Vector3 targetPos = this.target;

		Quaternion rot = Quaternion.CreateFromYawPitchRoll(
			this.angle.X * QuaternionExtensions.Deg2Rad,
			(this.angle.Z + this.GroupPoseRollAdjust) * QuaternionExtensions.Deg2Rad,
			this.angle.Y * QuaternionExtensions.Deg2Rad);

		float distance = this.distance;

		if (blend is OrbitCamera blendOrbit)
		{
			state.FieldOfView = float.Lerp(
				this.FieldOfView + this.GroupPoseFovAdjust,
				blendOrbit.fieldOfView + blendOrbit.GroupPoseFovAdjust,
				blendWeight) / 100.0f;

			targetPos = Vector3.Lerp(targetPos, blendOrbit.target, blendWeight);

			Quaternion blendRot = Quaternion.CreateFromYawPitchRoll(
				blendOrbit.angle.X * QuaternionExtensions.Deg2Rad,
				(blendOrbit.angle.Z + blendOrbit.GroupPoseRollAdjust) * QuaternionExtensions.Deg2Rad,
				blendOrbit.angle.Y * QuaternionExtensions.Deg2Rad);

			rot = Quaternion.Lerp(rot, blendRot, blendWeight);

			distance = float.Lerp(distance, blendOrbit.distance, blendWeight);
		}

		Vector3 forward = Vector3.Transform(new(1, 0, 0), rot);
		Vector3 position = targetPos + (forward * -distance);
		Vector3 up = Vector3.Transform(new(0, 1, 0), rot);
		state.ViewMatrix = Matrix4x4.CreateLookAt(position, targetPos, up);
	}

	public void ImportGroupPoseSettings(float fovAdjust, float roll)
	{
		this.GroupPoseFovAdjust = fovAdjust * 100;
		this.GroupPoseRollAdjust = roll * QuaternionExtensions.Rad2Deg;
	}
}

public partial class OrbitTargetCamera : OrbitCamera
{
	[Notify] private Vector3 targetOffset = new(0, 1.5f, 0);

	public unsafe override void Calculate(ref CameraState state, StudioCameraBase? blend, float blendWeight)
	{
		if (this.Services.Target.HasValidTarget)
		{
			Vector3 targetPos = this.Services.Target.Target->Position;
			targetPos += this.TargetOffset;
			this.Target = targetPos;
		}

		base.Calculate(ref state, blend, blendWeight);
	}
}