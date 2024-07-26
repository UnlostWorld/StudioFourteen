namespace ScreenshotStudio.Structs;

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

	public static void Add(ref this hkVector4f self, hkVector4f other)
	{
		self.X += other.X;
		self.Y += other.Y;
		self.Z += other.Z;
		self.W += other.W;
	}

	public static void Subtract(ref this hkVector4f self, hkVector4f other)
	{
		self.X -= other.X;
		self.Y -= other.Y;
		self.Z -= other.Z;
		self.W -= other.W;
	}

	public static void Set(ref this hkVector4f self, hkVector4f other)
	{
		self.X = other.X;
		self.Y = other.Y;
		self.Z = other.Z;
		self.W = other.W;
	}
}
