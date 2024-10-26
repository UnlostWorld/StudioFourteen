namespace StudioFourteen.Cameras;

using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Common.Lua;
using PropertyChanged.SourceGenerator;
using StudioFourteen.Input;
using StudioFourteen.Mvm;
using StudioFourteen.Structs.Extensions;
using System;
using System.Numerics;
using System.Windows.Input;

public abstract partial class StudioCameraBase : ViewModel
{
	// a distance of 0 hides the character, so a default of 3 seems good.
	private const float DefaultCameraDistance = 3;
	private Vector2 lastGroupPoseCameraAngle;

	[Notify] private string name = "Default";
	[Notify] private float fieldOfView = 75.0f;

	[Notify(Setter.Private)] private float groupPoseFovAdjust;

	public abstract string TypeName { get; }
	public bool IsInitialized { get; private set; } = false;

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

		// a huge hack, but to get dragging the game window to move the cameras
		// we can use the camera angle values as a cheap input delta, since we don't
		// use them for the camera itself.
		// NOTE: Setting the camera angle back to 0 fails if the user has _just_ released
		// their mouse, causing the drag delta to keep going forever, however...
		// Using the delta from the last known angle will get stuck when the angle _would_ have
		// rotated the camera over the players head, and we want the drag to keep working after that point.
		// so... do both?
		Vector2 angleDelta = camera->Angle - this.lastGroupPoseCameraAngle;

		// Ensure the angle value has actually changed, and isn't just stuck
		// at some tiny value because the user released their mouse.
		if (Math.Abs(angleDelta.X) > 0.001 || Math.Abs(angleDelta.Y) > 0.001)
		{
			Vector2 dragDelta = camera->Angle;
			dragDelta *= QuaternionExtensions.Rad2Deg;

			// NOTE: should this be input service bindings? :think:
			if (Keyboard.IsKeyDown(Key.LeftShift))
				dragDelta *= 10;

			if (Keyboard.IsKeyDown(Key.LeftCtrl))
				dragDelta /= 10;

			this.OnMouseDrag(dragDelta);

			this.lastGroupPoseCameraAngle = camera->Angle;
		}

		// Set the angle back to zero so next frame we have clean uncapped rotation data.
		// but only if the user is still dragging.
		camera->Angle = Vector2.Zero;

		// Same for scroll wheel
		float scrollDelta = camera->Camera.Distance - DefaultCameraDistance;

		// NOTE: should this be input service bindings? :think:
		if (Keyboard.IsKeyDown(Key.LeftShift))
			scrollDelta *= 10;

		if (Keyboard.IsKeyDown(Key.LeftCtrl))
			scrollDelta /= 10;

		this.OnMouseScroll(scrollDelta);
		camera->Camera.Distance = DefaultCameraDistance;
	}

	public virtual void Activate()
	{
	}

	public virtual void OnFrameworkUpdate(IFramework framework)
	{
	}

	public virtual void Deactivate()
	{
	}

	public virtual void Initialize(CameraState currentState)
	{
		this.IsInitialized = true;
	}

	protected virtual void OnMouseDrag(Vector2 delta)
	{
	}

	protected virtual void OnMouseScroll(float delta)
	{
	}
}
