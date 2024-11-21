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

namespace StudioFourteen.Structs;

using FFXIVClientStructs.Havok.Common.Base.Math.Vector;
using System.Numerics;

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

	public static void Subtract(ref this hkVector4f self, Vector4 other)
	{
		self.X -= other.X;
		self.Y -= other.Y;
		self.Z -= other.Z;
		self.W -= other.W;
	}

	public static void Subtract(ref this hkVector4f self, Vector3 other)
	{
		self.X -= other.X;
		self.Y -= other.Y;
		self.Z -= other.Z;
	}

	public static void Set(ref this hkVector4f self, hkVector4f other)
	{
		self.X = other.X;
		self.Y = other.Y;
		self.Z = other.Z;
		self.W = other.W;
	}

	public static void Set(ref this hkVector4f self, Vector3 other)
	{
		self.X = other.X;
		self.Y = other.Y;
		self.Z = other.Z;
		self.W = 0;
	}

	public static Vector3 ToVector3(this hkVector4f self)
	{
		return new Vector3(self.X, self.Y, self.Z);
	}

	public static void FromVector3(ref this hkVector4f self, Vector3 vec)
	{
		self.X = vec.X;
		self.Y = vec.Y;
		self.Z = vec.Z;
		self.W = 0;
	}

	public static hkVector4f ToHkVector(this Vector3 self)
	{
		hkVector4f val = default;
		val.X = self.X;
		val.Y = self.Y;
		val.Z = self.Z;
		return val;
	}
}
