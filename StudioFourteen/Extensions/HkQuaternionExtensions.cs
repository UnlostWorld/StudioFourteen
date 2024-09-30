// System.Numerics
// https://github.com/microsoft/referencesource/tree/master/System.Numerics/System/Numerics/Quaternion.cs

namespace ScreenshotStudio.Structs.Extensions;

using FFXIVClientStructs.Havok.Common.Base.Math.Vector;
using FFXIVClientStructs.Havok.Common.Base.Math.Quaternion;
using System;
using System.Numerics;

public static class HkQuaternionExtensions
{
	public static readonly hkQuaternionf Identity = new hkQuaternionf()
	{
		X = 0,
		Y = 0,
		Z = 0,
		W = 1,
	};

	public static readonly float Deg2Rad = ((float)Math.PI * 2) / 360;
	public static readonly float Rad2Deg = 360 / ((float)Math.PI * 2);

	public static Quaternion ToQuaternion(this hkQuaternionf self)
	{
		return new(self.X, self.Y, self.Z, self.W);
	}

	public static hkQuaternionf FromQuaternion(Quaternion q)
	{
		hkQuaternionf self = default;
		self.X = q.X;
		self.Y = q.Y;
		self.Z = q.Z;
		self.W = q.W;
		return self;
	}

	public static void FromQuaternion(ref this hkQuaternionf self, Quaternion q)
	{
		self.X = q.X;
		self.Y = q.Y;
		self.Z = q.Z;
		self.W = q.W;
	}

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

	public static void Divide(ref this hkQuaternionf self, Quaternion other)
	{
		Divide(ref self, HkQuaternionExtensions.FromQuaternion(other));
	}

	public static void Set(ref this hkQuaternionf self, hkQuaternionf other)
	{
		self.X = other.X;
		self.Y = other.Y;
		self.Z = other.Z;
		self.W = other.W;
	}

	public static void Set(ref this hkQuaternionf self, Quaternion other)
	{
		self.X = other.X;
		self.Y = other.Y;
		self.Z = other.Z;
		self.W = other.W;
	}

	public static hkQuaternionf ToHkQuaternion(this Quaternion self)
	{
		hkQuaternionf val = default;
		val.X = self.X;
		val.Y = self.Y;
		val.Z = self.Z;
		val.W = self.W;
		return val;
	}
}
