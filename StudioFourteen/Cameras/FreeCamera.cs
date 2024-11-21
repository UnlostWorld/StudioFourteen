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
using StudioFourteen.Input;
using StudioFourteen.Structs.Extensions;
using System;
using System.Numerics;
using System.Windows.Input;

public partial class FreeCamera : StudioCameraBase
{
	private const float MoveSpeedMultiplier = 1.01f;
	private const float MoveSpeed = 2.0f;
	private const float MoveSpeedMaximum = 200.0f;

	private readonly KeyBindListener moveForwardsListener = new(KeyBindEvents.FreeCamera_MoveForwards);
	private readonly KeyBindListener moveBackListener = new(KeyBindEvents.FreeCamera_MoveBack);
	private readonly KeyBindListener moveUpListener = new(KeyBindEvents.FreeCamera_MoveUp);
	private readonly KeyBindListener moveDownListener = new(KeyBindEvents.FreeCamera_MoveDown);
	private readonly KeyBindListener moveLeftListener = new(KeyBindEvents.FreeCamera_MoveLeft);
	private readonly KeyBindListener moveRightListener = new(KeyBindEvents.FreeCamera_MoveRight);
	private readonly KeyBindListener yawLeftListener = new(KeyBindEvents.FreeCamera_YawLeft);
	private readonly KeyBindListener yawRightListener = new(KeyBindEvents.FreeCamera_YawRight);
	private readonly KeyBindListener pitchUpListener = new(KeyBindEvents.FreeCamera_PitchUp);
	private readonly KeyBindListener pitchDownListener = new(KeyBindEvents.FreeCamera_PitchDown);
	private readonly KeyBindListener rollLeftListener = new(KeyBindEvents.FreeCamera_RollLeft);
	private readonly KeyBindListener rollRightListener = new(KeyBindEvents.FreeCamera_RollRight);

	[Notify] private Vector3 position;
	[Notify] private Quaternion rotation;

	private Vector3 desiredMove = Vector3.Zero;
	private Vector3 desiredRot = Vector3.Zero;
	private float moveSpeed = 2.0f;

	public override string TypeName => Resources.Find("LOC_FreeCamera", "Free Target");

	public override void Initialize(CameraState currentState)
	{
		base.Initialize(currentState);

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
	}

	public override unsafe void UpdateGroupPoseCamera(GroupPoseCamera* camera)
	{
		base.UpdateGroupPoseCamera(camera);

		camera->Camera.Distance = float.MaxValue;
	}

	public override void OnFrameworkUpdate(IFramework framework)
	{
		base.OnFrameworkUpdate(framework);

		Vector3 moveDir = Vector3.Zero;

		if (this.moveForwardsListener.IsDown())
			moveDir.X += 1;

		if (this.moveBackListener.IsDown())
			moveDir.X -= 1;

		if (this.moveLeftListener.IsDown())
			moveDir.Z -= 1;

		if (this.moveRightListener.IsDown())
			moveDir.Z += 1;

		if (this.moveUpListener.IsDown())
			moveDir.Y += 1;

		if (this.moveDownListener.IsDown())
			moveDir.Y -= 1;

		this.desiredMove = moveDir;

		Vector3 rot = Vector3.Zero;

		if (this.yawLeftListener.IsDown())
			rot.X += 1;

		if (this.yawRightListener.IsDown())
			rot.X -= 1;

		if (this.pitchUpListener.IsDown())
			rot.Y += 1;

		if (this.pitchDownListener.IsDown())
			rot.Y -= 1;

		if (this.rollLeftListener.IsDown())
			rot.Z -= 1;

		if (this.rollRightListener.IsDown())
			rot.Z += 1;

		this.desiredRot = rot;
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

	protected override void OnMouseDrag(Vector2 delta, MouseButton button)
	{
		base.OnMouseDrag(delta, button);

		delta /= 8;

		Quaternion x = Quaternion.CreateFromYawPitchRoll(-delta.X * QuaternionExtensions.Deg2Rad, 0, 0);
		Quaternion y = Quaternion.CreateFromYawPitchRoll(0, 0, -delta.Y * QuaternionExtensions.Deg2Rad);
		this.Rotation = Quaternion.Multiply(x, this.Rotation);
		this.Rotation = Quaternion.Multiply(this.Rotation, y);
	}
}