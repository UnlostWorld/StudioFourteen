// System.Numerics
// https://github.com/microsoft/referencesource/tree/master/System.Numerics/System/Numerics/Quaternion.cs

namespace ScreenshotStudio.Structs.Extensions;

using FFXIVClientStructs.Havok.Common.Base.Math.Vector;
using FFXIVClientStructs.Havok.Common.Base.Math.Quaternion;
using System;
using FFXIVClientStructs.FFXIV.Common.Lua;

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

	public static void Normalize(this ref hkQuaternionf self)
	{
		float ls = (self.X * self.X) + (self.Y * self.Y) + (self.Z * self.Z) + (self.W * self.W);

		float invNorm = 1.0f / MathF.Sqrt(ls);

		self.X = self.X * invNorm;
		self.Y = self.Y * invNorm;
		self.Z = self.Z * invNorm;
		self.W = self.W * invNorm;
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

	public static hkQuaternionf Inverse(this hkQuaternionf value)
	{
		hkQuaternionf ans = value.Conjugate();

		float num = (value.X * value.X) + (value.Y * value.Y) + (value.Z * value.Z) + (value.W * value.W);
		ans.X /= num;
		ans.Y /= num;
		ans.Z /= num;
		ans.W /= num;
		return ans;
	}

	public static hkQuaternionf Conjugate(this hkQuaternionf value)
	{
		hkQuaternionf ans;

		ans.X = -value.X;
		ans.Y = -value.Y;
		ans.Z = -value.Z;
		ans.W = value.W;

		return ans;
	}

	public static void Divide(ref this hkQuaternionf self, hkQuaternionf other)
	{
		self = self.Conjugate();
		self.Multiply(other);
		self = self.Conjugate();
	}

	public static void Set(ref this hkQuaternionf self, hkQuaternionf other)
	{
		self.X = other.X;
		self.Y = other.Y;
		self.Z = other.Z;
		self.W = other.W;
	}

	public static hkQuaternionf FromEuler(hkVector4f euler)
	{
		hkQuaternionf q = default;
		q.FromEuler(euler);
		return q;
	}

	public static void FromEuler(ref this hkQuaternionf self, hkVector4f euler)
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

	public static hkVector4f ToEuler(this hkQuaternionf self)
	{
		float yaw = MathF.Atan2(2.0f * ((self.Y * self.W) + (self.X * self.Z)), 1.0f - (2.0f * ((self.X * self.X) + (self.Y * self.Y))));
		float pitch = MathF.Asin(2.0f * ((self.X * self.W) - (self.Y * self.Z)));
		float roll = MathF.Atan2(2.0f * ((self.X * self.Y) + (self.Z * self.W)), 1.0f - (2.0f * ((self.X * self.X) + (self.Z * self.Z))));

		hkVector4f res = default;
		res.X = yaw * Rad2Deg;
		res.Y = pitch * Rad2Deg;
		res.Z = roll * Rad2Deg;
		return res;
	}
}
