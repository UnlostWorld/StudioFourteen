// System.Numerics
// https://github.com/microsoft/referencesource/tree/master/System.Numerics/System/Numerics/Quaternion.cs

namespace ScreenshotStudio.Structs.Extensions;

using FFXIVClientStructs.Havok.Common.Base.Math.Vector;
using FFXIVClientStructs.Havok.Common.Base.Math.Quaternion;
using System;

public static class HkQuaternionExtensions
{
	public static readonly hkQuaternionf Identity = new hkQuaternionf()
	{
		X = 0,
		Y = 0,
		Z = 0,
		W = 1,
	};

	private static readonly float Deg2Rad = ((float)Math.PI * 2) / 360;
	private static readonly float Rad2Deg = 360 / ((float)Math.PI * 2);

	public static hkVector4f ToVector(ref this hkQuaternionf q)
	{
		hkVector4f v = default;
		v.W = q.W;
		v.X = q.X;
		v.Y = q.Y;
		v.Z = q.Z;
		return v;
	}

	public static hkQuaternionf New(float x, float y, float z, float w)
	{
		hkQuaternionf v = default;
		v.X = x;
		v.Y = y;
		v.Z = z;
		v.W = w;
		return v;
	}

	public static void Multiply(ref this hkQuaternionf self, hkQuaternionf other)
	{
		float q1x = self.X;
		float q1y = self.Y;
		float q1z = self.Z;
		float q1w = self.W;

		float q2x = other.X;
		float q2y = other.Y;
		float q2z = other.Z;
		float q2w = other.W;

		// cross(av, bv)
		float cx = (q1y * q2z) - (q1z * q2y);
		float cy = (q1z * q2x) - (q1x * q2z);
		float cz = (q1x * q2y) - (q1y * q2x);

		float dot = (q1x * q2x) + (q1y * q2y) + (q1z * q2z);

		self.X = (q1x * q2w) + (q2x * q1w) + cx;
		self.Y = (q1y * q2w) + (q2y * q1w) + cy;
		self.Z = (q1z * q2w) + (q2z * q1w) + cz;
		self.W = (q1w * q2w) - dot;
	}

	public static hkQuaternionf FromEuler(hkVector4f euler)
	{
		hkQuaternionf q = default;
		q.FromEuler(euler);
		return q;
	}

	public static void FromEuler(ref this hkQuaternionf self, hkVector4f euler)
	{
		float yaw = euler.Y * Deg2Rad;
		float pitch = euler.X * Deg2Rad;
		float roll = euler.Z * Deg2Rad;

		float c1 = MathF.Cos(yaw / 2);
		float s1 = MathF.Sin(yaw / 2);
		float c2 = MathF.Cos(pitch / 2);
		float s2 = MathF.Sin(pitch / 2);
		float c3 = MathF.Cos(roll / 2);
		float s3 = MathF.Sin(roll / 2);

		float c1c2 = c1 * c2;
		float s1s2 = s1 * s2;

		self.X = (c1c2 * s3) + (s1s2 * c3);
		self.Y = (s1 * c2 * c3) + (c1 * s2 * s3);
		self.Z = (c1 * s2 * c3) - (s1 * c2 * s3);
		self.W = (c1c2 * c3) - (s1s2 * s3);
	}

	public static hkVector4f ToEuler(ref this hkQuaternionf self)
	{
		hkVector4f v = default;

		double test = (self.X * self.Y) + (self.Z * self.W);

		if (test > 0.4995f)
		{
			v.Y = 2f * (float)Math.Atan2(self.X, self.Y);
			v.X = (float)Math.PI / 2;
			v.Z = 0;
		}
		else if (test < -0.4995f)
		{
			v.Y = -2f * (float)Math.Atan2(self.X, self.W);
			v.X = -(float)Math.PI / 2;
			v.Z = 0;
		}
		else
		{
			double sqx = self.X * self.X;
			double sqy = self.Y * self.Y;
			double sqz = self.Z * self.Z;

			v.Y = (float)Math.Atan2((2 * self.Y * self.W) - (2 * self.X * self.Z), 1 - (2 * sqy) - (2 * sqz));
			v.X = (float)Math.Asin(2 * test);
			v.Z = (float)Math.Atan2((2 * self.X * self.W) - (2 * self.Y * self.Z), 1 - (2 * sqx) - (2 * sqz));
		}

		v.X = NormalizeAngle(v.X * Rad2Deg);
		v.Y = NormalizeAngle(v.Y * Rad2Deg);
		v.Z = NormalizeAngle(v.Z * Rad2Deg);

		return v;
	}

	private static float NormalizeAngle(float angle)
	{
		while (angle > 360)
			angle -= 360;

		while (angle < 0)
			angle += 360;

		return angle;
	}
}
