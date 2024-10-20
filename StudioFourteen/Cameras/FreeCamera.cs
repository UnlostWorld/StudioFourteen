namespace StudioFourteen.Cameras;

using PropertyChanged.SourceGenerator;
using System.Numerics;

public partial class FreeCamera : StudioCameraBase
{
	[Notify] private Vector3 position;
	[Notify] private Quaternion rotation;

	public override string TypeName => Resources.Find("LOC_FreeCamera", "Free Target");

	public override void Calculate(ref CameraState state, StudioCameraBase? blend = null, float blendWeight = 0)
	{
		base.Calculate(ref state, blend, blendWeight);

		state.Position = this.Position;
		state.Rotation = this.Rotation;

		if (blend is FreeCamera blendFree)
		{
			state.Position = Vector3.Lerp(state.Position, blendFree.Position, blendWeight);
			state.Rotation = Quaternion.Lerp(state.Rotation, blendFree.Rotation, blendWeight);
		}
		else if (blend is OrbitCamera blendOrbit)
		{
			state.Position = Vector3.Lerp(state.Position, blendOrbit.GetCameraPosition(), blendWeight);
			state.Rotation = Quaternion.Lerp(state.Rotation, blendOrbit.GetCameraRotation(), blendWeight);
		}
	}

	public override void Initialize(CameraState currentState)
	{
		base.Initialize(currentState);

		this.Position = currentState.Position;
		this.Rotation = currentState.Rotation;
	}
}