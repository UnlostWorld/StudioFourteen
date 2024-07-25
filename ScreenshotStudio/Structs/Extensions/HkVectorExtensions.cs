namespace ScreenshotStudio.Structs;

using System.Numerics;
using FFXIVClientStructs.Havok.Common.Base.Math.Vector;

public static class HkVectorExtensions
{
	public static hkVector4f Zero = new hkVector4f()
	{
		W = 0,
		X = 0,
		Y = 0,
		Z = 0,
	};

	public static hkVector4f One = new hkVector4f()
	{
		W = 1,
		X = 1,
		Y = 1,
		Z = 1,
	};

	public static Vector3 ToVector3(ref this hkVector4f vec) => new Vector3(vec.X, vec.Y, vec.Z);
	public static Vector4 ToVector4(ref this hkVector4f vec) => new Vector4(vec.X, vec.Y, vec.Z, vec.W);

	public static hkVector4f ToHavok(ref this Vector3 v) => new hkVector4f { X = v.X, Y = v.Y, Z = v.Z, W = 1 };

	public static hkVector4f SetFromVector3(ref this hkVector4f tar, Vector3 vec)
	{
		tar.X = vec.X;
		tar.Y = vec.Y;
		tar.Z = vec.Z;
		return tar;
	}
}
