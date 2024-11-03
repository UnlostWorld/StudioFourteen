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

	public override void OnFrameworkUpdate(IFramework framework)
	{
		base.OnFrameworkUpdate(framework);

		Vector3 moveDir = Vector3.Zero;

		if (this.Services.Input.IsDown(KeyBindEvents.FreeCamera_MoveForwards))
			moveDir.X += 1;

		if (this.Services.Input.IsDown(KeyBindEvents.FreeCamera_MoveBack))
			moveDir.X -= 1;

		if (this.Services.Input.IsDown(KeyBindEvents.FreeCamera_MoveLeft))
			moveDir.Z -= 1;

		if (this.Services.Input.IsDown(KeyBindEvents.FreeCamera_MoveRight))
			moveDir.Z += 1;

		if (this.Services.Input.IsDown(KeyBindEvents.FreeCamera_MoveUp))
			moveDir.Y += 1;

		if (this.Services.Input.IsDown(KeyBindEvents.FreeCamera_MoveDown))
			moveDir.Y -= 1;

		this.desiredMove = moveDir;

		Vector3 rot = Vector3.Zero;

		if (this.Services.Input.IsDown(KeyBindEvents.FreeCamera_YawLeft))
			rot.X += 1;

		if (this.Services.Input.IsDown(KeyBindEvents.FreeCamera_YawRight))
			rot.X -= 1;

		if (this.Services.Input.IsDown(KeyBindEvents.FreeCamera_PitchUp))
			rot.Y += 1;

		if (this.Services.Input.IsDown(KeyBindEvents.FreeCamera_PitchDown))
			rot.Y -= 1;

		if (this.Services.Input.IsDown(KeyBindEvents.FreeCamera_RollLeft))
			rot.Z -= 1;

		if (this.Services.Input.IsDown(KeyBindEvents.FreeCamera_RollRight))
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

		Quaternion x = Quaternion.CreateFromYawPitchRoll(delta.X * QuaternionExtensions.Deg2Rad, 0, 0);
		Quaternion y = Quaternion.CreateFromYawPitchRoll(0, 0, delta.Y * QuaternionExtensions.Deg2Rad);
		this.Rotation = Quaternion.Multiply(x, this.Rotation);
		this.Rotation = Quaternion.Multiply(this.Rotation, y);
	}
}