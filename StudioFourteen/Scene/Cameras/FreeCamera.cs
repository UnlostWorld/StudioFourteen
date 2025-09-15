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
using System;
using System.Numerics;

public partial class FreeCamera : Camera
{
	private const float MoveSpeedMultiplier = 1.01f;
	private const float MoveSpeed = 2.0f;
	private const float MoveSpeedMaximum = 200.0f;

	private readonly Input0DListener moveForwardsListener = new(InputAction.FreeCamera_MoveForwards);
	private readonly Input0DListener moveBackListener = new(InputAction.FreeCamera_MoveBack);
	private readonly Input0DListener moveUpListener = new(InputAction.FreeCamera_MoveUp);
	private readonly Input0DListener moveDownListener = new(InputAction.FreeCamera_MoveDown);
	private readonly Input0DListener moveLeftListener = new(InputAction.FreeCamera_MoveLeft);
	private readonly Input0DListener moveRightListener = new(InputAction.FreeCamera_MoveRight);
	private readonly Input0DListener yawLeftListener = new(InputAction.FreeCamera_YawLeft);
	private readonly Input0DListener yawRightListener = new(InputAction.FreeCamera_YawRight);
	private readonly Input0DListener pitchUpListener = new(InputAction.FreeCamera_PitchUp);
	private readonly Input0DListener pitchDownListener = new(InputAction.FreeCamera_PitchDown);
	private readonly Input0DListener rollLeftListener = new(InputAction.FreeCamera_RollLeft);
	private readonly Input0DListener rollRightListener = new(InputAction.FreeCamera_RollRight);
	private readonly Input0DListener rotateLeftListener = new(InputAction.FreeCamera_RotateLeft);
	private readonly Input0DListener rotateRightListener = new(InputAction.FreeCamera_RotateRight);
	private readonly Input0DListener rotateUpListener = new(InputAction.FreeCamera_RotateUp);
	private readonly Input0DListener rotateDownListener = new(InputAction.FreeCamera_RotateDown);

	private Vector3 desiredMove = Vector3.Zero;
	private Vector3 desiredRot = Vector3.Zero;
	private float moveSpeed = 2.0f;

	public override string TypeName => Resources.Find("LOC_FreeCamera", "Free Camera");

	[Bind] public partial Vector3 Position { get; set; }
	[Bind] public partial Quaternion Rotation { get; set; }

	public override void Initialize(CameraState currentState, Camera? previousCamera)
	{
		base.Initialize(currentState, previousCamera);

		this.Position = currentState.Position;
		this.Rotation = currentState.Rotation;
	}

	public override void Activate()
	{
		base.Activate();

		this.moveForwardsListener.Enable();
		this.moveBackListener.Enable();
		this.moveUpListener.Enable();
		this.moveDownListener.Enable();
		this.moveLeftListener.Enable();
		this.moveRightListener.Enable();
		this.yawLeftListener.Enable();
		this.yawRightListener.Enable();
		this.pitchUpListener.Enable();
		this.pitchDownListener.Enable();
		this.rollLeftListener.Enable();
		this.rollRightListener.Enable();
		this.rotateLeftListener.Enable();
		this.rotateRightListener.Enable();
		this.rotateUpListener.Enable();
		this.rotateDownListener.Enable();
	}

	public override void Deactivate()
	{
		base.Deactivate();

		this.moveForwardsListener.Disable();
		this.moveBackListener.Disable();
		this.moveUpListener.Disable();
		this.moveDownListener.Disable();
		this.moveLeftListener.Disable();
		this.moveRightListener.Disable();
		this.yawLeftListener.Disable();
		this.yawRightListener.Disable();
		this.pitchUpListener.Disable();
		this.pitchDownListener.Disable();
		this.rollLeftListener.Disable();
		this.rollRightListener.Disable();
		this.rotateLeftListener.Disable();
		this.rotateRightListener.Disable();
		this.rotateUpListener.Disable();
		this.rotateDownListener.Disable();
	}

	public override unsafe void UpdateGameCamera(GameCameraEx* camera)
	{
		base.UpdateGameCamera(camera);

		camera->Camera.Distance = float.MaxValue;
	}

	public override void OnGameTick()
	{
		base.OnGameTick();

		Vector3 moveDir = Vector3.Zero;
		moveDir.X += this.moveForwardsListener.Value;
		moveDir.X -= this.moveBackListener.Value;
		moveDir.Z -= this.moveLeftListener.Value;
		moveDir.Z += this.moveRightListener.Value;
		moveDir.Y += this.moveUpListener.Value;
		moveDir.Y -= this.moveDownListener.Value;
		this.desiredMove = moveDir;

		Vector3 rot = Vector3.Zero;
		rot.X += this.yawLeftListener.Value;
		rot.X -= this.yawRightListener.Value;
		rot.Y += this.pitchUpListener.Value;
		rot.Y -= this.pitchDownListener.Value;
		rot.Z -= this.rollLeftListener.Value;
		rot.Z += this.rollRightListener.Value;
		this.desiredRot = rot;

		float x = (-this.rotateLeftListener.Value + this.rotateRightListener.Value) / 2;
		float y = (-this.rotateUpListener.Value + this.rotateDownListener.Value) / 2;
		Quaternion xRot = Quaternion.CreateFromYawPitchRoll(x * QuaternionExtensions.Deg2Rad, 0, 0);
		Quaternion yRot = Quaternion.CreateFromYawPitchRoll(0, 0, y * QuaternionExtensions.Deg2Rad);
		this.Rotation = Quaternion.Multiply(xRot, this.Rotation);
		this.Rotation = Quaternion.Multiply(this.Rotation, yRot);
	}

	public override void Tick(float deltaTime)
	{
		base.Tick(deltaTime);

		if (this.desiredMove.X != 0 || this.desiredMove.Y != 0 || this.desiredMove.Z != 0)
		{
			float newSpeed = this.moveSpeed * MoveSpeedMultiplier;
			newSpeed *= deltaTime;
			this.moveSpeed += newSpeed;
			this.moveSpeed = MathF.Min(MoveSpeedMaximum, this.moveSpeed);
		}
		else
		{
			this.moveSpeed = MoveSpeed;
		}

		this.desiredMove *= deltaTime;
		this.desiredMove *= this.moveSpeed;
		this.desiredMove = Vector3.Transform(this.desiredMove, this.Rotation);
		this.Position += this.desiredMove;
		this.desiredMove = Vector3.Zero;

		this.desiredRot *= deltaTime;
		Quaternion x = Quaternion.CreateFromYawPitchRoll(this.desiredRot.X, 0, 0);
		Quaternion y = Quaternion.CreateFromYawPitchRoll(0, this.desiredRot.Z, this.desiredRot.Y);
		this.Rotation = Quaternion.Multiply(x, this.Rotation);
		this.Rotation = Quaternion.Multiply(this.Rotation, y);
		this.desiredRot = Vector3.Zero;
	}

	public override void Calculate(ref CameraState state, Camera? blend = null, float blendWeight = 0)
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
}