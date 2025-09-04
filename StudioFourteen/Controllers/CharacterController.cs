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

namespace StudioFourteen.Controllers;

using System;
using System.Numerics;
using StudioFourteen.Input;
using StudioFourteen.Scene.GameObjects.Characters;
using StudioFourteen.Services;
using StudioFourteen.Structs.Extensions;

public class CharacterController(Character character)
	: SceneObjectControllerBase<Character>(character)
{
	public float MoveSpeed = 4;
	public float TurnSpeed = 8;

	private readonly Input2DListener moveListener = new(
		InputAction.CharacterController_Right,
		InputAction.CharacterController_Left,
		InputAction.CharacterController_Forwards,
		InputAction.CharacterController_Backwards);

	private Quaternion currentRotation = Quaternion.Identity;
	private float desiredAngle = 0;

	public override void Activate()
	{
		base.Activate();
		this.moveListener.Enable();
		this.Services.Tick.Add(TickService.Channels.GameTick, this.OnGameTick);
	}

	public override void Deactivate()
	{
		base.Deactivate();
		this.Services.Tick.Remove(TickService.Channels.GameTick, this.OnGameTick);
		this.moveListener.Disable();
	}

	private void OnGameTick()
	{
		if (this.Object == null)
			return;

		Vector3 camForward = this.Services.Camera.CurrentForward;
		camForward.Y = 0;

		Vector3 left = Vector3.Transform(
			camForward,
			Quaternion.CreateFromAxisAngle(Vector3.UnitY, QuaternionExtensions.Deg2Rad * -90));

		Vector3 translation = Vector3.Zero;
		translation += camForward * this.moveListener.Y.Value;
		translation += left * this.moveListener.X.Value;

		if (translation.Length() > 0)
		{
			translation = Vector3.Normalize(translation);
			translation *= TickService.DeltaTime;
			translation *= this.MoveSpeed;

			Vector3 look = Vector3.Normalize(translation);
			this.desiredAngle = MathF.Atan2(look.X, look.Z);
		}

		Quaternion rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, this.desiredAngle);

		this.currentRotation = Quaternion.Lerp(
			this.currentRotation,
			rotation,
			TickService.DeltaTime * this.TurnSpeed);

		this.Object.WorldTransform = Transform.FromTRS(
			this.Object.WorldTransform.Translation + translation,
			this.currentRotation,
			Vector3.One);
	}
}