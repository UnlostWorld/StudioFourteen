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

namespace StudioFourteen.Structs.Extensions;

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
