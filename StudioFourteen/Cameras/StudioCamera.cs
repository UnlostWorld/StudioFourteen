// .                      @@             _____ _______ _    _ _____ _____ ____
//            @       @@@@@             / ____|__   __| |  | |  __ \_   _/ __ \
//           @@@  @@@@                 | (___    | |  | |  | | |  | || || |  | |
//           @@@@@@@@@  @    @          \___ \   | |  | |  | | |  | || || |  | |
//          @@@@       @@@@@@@          ____) |  | |  | |__| | |__| || || |__| |
//      @@@@@             @@@          |_____/   |_|   \____/|_____/_____\____/
//       @@@      @@@      @@        ___     _    _   _  __   _____  ___  ___  _  _
//        @@    @@@@@@@    @@       |  _|  / _ \ | | | || _ \|_   _|| __|| __|| \| |
//        @@    @@@@@@@    @   @    | __| | (_) || |_| ||   /  | |  | _| | _| | .` |
//      @@@@      @@@      @@@@     |_|    \___/  \___/ |_|_\  |_|  |___||___||_|\_|
//       @@@@             @@@
//         @@@@@      @@@@@               This software is licensed under the
//          @@@@@@@@@@@@@@                 GNU AFFERO GENERAL PUBLIC LICENSE
//              @@@@  @                       Version 3, 19 November 2007

namespace StudioFourteen.Cameras;

using Dalamud.Plugin.Services;
using PropertyChanged.SourceGenerator;
using StudioFourteen.Mvm;
using StudioFourteen.Overlays;
using System;
using System.Numerics;
using System.Windows.Input;

public abstract partial class StudioCameraBase : ViewModel, IDisposable
{
	// a distance of 0 hides the character, so a default of 3 seems good.
	private const float DefaultCameraDistance = 3;

	private readonly CameraWireframeOverlay cameraOverlay = new("Cameras", "Camera");

	[Notify] private string name = "Default";
	[Notify] private float fieldOfView;

	[Notify(Setter.Private)] private float groupPoseFovAdjust;

	public abstract string TypeName { get; }
	public bool IsInitialized { get; set; } = false;

	public void Reset()
	{
		this.IsInitialized = false;
	}

	public virtual void Initialize(CameraState currentState)
	{
		this.IsInitialized = true;

		this.FieldOfView = 75.0f;
	}

	public virtual void Tick(float deltaTime)
	{
	}

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

	public unsafe virtual void UpdateGroupPoseCamera(GroupPoseCamera* camera)
	{
		this.GroupPoseFovAdjust = camera->FoV * 100;
	}

	public virtual void Activate()
	{
		this.cameraOverlay.Disable();
		this.Services.Input.MouseDrag += this.OnMouseDrag;
		this.Services.Input.MouseWheel += this.OnMouseWheel;
	}

	public virtual void OnFrameworkUpdate(IFramework framework)
	{
	}

	public virtual void OnRender(ref CameraState state)
	{
		this.cameraOverlay.Position = state.Position;
		this.cameraOverlay.Rotation = state.Rotation;
	}

	public virtual void Deactivate()
	{
		this.cameraOverlay.Enable();
		this.Services.Input.MouseDrag -= this.OnMouseDrag;
		this.Services.Input.MouseWheel -= this.OnMouseWheel;
	}

	public void Dispose()
	{
		this.cameraOverlay.Disable();
	}

	protected virtual void OnMouseDrag(Vector2 delta, MouseButton button)
	{
	}

	protected virtual void OnMouseWheel(float delta)
	{
	}
}
