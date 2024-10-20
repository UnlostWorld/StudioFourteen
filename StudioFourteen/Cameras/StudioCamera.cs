namespace StudioFourteen.Cameras;

using PropertyChanged.SourceGenerator;
using StudioFourteen.Mvm;

public abstract partial class StudioCameraBase : ViewModel
{
	[Notify] private string name = "Default";
	[Notify] private float fieldOfView = 75.0f;

	[Notify(Setter.Private)] private float groupPoseFovAdjust;

	public abstract string TypeName { get; }

	public unsafe virtual void Calculate(ref CameraState state, StudioCameraBase? blend = null, float blendWeight = 0)
	{
		state.FieldOfView = (this.FieldOfView + this.GroupPoseFovAdjust) / 100.0f;

		if (blend is OrbitCamera blendOrbit)
		{
			state.FieldOfView = float.Lerp(
				this.FieldOfView + this.GroupPoseFovAdjust,
				blendOrbit.FieldOfView + blendOrbit.GroupPoseFovAdjust,
				blendWeight) / 100.0f;
		}
	}

	public virtual void ImportGroupPoseSettings(float fovAdjust, float roll)
	{
		this.GroupPoseFovAdjust = fovAdjust * 100;
	}
}
