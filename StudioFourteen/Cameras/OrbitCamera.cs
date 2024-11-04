namespace StudioFourteen.Cameras;

using Dalamud.Plugin.Services;
using PropertyChanged.SourceGenerator;
using StudioFourteen.Input;
using StudioFourteen.Overlays;
using StudioFourteen.Structs.Extensions;
using System;
using System.Numerics;
using System.Windows.Input;

public partial class OrbitCamera : StudioCameraBase
{
	protected Vector3 desiredRot = Vector3.Zero;
	protected Vector3 desiredMove = Vector3.Zero;

	private readonly PointOverlay targetPointOverlay = new("Cameras", "OrbitCameraTarget");

	private readonly KeyBindListener moveUpListener = new(KeyBindEvents.OrbitCamera_MoveUp);
	private readonly KeyBindListener moveDownListener = new(KeyBindEvents.OrbitCamera_MoveDown);
	private readonly KeyBindListener moveLeftListener = new(KeyBindEvents.OrbitCamera_MoveLeft);
	private readonly KeyBindListener moveRightListener = new(KeyBindEvents.OrbitCamera_MoveRight);
	private readonly KeyBindListener panUpListener = new(KeyBindEvents.OrbitCamera_PanUp);
	private readonly KeyBindListener panDownListener = new(KeyBindEvents.OrbitCamera_PanDown);
	private readonly KeyBindListener panLeftListener = new(KeyBindEvents.OrbitCamera_PanLeft);
	private readonly KeyBindListener panRightListener = new(KeyBindEvents.OrbitCamera_PanRight);
	private readonly KeyBindListener rollLeftListener = new(KeyBindEvents.OrbitCamera_RollLeft);
	private readonly KeyBindListener rollRightListener = new(KeyBindEvents.OrbitCamera_RollRight);

	[Notify] private Vector3 target;
	[Notify] private float distance;
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
		this.Distance = 3;

		// TODO: a better initial angle
		this.Angle = Vector2.Zero;
	}

	public override void Activate()
	{
		base.Activate();

		this.moveUpListener.Enable();
		this.moveDownListener.Enable();
		this.moveLeftListener.Enable();
		this.moveRightListener.Enable();
		this.panUpListener.Enable();
		this.panDownListener.Enable();
		this.panLeftListener.Enable();
		this.panRightListener.Enable();
		this.rollLeftListener.Enable();
		this.rollRightListener.Enable();

		this.targetPointOverlay.Enable();
	}

	public override void Deactivate()
	{
		base.Deactivate();

		this.moveUpListener.Disable();
		this.moveDownListener.Disable();
		this.moveLeftListener.Disable();
		this.moveRightListener.Disable();
		this.panUpListener.Disable();
		this.panDownListener.Disable();
		this.panLeftListener.Disable();
		this.panRightListener.Disable();
		this.rollLeftListener.Disable();
		this.rollRightListener.Disable();

		this.targetPointOverlay.Disable();
	}

	public override void OnFrameworkUpdate(IFramework framework)
	{
		base.OnFrameworkUpdate(framework);

		Vector3 moveDir = Vector3.Zero;

		if (this.moveUpListener.IsDown())
			moveDir.Y += 1;

		if (this.moveDownListener.IsDown())
			moveDir.Y -= 1;

		if (this.moveLeftListener.IsDown())
			moveDir.X -= 1;

		if (this.moveRightListener.IsDown())
			moveDir.X += 1;

		this.desiredMove = moveDir;

		Vector3 rot = Vector3.Zero;

		if (this.panLeftListener.IsDown())
			rot.X += 1;

		if (this.panRightListener.IsDown())
			rot.X -= 1;

		if (this.panUpListener.IsDown())
			rot.Y += 1;

		if (this.panDownListener.IsDown())
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

		this.desiredMove *= deltaTime;
		this.Target += this.desiredMove;
		this.desiredMove = Vector3.Zero;

		this.desiredRot *= deltaTime;
		Quaternion x = Quaternion.CreateFromYawPitchRoll(this.desiredRot.X, 0, 0);
		Quaternion y = Quaternion.CreateFromYawPitchRoll(0, this.desiredRot.Z, this.desiredRot.Y);
		this.Rotation = Quaternion.Multiply(x, this.Rotation);
		this.Rotation = Quaternion.Multiply(this.Rotation, y);
		this.desiredRot = Vector3.Zero;

		this.targetPointOverlay.WorldPosition = this.Target;
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

	protected override void OnMouseDrag(Vector2 delta, MouseButton button)
	{
		base.OnMouseDrag(delta, button);

		Vector2 angle = this.Angle;
		angle.X -= delta.X / 8;
		angle.Y -= delta.Y / 8;
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
