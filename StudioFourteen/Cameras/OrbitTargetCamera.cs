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

using PropertyChanged.SourceGenerator;
using StudioFourteen.Scene;
using System.Numerics;

public partial class OrbitTargetCamera : OrbitCamera
{
	private Vector3 currentTargetPosition;

	[Notify] private Vector3 targetOffset = new(0, 0, 0);
	[Notify] private float lerpSpeed = 2;

	public override string TypeDisplayName => Resources.Find("LOC_OrbitTargetCamera", "Orbit Target");

	public override void Initialize(CameraState currentState, StudioCameraBase? previousCamera)
	{
		base.Initialize(currentState, previousCamera);

		this.TargetOffset = new(0, 1.0f, 0);
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

		if (this.Services.Selection.Current is TransformSceneObjectBase transformObject)
		{
			this.currentTargetPosition = Vector3.Transform(Vector3.Zero, transformObject.WorldTransform.ToMatrix());
		}

		this.Target = Vector3.Lerp(this.Target, this.currentTargetPosition + this.TargetOffset, deltaTime * this.lerpSpeed);

		base.Tick(deltaTime);
	}
}
