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

namespace StudioFourteen.Structs.Extensions;

using FFXIVClientStructs.Havok.Common.Base.Math.Quaternion;
using System;
using System.Numerics;

public static class QuaternionExtensions
{
	public static readonly float Deg2Rad = ((float)Math.PI * 2) / 360;
	public static readonly float Rad2Deg = 360 / ((float)Math.PI * 2);

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

		float halfRoll = (euler.Z * Deg2Rad) * 0.5f;
		sr = (float)Math.Sin(halfRoll);
		cr = (float)Math.Cos(halfRoll);

		float halfPitch = (euler.Y * Deg2Rad) * 0.5f;
		sp = (float)Math.Sin(halfPitch);
		cp = (float)Math.Cos(halfPitch);

		float halfYaw = (euler.X * Deg2Rad) * 0.5f;
		sy = (float)Math.Sin(halfYaw);
		cy = (float)Math.Cos(halfYaw);

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
		res.X = yaw * Rad2Deg;
		res.Y = pitch * Rad2Deg;
		res.Z = roll * Rad2Deg;
		return res;
	}

	public static bool IsApproximately(this Quaternion a, Quaternion b, float delta = float.Epsilon)
	{
		return a.X.IsApproximately(b.X, delta)
			&& a.Y.IsApproximately(b.Y, delta)
			&& a.Z.IsApproximately(b.Z, delta)
			&& a.W.IsApproximately(b.W, delta);
	}

	public static Quaternion Conjugate(this Quaternion value)
	{
		return Quaternion.Conjugate(value);
	}
}
