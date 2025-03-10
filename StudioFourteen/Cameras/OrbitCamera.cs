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

namespace StudioFourteen.Cameras;

using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Common.Lua;
using PropertyChanged.SourceGenerator;
using StudioFourteen.Input;
using StudioFourteen.Overlays;
using StudioFourteen.Structs.Extensions;
using StudioFourteen.Utilities;
using System;
using System.Numerics;
using System.Windows.Input;

public partial class OrbitCamera : StudioCameraBase
{
	protected Vector3 desiredRot = Vector3.Zero;
	protected Vector3 desiredMove = Vector3.Zero;

	private readonly InputActionListener moveUpListener = new(InputAction.OrbitCamera_MoveUp);
	private readonly InputActionListener moveDownListener = new(InputAction.OrbitCamera_MoveDown);
	private readonly InputActionListener moveLeftListener = new(InputAction.OrbitCamera_MoveLeft);
	private readonly InputActionListener moveRightListener = new(InputAction.OrbitCamera_MoveRight);
	private readonly InputActionListener moveForwardListener = new (InputAction.OrbitCamera_MoveForward);
	private readonly InputActionListener moveBackwardListener = new(InputAction.OrbitCamera_MoveBackward);
	private readonly InputActionListener panUpListener = new(InputAction.OrbitCamera_PanUp);
	private readonly InputActionListener panDownListener = new(InputAction.OrbitCamera_PanDown);
	private readonly InputActionListener panLeftListener = new(InputAction.OrbitCamera_PanLeft);
	private readonly InputActionListener panRightListener = new(InputAction.OrbitCamera_PanRight);
	private readonly InputActionListener rollLeftListener = new(InputAction.OrbitCamera_RollLeft);
	private readonly InputActionListener rollRightListener = new(InputAction.OrbitCamera_RollRight);
	private readonly InputActionListener zoomInListener = new(InputAction.OrbitCamera_ZoomIn);
	private readonly InputActionListener zoomOutListener = new(InputAction.OrbitCamera_ZoomOut);
	private readonly InputActionListener rotateLeftListener = new(InputAction.OrbitCamera_RotateLeft);
	private readonly InputActionListener rotateRightListener = new(InputAction.OrbitCamera_RotateRight);
	private readonly InputActionListener rotateUpListener = new(InputAction.OrbitCamera_RotateUp);
	private readonly InputActionListener rotateDownListener = new(InputAction.OrbitCamera_RotateDown);

	private float actualDistance;

	[Notify] private Vector3 target;
	[Notify] private float distance;
	[Notify] private Vector2 angle;
	[Notify] private Quaternion rotation;

	[PropertyAttribute("[Newtonsoft.Json.JsonIgnore]")]
	[Notify(Setter.Private)]
	private float groupPoseRollAdjust;

	public override string TypeDisplayName => Resources.Find("LOC_OrbitCamera", "Orbit");

	public override void Initialize(CameraState currentState, StudioCameraBase? previousCamera)
	{
		base.Initialize(currentState, previousCamera);

		if (previousCamera is OrbitCamera previousOrbit)
		{
			this.Target = previousOrbit.Target;
			this.Distance = previousOrbit.Distance;
			this.Angle = previousOrbit.Angle;
			this.Rotation = previousOrbit.Rotation;
		}
		else
		{
			Vector3 targetPos = currentState.Position + Vector3.Transform(new Vector3(this.distance, 0, 0), currentState.Rotation);
			this.target = targetPos;

			if (this.Services.Camera.InitialCamera != null)
			{
				this.Angle = this.Services.Camera.InitialCamera.Value.Angle * QuaternionExtensions.Rad2Deg;
			}

			this.Rotation = Quaternion.Identity;
			this.Distance = 3;
		}
	}

	public override void Activate()
	{
		base.Activate();

		this.moveUpListener.Enable();
		this.moveDownListener.Enable();
		this.moveLeftListener.Enable();
		this.moveRightListener.Enable();
		this.moveForwardListener.Enable();
		this.moveBackwardListener.Enable();
		this.panUpListener.Enable();
		this.panDownListener.Enable();
		this.panLeftListener.Enable();
		this.panRightListener.Enable();
		this.rollLeftListener.Enable();
		this.rollRightListener.Enable();
		this.zoomInListener.Enable();
		this.zoomOutListener.Enable();
		this.rotateLeftListener.Enable();
		this.rotateRightListener.Enable();
		this.rotateUpListener.Enable();
		this.rotateDownListener.Enable();
	}

	public override void Deactivate()
	{
		base.Deactivate();

		this.moveUpListener.Disable();
		this.moveDownListener.Disable();
		this.moveLeftListener.Disable();
		this.moveRightListener.Disable();
		this.moveForwardListener.Disable();
		this.moveBackwardListener.Disable();
		this.panUpListener.Disable();
		this.panDownListener.Disable();
		this.panLeftListener.Disable();
		this.panRightListener.Disable();
		this.rollLeftListener.Disable();
		this.rollRightListener.Disable();
		this.zoomInListener.Disable();
		this.zoomOutListener.Disable();
		this.rotateLeftListener.Disable();
		this.rotateRightListener.Disable();
		this.rotateUpListener.Disable();
		this.rotateDownListener.Disable();
	}

	public override void OnFrameworkUpdate(IFramework framework)
	{
		base.OnFrameworkUpdate(framework);

		Vector3 moveDir = Vector3.Zero;
		moveDir.Y += this.moveUpListener.Value;
		moveDir.Y -= this.moveDownListener.Value;
		moveDir.X -= this.moveLeftListener.Value;
		moveDir.X += this.moveRightListener.Value;
		moveDir.Z += this.moveForwardListener.Value;
		moveDir.Z -= this.moveBackwardListener.Value;
		this.desiredMove = moveDir;

		Vector3 rot = Vector3.Zero;
		rot.X += this.panLeftListener.Value / 2;
		rot.X -= this.panRightListener.Value / 2;
		rot.Y += this.panUpListener.Value / 2;
		rot.Y -= this.panDownListener.Value / 2;
		rot.Z -= this.rollLeftListener.Value;
		rot.Z += this.rollRightListener.Value;
		this.desiredRot = rot;

		float d = this.distance;
		d -= this.zoomInListener.Value;
		d += this.zoomOutListener.Value;
		this.Distance = Math.Max(d, 0.1f);

		Vector2 angle = this.Angle;
		angle.X -= this.rotateLeftListener.Value;
		angle.X += this.rotateRightListener.Value;
		angle.Y -= this.rotateUpListener.Value;
		angle.Y += this.rotateDownListener.Value;
		angle = MathUtility.Wrap(angle);
		this.Angle = angle;
	}

	public override void Tick(float deltaTime)
	{
		base.Tick(deltaTime);

		this.desiredMove *= deltaTime;
		this.Target += this.desiredMove;
		this.desiredMove = Vector3.Zero;

		this.desiredRot *= deltaTime;
		Quaternion x = Quaternion.CreateFromYawPitchRoll(this.desiredRot.X, 0, 0);
		Quaternion y = Quaternion.CreateFromYawPitchRoll(0, this.desiredRot.Z, this.desiredRot.Y);
		this.Rotation = Quaternion.Multiply(x, this.Rotation);
		this.Rotation = Quaternion.Multiply(this.Rotation, y);
		this.desiredRot = Vector3.Zero;

		this.actualDistance = float.Lerp(this.actualDistance, this.distance, deltaTime * 8);
	}

	public unsafe override void UpdateGroupPoseCamera(GroupPoseCamera* camera)
	{
		base.UpdateGroupPoseCamera(camera);
		this.GroupPoseRollAdjust = camera->Rotation * QuaternionExtensions.Rad2Deg;

		camera->Angle = this.Angle * QuaternionExtensions.Deg2Rad;
		camera->Camera.Distance = this.distance;
	}

	public override void Calculate(ref CameraState state, StudioCameraBase? blend, float blendWeight)
	{
		base.Calculate(ref state, blend, blendWeight);

		Vector3 targetPos = this.target;
		Quaternion lookRot = this.GetLookRotation();
		Quaternion rotation = this.Rotation;
		float distance = this.actualDistance;

		if (blend is OrbitCamera blendOrbit)
		{
			targetPos = Vector3.Lerp(targetPos, blendOrbit.target, blendWeight);
			lookRot = Quaternion.Lerp(lookRot, blendOrbit.GetLookRotation(), blendWeight);
			distance = float.Lerp(distance, blendOrbit.distance, blendWeight);
			rotation = Quaternion.Lerp(rotation, blendOrbit.Rotation, blendWeight);
		}

		Vector3 forward = Vector3.Transform(new(1, 0, 0), lookRot);
		Vector3 position = targetPos + (forward * -distance);

		if (blend is FreeCamera blendFree)
		{
			position = Vector3.Lerp(position, blendFree.Position, blendWeight);
			lookRot = Quaternion.Lerp(lookRot, blendFree.Rotation, blendWeight);
			rotation = Quaternion.Lerp(rotation, Quaternion.Identity, blendWeight);
		}

		state.Position = position;
		state.Rotation = Quaternion.Multiply(lookRot, rotation);
	}

	public Vector3 GetCameraPosition()
	{
		Vector3 targetPos = this.target;
		Quaternion rot = this.GetLookRotation();
		Vector3 forward = Vector3.Transform(new(1, 0, 0), rot);
		Vector3 position = targetPos + (forward * -this.actualDistance);

		return position;
	}

	public Quaternion GetLookRotation()
	{
		return Quaternion.CreateFromYawPitchRoll(
			(this.angle.X + 90) * QuaternionExtensions.Deg2Rad,
			this.GroupPoseRollAdjust * QuaternionExtensions.Deg2Rad,
			this.angle.Y * QuaternionExtensions.Deg2Rad);
	}

	public Quaternion GetCameraRotation()
	{
		Quaternion lookAtRot = this.GetLookRotation();
		return Quaternion.Multiply(lookAtRot, this.Rotation);
	}
}
