namespace StudioFourteen.Cameras;

using PropertyChanged.SourceGenerator;
using StudioFourteen.Structs.Extensions;
using System.Numerics;

public partial class OrbitCamera : StudioCameraBase
{
	[Notify] private Vector3 target;
	[Notify] private float distance = 3;
	[Notify] private Vector3 angle;

	[Notify(Setter.Private)] private float groupPoseRollAdjust;

	public override string TypeName => Resources.Find("LOC_OrbitCamera", "Orbit");

	public override void Calculate(ref CameraState state, StudioCameraBase? blend, float blendWeight)
	{
		base.Calculate(ref state, blend, blendWeight);

		Vector3 targetPos = this.target;
		Quaternion rot = this.GetCameraRotation();
		float distance = this.distance;

		if (blend is OrbitCamera blendOrbit)
		{
			targetPos = Vector3.Lerp(targetPos, blendOrbit.target, blendWeight);
			Quaternion blendRot = blendOrbit.GetCameraRotation();

			rot = Quaternion.Lerp(rot, blendRot, blendWeight);
			distance = float.Lerp(distance, blendOrbit.distance, blendWeight);
		}

		Vector3 forward = Vector3.Transform(new(1, 0, 0), rot);
		Vector3 position = targetPos + (forward * -distance);

		if (blend is FreeCamera blendFree)
		{
			position = Vector3.Lerp(position, blendFree.Position, blendWeight);
			rot = Quaternion.Lerp(rot, blendFree.Rotation, blendWeight);
		}

		state.Position = position;
		state.Rotation = rot;
	}

	public Vector3 GetCameraPosition()
	{
		Vector3 targetPos = this.target;
		Quaternion rot = this.GetCameraRotation();
		Vector3 forward = Vector3.Transform(new(1, 0, 0), rot);
		Vector3 position = targetPos + (forward * -this.distance);

		return position;
	}

	public Quaternion GetCameraRotation()
	{
		return Quaternion.CreateFromYawPitchRoll(
			this.angle.X * QuaternionExtensions.Deg2Rad,
			(this.angle.Z + this.GroupPoseRollAdjust) * QuaternionExtensions.Deg2Rad,
			this.angle.Y * QuaternionExtensions.Deg2Rad);
	}

	public override void ImportGroupPoseSettings(float fovAdjust, float roll)
	{
		base.ImportGroupPoseSettings(fovAdjust, roll);
		this.GroupPoseRollAdjust = roll * QuaternionExtensions.Rad2Deg;
	}

	public override void Initialize(CameraState currentState)
	{
		base.Initialize(currentState);

		Vector3 targetPos = currentState.Position + Vector3.Transform(new Vector3(this.distance, 0, 0), currentState.Rotation);
		this.target = targetPos;

		// TODO: Determine a good starting angle
	}
}
