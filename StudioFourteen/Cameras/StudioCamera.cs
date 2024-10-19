namespace StudioFourteen.Cameras;

using PropertyChanged.SourceGenerator;
using StudioFourteen.Mvm;
using StudioFourteen.Structs.Extensions;
using System.Numerics;

public abstract partial class StudioCameraBase : ViewModel
{
	[Notify] private string name = "Default";
	[Notify] private float fieldOfView = 0.75f;

	public unsafe abstract Matrix4x4 Calculate();
}

public partial class OrbitCamera : StudioCameraBase
{
	[Notify] private Vector3 target;
	[Notify] private float distance = 3;
	[Notify] private Vector3 angle;

	public override Matrix4x4 Calculate()
	{
		Quaternion rot = Quaternion.CreateFromYawPitchRoll(
			this.angle.X * QuaternionExtensions.Deg2Rad,
			this.angle.Z * QuaternionExtensions.Deg2Rad,
			this.angle.Y * QuaternionExtensions.Deg2Rad);

		Vector3 forward = Vector3.Transform(new(1, 0, 0), rot);
		Vector3 position = this.target + (forward * -this.distance);
		Vector3 up = Vector3.Transform(new(0, 1, 0), rot);

		return Matrix4x4.CreateLookAt(position, this.target, up);
	}
}

public partial class OrbitTargetCamera : OrbitCamera
{
	[Notify] private Vector3 targetOffset = new(0, 1.5f, 0);

	public unsafe override Matrix4x4 Calculate()
	{
		this.Target = this.Services.Target.Target->Position;
		this.Target = this.Target + this.TargetOffset;

		return base.Calculate();
	}
}