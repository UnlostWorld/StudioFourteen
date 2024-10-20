namespace StudioFourteen.Cameras;

using PropertyChanged.SourceGenerator;
using System.Numerics;

public partial class OrbitTargetCamera : OrbitCamera
{
	[Notify] private Vector3 targetOffset = new(0, 1.5f, 0);

	public override string TypeName => Resources.Find("LOC_OrbitTargetCamera", "Orbit Target");

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
