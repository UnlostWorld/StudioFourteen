namespace StudioFourteen.Cameras;

using Dalamud.Plugin.Services;
using PropertyChanged.SourceGenerator;
using StudioFourteen.Input;
using StudioFourteen.Structs.Extensions;
using System;
using System.Numerics;

public partial class FreeCamera : StudioCameraBase
{
	private const float MoveSpeedMultiplier = 1.01f;
	private const float MoveSpeed = 2.0f;
	private const float MoveSpeedMaximum = 200.0f;

	[Notify] private Vector3 position;
	[Notify] private Quaternion rotation;

	private float moveSpeed = 2.0f;

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

	public override void Update(float deltaTime)
	{
		base.Update(deltaTime);

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

		if (moveDir.X != 0 || moveDir.Y != 0 || moveDir.Z != 0)
		{
			this.moveSpeed *= MoveSpeedMultiplier;
			this.moveSpeed = MathF.Min(MoveSpeedMaximum, this.moveSpeed);
		}
		else
		{
			this.moveSpeed = MoveSpeed;
		}

		moveDir *= deltaTime;
		moveDir *= this.moveSpeed;

		moveDir = Vector3.Transform(moveDir, this.Rotation);
		this.position += moveDir;
	}

	protected override void Drag(Vector2 delta)
	{
		base.Drag(delta);

		Quaternion x = Quaternion.CreateFromYawPitchRoll(delta.X * QuaternionExtensions.Deg2Rad, 0, 0);
		Quaternion y = Quaternion.CreateFromYawPitchRoll(0, 0, delta.Y * QuaternionExtensions.Deg2Rad);
		this.Rotation = Quaternion.Multiply(x, this.Rotation);
		this.Rotation = Quaternion.Multiply(this.Rotation, y);
	}
}