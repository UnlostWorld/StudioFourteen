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

namespace StudioFourteen.Services.Numerics;

using System;
using System.Numerics;

public static class QuaternionExtensions
{
	public static Quaternion FromEuler(Vector3 euler)
	{
		Quaternion q = default;
		q.FromEuler(euler);
		return q;
	}

	public static void FromEuler(ref this Quaternion self, Vector3 euler)
	{
		// Roll first, about axis the object is facing, then
		// pitch upward, then yaw to face into the new heading
		float sr, cr, sp, cp, sy, cy;

		float halfRoll = (euler.Z * Math.Deg2Rad) * 0.5f;
		sr = Math.Sin(halfRoll);
		cr = Math.Cos(halfRoll);

		float halfPitch = (euler.Y * Math.Deg2Rad) * 0.5f;
		sp = Math.Sin(halfPitch);
		cp = Math.Cos(halfPitch);

		float halfYaw = (euler.X * Math.Deg2Rad) * 0.5f;
		sy = Math.Sin(halfYaw);
		cy = Math.Cos(halfYaw);

		self.X = (cy * sp * cr) + (sy * cp * sr);
		self.Y = (sy * cp * cr) - (cy * sp * sr);
		self.Z = (cy * cp * sr) - (sy * sp * cr);
		self.W = (cy * cp * cr) + (sy * sp * sr);
	}

	public static Vector3 ToEuler(this Quaternion self)
	{
		float yaw = MathF.Atan2(2.0f * ((self.Y * self.W) + (self.X * self.Z)), 1.0f - (2.0f * ((self.X * self.X) + (self.Y * self.Y))));
		float pitch = MathF.Asin(2.0f * ((self.X * self.W) - (self.Y * self.Z)));
		float roll = MathF.Atan2(2.0f * ((self.X * self.Y) + (self.Z * self.W)), 1.0f - (2.0f * ((self.X * self.X) + (self.Z * self.Z))));

		Vector3 res = default;
		res.X = yaw * Math.Rad2Deg;
		res.Y = pitch * Math.Rad2Deg;
		res.Z = roll * Math.Rad2Deg;
		return res;
	}

	public static bool IsApproximately(this Quaternion a, Quaternion b, float delta = float.Epsilon)
	{
		Vector3 vA = Vector3.Transform(Vector3.UnitX, a);
		Vector3 vB = Vector3.Transform(Vector3.UnitX, b);

		if (!vA.IsApproximately(vB, delta))
			return false;

		vA = Vector3.Transform(Vector3.UnitY, a);
		vB = Vector3.Transform(Vector3.UnitY, b);

		if (!vA.IsApproximately(vB, delta))
			return false;

		return true;
	}

	public static Quaternion Conjugate(this Quaternion value)
	{
		return Quaternion.Conjugate(value);
	}
}
