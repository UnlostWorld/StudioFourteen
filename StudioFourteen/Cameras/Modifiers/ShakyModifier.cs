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

namespace StudioFourteen.Cameras.Modifiers;

using System;
using System.Numerics;
using StudioFourteen.Structs.Extensions;

public class ShakyModifier : CameraModifierBase
{
	private float v = 0;
	private Vector3 pos = Vector3.Zero;
	private Quaternion rot = Quaternion.Identity;

	public float PositionIntensity { get; set; } = 0.01f;
	public float RotationIntensity { get; set; } = 0.5f;
	public float Speed { get; set; } = 1.0f;

	public override string TypeDisplayName => Resources.Find("LOC_ShakyModifierCamera", "Shaky");

	public override void Tick(float deltaTime)
	{
		base.Tick(deltaTime);

		this.v += deltaTime * this.Speed;

		this.pos.X = MathF.Sin(this.v) * this.PositionIntensity;
		this.pos.Y = MathF.Cos(0.33f * this.v) * this.PositionIntensity;
		this.pos.Z = MathF.Cos(0.66f * this.v) * this.PositionIntensity;

		Vector3 rotEuler = Vector3.Zero;
		rotEuler.X = MathF.Sin(0.5467f + this.v) * this.RotationIntensity;
		rotEuler.Y = MathF.Cos(0.278f + (0.33f * this.v)) * this.RotationIntensity;
		rotEuler.Z = MathF.Cos(0.817f + (0.66f * this.v)) * this.RotationIntensity;
		this.rot.FromEuler(rotEuler);
	}

	public override void Calculate(ref CameraState state, float weight)
	{
		base.Calculate(ref state, weight);

		state.Position += Vector3.Lerp(Vector3.Zero, this.pos, weight);
		state.Rotation *= Quaternion.Lerp(Quaternion.Identity, this.rot, weight);
	}
}