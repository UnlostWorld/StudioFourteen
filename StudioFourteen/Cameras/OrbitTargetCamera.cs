namespace StudioFourteen.Cameras;

using PropertyChanged.SourceGenerator;
using System;
using System.Numerics;
using WpfUtils.Animation;

public partial class OrbitTargetCamera : OrbitCamera
{
	public const float TargetBlendDuration = 0.250f;

	private readonly EasingFunctionBase targetEase = new SineEase();
	private int currentTargetIndex = -1;
	private Vector3 oldTargetPosition;
	private Vector3 currentTargetPosition;
	private float targetBlend = -1;

	[Notify] private Vector3 targetOffset = new(0, 0, 0);

	public override string TypeName => Resources.Find("LOC_OrbitTargetCamera", "Orbit Target");

	public unsafe override void Initialize(CameraState currentState)
	{
		base.Initialize(currentState);

		if (this.Services.Target.HasValidTarget)
		{
			Vector3 offset = this.TargetOffset;
			offset.Y = this.Services.Target.Target->Height;
			this.TargetOffset = offset;
		}
	}

	public unsafe override void Tick(float deltaTime)
	{
		this.desiredMove *= deltaTime;

		Vector3 targetOffset = this.TargetOffset;
		targetOffset.Y += this.desiredMove.Y;

		Vector3 xMove = new(this.desiredMove.Z, 0, this.desiredMove.X);
		xMove = Vector3.Transform(xMove, this.GetCameraRotation());
		targetOffset += xMove;

		this.TargetOffset = targetOffset;
		this.desiredMove = Vector3.Zero;

		if (this.currentTargetIndex != this.Services.Target.TargetObjectIndex && this.currentTargetIndex != -1)
		{
			this.oldTargetPosition = this.currentTargetPosition;
			this.targetBlend = TargetBlendDuration;
		}

		if (this.Services.Target.HasValidTarget)
		{
			this.currentTargetPosition = this.Services.Target.Target->Position;
			this.currentTargetIndex = this.Services.Target.TargetObjectIndex;

			Vector3 targetPos = this.currentTargetPosition;

			if (this.targetBlend > 0)
			{
				this.targetBlend -= deltaTime;
				float blendValue = this.targetBlend / TargetBlendDuration;
				blendValue = Math.Clamp(blendValue, 0.0f, 1.0f);
				blendValue = this.targetEase.Ease(blendValue, EasingFunctionBase.EasingModes.EaseInOut);
				targetPos = Vector3.Lerp(targetPos, this.oldTargetPosition, blendValue);
			}

			targetPos += this.TargetOffset;
			this.Target = targetPos;
		}

		base.Tick(deltaTime);
	}
}
