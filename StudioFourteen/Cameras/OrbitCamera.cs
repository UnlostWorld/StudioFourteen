namespace StudioFourteen.Cameras;

using Dalamud.Plugin.Services;
using PropertyChanged.SourceGenerator;
using StudioFourteen.Input;
using StudioFourteen.Structs.Extensions;
using System;
using System.Numerics;

public partial class OrbitCamera : StudioCameraBase
{
	protected Vector3 desiredRot = Vector3.Zero;
	protected Vector3 desiredMove = Vector3.Zero;

	[Notify] private Vector3 target;
	[Notify] private float distance = 3;
	[Notify] private Vector2 angle;
	[Notify] private Quaternion rotation;

	[Notify(Setter.Private)] private float groupPoseRollAdjust;

	public override string TypeName => Resources.Find("LOC_OrbitCamera", "Orbit");

	public override void Initialize(CameraState currentState)
	{
		base.Initialize(currentState);

		Vector3 targetPos = currentState.Position + Vector3.Transform(new Vector3(this.distance, 0, 0), currentState.Rotation);
		this.target = targetPos;

		this.Rotation = Quaternion.Identity;

		this.Log.Information($"{currentState.Rotation.ToEuler()}");
	}

	public override void OnFrameworkUpdate(IFramework framework)
	{
		base.OnFrameworkUpdate(framework);

		Vector3 moveDir = Vector3.Zero;

		if (this.Services.Input.IsDown(KeyBindEvents.OrbitCamera_MoveUp))
			moveDir.Y += 1;

		if (this.Services.Input.IsDown(KeyBindEvents.OrbitCamera_MoveDown))
			moveDir.Y -= 1;

		if (this.Services.Input.IsDown(KeyBindEvents.OrbitCamera_MoveLeft))
			moveDir.X -= 1;

		if (this.Services.Input.IsDown(KeyBindEvents.OrbitCamera_MoveRight))
			moveDir.X += 1;

		this.desiredMove = moveDir;

		Vector3 rot = Vector3.Zero;

		if (this.Services.Input.IsDown(KeyBindEvents.OrbitCamera_PanLeft))
			rot.X += 1;

		if (this.Services.Input.IsDown(KeyBindEvents.OrbitCamera_PanRight))
			rot.X -= 1;

		if (this.Services.Input.IsDown(KeyBindEvents.OrbitCamera_PanUp))
			rot.Y += 1;

		if (this.Services.Input.IsDown(KeyBindEvents.OrbitCamera_PanDown))
			rot.Y -= 1;

		if (this.Services.Input.IsDown(KeyBindEvents.OrbitCamera_RollLeft))
			rot.Z -= 1;

		if (this.Services.Input.IsDown(KeyBindEvents.OrbitCamera_RollRight))
			rot.Z += 1;

		this.desiredRot = rot;
	}

	public override void Tick(float deltaTime)
	{
		this.desiredMove *= deltaTime;
		this.Target += this.desiredMove;
		this.desiredMove = Vector3.Zero;

		this.desiredRot *= deltaTime;
		Quaternion x = Quaternion.CreateFromYawPitchRoll(this.desiredRot.X, 0, 0);
		Quaternion y = Quaternion.CreateFromYawPitchRoll(0, this.desiredRot.Z, this.desiredRot.Y);
		this.Rotation = Quaternion.Multiply(x, this.Rotation);
		this.Rotation = Quaternion.Multiply(this.Rotation, y);
		this.desiredRot = Vector3.Zero;
	}

	public unsafe override void UpdateGroupPoseCamera(GroupPoseCamera* camera)
	{
		base.UpdateGroupPoseCamera(camera);
		this.GroupPoseRollAdjust = camera->Rotation * QuaternionExtensions.Rad2Deg;
	}

	public override void Calculate(ref CameraState state, StudioCameraBase? blend, float blendWeight)
	{
		base.Calculate(ref state, blend, blendWeight);

		Vector3 targetPos = this.target;
		Quaternion lookRot = this.GetLookRotation();
		Quaternion rotation = this.Rotation;
		float distance = this.distance;

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
		Vector3 position = targetPos + (forward * -this.distance);

		return position;
	}

	public Quaternion GetLookRotation()
	{
		return Quaternion.CreateFromYawPitchRoll(
			this.angle.X * QuaternionExtensions.Deg2Rad,
			this.GroupPoseRollAdjust * QuaternionExtensions.Deg2Rad,
			this.angle.Y * QuaternionExtensions.Deg2Rad);
	}

	public Quaternion GetCameraRotation()
	{
		Quaternion lookAtRot = this.GetLookRotation();
		return Quaternion.Multiply(lookAtRot, this.Rotation);
	}

	protected override void OnMouseDrag(Vector2 delta)
	{
		base.OnMouseDrag(delta);

		this.Log.Information($"DRAG {delta}");

		Vector2 angle = this.Angle;
		angle.X += delta.X;
		angle.Y += delta.Y;
		this.Angle = angle;
	}

	protected override void OnMouseScroll(float delta)
	{
		base.OnMouseScroll(delta);

		float d = this.distance;
		d += delta;
		this.Distance = Math.Max(d, 0);
	}
}
