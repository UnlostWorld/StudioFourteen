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

namespace StudioFourteen.Scene.Cameras;

using StudioFourteen.Input;
using StudioFourteen.Structs.Extensions;
using StudioFourteen.Utilities;
using StudioFourteen.Xaml;
using System;
using System.Numerics;

public partial class OrbitCamera : Camera
{
	protected Vector3 desiredRot = Vector3.Zero;
	protected Vector3 desiredMove = Vector3.Zero;

	private readonly Input3DListener moveListener = new(
		InputAction.OrbitCamera_MoveRight,
		InputAction.OrbitCamera_MoveLeft,
		InputAction.OrbitCamera_MoveUp,
		InputAction.OrbitCamera_MoveDown,
		InputAction.OrbitCamera_MoveForward,
		InputAction.OrbitCamera_MoveBackward,
		"Orbit Camera Move");

	private readonly Input3DListener panListener = new(
		InputAction.OrbitCamera_PanLeft,
		InputAction.OrbitCamera_PanRight,
		InputAction.OrbitCamera_PanUp,
		InputAction.OrbitCamera_PanDown,
		InputAction.OrbitCamera_RollRight,
		InputAction.OrbitCamera_RollLeft,
		"Orbit Camera Pan");

	private readonly Input1DListener zoomListener = new(
		InputAction.OrbitCamera_ZoomOut,
		InputAction.OrbitCamera_ZoomIn,
		"Orbit Camera Zoom");

	private readonly Input2DListener rotateListener = new(
		InputAction.OrbitCamera_RotateRight,
		InputAction.OrbitCamera_RotateLeft,
		InputAction.OrbitCamera_RotateDown,
		InputAction.OrbitCamera_RotateUp);

	private float actualDistance;

	[Bind] public partial Vector3 Target { get; set; }
	[Bind] public partial float Distance { get; set; }
	[Bind] public partial Vector2 Angle { get; set; }
	[Bind] public partial Quaternion Rotation { get; set; }
	[Bind] public partial float GroupPoseRollAdjust { get; set; }

	public override string TypeName => XamlResources.Find("LOC_OrbitCamera", "Orbit");

	public override void Initialize(CameraState currentState, Camera? previousCamera)
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
			Vector3 targetPos = currentState.Position + Vector3.Transform(new Vector3(this.Distance, 0, 0), currentState.Rotation);
			this.Target = targetPos;

			if (CameraService.InitialCamera != null)
			{
				this.Angle = CameraService.InitialCamera.Value.Angle * QuaternionExtensions.Rad2Deg;
			}

			this.Rotation = Quaternion.Identity;
			this.Distance = 3;
		}
	}

	public override void Activate()
	{
		base.Activate();

		this.moveListener.Enable();
		this.panListener.Enable();
		this.zoomListener.Enable();
		this.rotateListener.Enable();
	}

	public override void Deactivate()
	{
		base.Deactivate();

		this.moveListener.Disable();
		this.panListener.Disable();
		this.zoomListener.Disable();
		this.rotateListener.Disable();
	}

	public override void OnGameTick()
	{
		base.OnGameTick();

		this.desiredMove = this.moveListener.Value;
		this.desiredRot = this.panListener.Value / 2;
		this.Distance = Math.Max(this.Distance + this.zoomListener.Value, 0.1f);
		this.Angle = MathUtility.Wrap(this.Angle + this.rotateListener.Value);
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

		this.actualDistance = float.Lerp(this.actualDistance, this.Distance, deltaTime * 8);
	}

	public unsafe override void UpdateGameCamera(GameCameraEx* camera)
	{
		base.UpdateGameCamera(camera);
		this.GroupPoseRollAdjust = camera->Rotation * QuaternionExtensions.Rad2Deg;

		camera->Angle = this.Angle * QuaternionExtensions.Deg2Rad;
		camera->Camera.Distance = this.Distance;
	}

	public override void Calculate(ref CameraState state, Camera? blend, float blendWeight)
	{
		base.Calculate(ref state, blend, blendWeight);

		Vector3 targetPos = this.Target;
		Quaternion lookRot = this.GetLookRotation();
		Quaternion rotation = this.Rotation;
		float distance = this.actualDistance;

		if (blend is OrbitCamera blendOrbit)
		{
			targetPos = Vector3.Lerp(targetPos, blendOrbit.Target, blendWeight);
			lookRot = Quaternion.Lerp(lookRot, blendOrbit.GetLookRotation(), blendWeight);
			distance = float.Lerp(distance, blendOrbit.Distance, blendWeight);
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
		Vector3 targetPos = this.Target;
		Quaternion rot = this.GetLookRotation();
		Vector3 forward = Vector3.Transform(new(1, 0, 0), rot);
		Vector3 position = targetPos + (forward * -this.actualDistance);

		return position;
	}

	public Quaternion GetLookRotation()
	{
		return Quaternion.CreateFromYawPitchRoll(
			(this.Angle.X + 90) * QuaternionExtensions.Deg2Rad,
			this.GroupPoseRollAdjust * QuaternionExtensions.Deg2Rad,
			this.Angle.Y * QuaternionExtensions.Deg2Rad);
	}

	public Quaternion GetCameraRotation()
	{
		Quaternion lookAtRot = this.GetLookRotation();
		return Quaternion.Multiply(lookAtRot, this.Rotation);
	}
}
