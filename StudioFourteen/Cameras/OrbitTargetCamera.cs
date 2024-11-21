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

using FFXIVClientStructs.FFXIV.Client.Game.Character;
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
			Character* pTarget = this.Services.Target.GetTarget();
			Vector3 targetPosition = pTarget->DrawObject->Position;
			targetPosition.Y = targetPosition.Y + (pTarget->Height * 1.5f);

			this.currentTargetPosition = targetPosition;
			this.currentTargetIndex = this.Services.Target.TargetObjectIndex;

			if (this.targetBlend > 0)
			{
				this.targetBlend -= deltaTime;
				float blendValue = this.targetBlend / TargetBlendDuration;
				blendValue = Math.Clamp(blendValue, 0.0f, 1.0f);
				blendValue = this.targetEase.Ease(blendValue, EasingFunctionBase.EasingModes.EaseInOut);
				targetPosition = Vector3.Lerp(targetPosition, this.oldTargetPosition, blendValue);
			}

			targetPosition += this.TargetOffset;
			this.Target = targetPosition;
		}

		base.Tick(deltaTime);
	}
}
