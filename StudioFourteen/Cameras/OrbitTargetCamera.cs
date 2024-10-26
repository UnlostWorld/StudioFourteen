namespace StudioFourteen.Cameras;

using PropertyChanged.SourceGenerator;
using System.Numerics;

public partial class OrbitTargetCamera : OrbitCamera
{
	[Notify] private Vector3 targetOffset = new(0, 1.5f, 0);

	public override string TypeName => Resources.Find("LOC_OrbitTargetCamera", "Orbit Target");

	public unsafe override void Tick(float deltaTime)
	{
		this.desiredMove *= deltaTime;

		Vector3 targetOffset = this.TargetOffset;
		targetOffset.Y += this.desiredMove.Y;

		Vector3 xMove = new(this.desiredMove.Z, 0, this.desiredMove.X);
		xMove = Vector3.Transform(xMove, this.GetCameraRotation());
		targetOffset += xMove;

		this.TargetOffset = targetOffset;
		this.desiredMove = Vector3.Zero;

		if (this.Services.Target.HasValidTarget)
		{
			Vector3 targetPos = this.Services.Target.Target->Position;
			targetPos += this.TargetOffset;
			this.Target = targetPos;
		}

		base.Tick(deltaTime);
	}
}
