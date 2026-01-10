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

using System.Drawing;
using System.Numerics;
using FFXIVClientStructs.Havok.Common.Base.Math.Vector;

public static class Vector3Extensions
{
	public static bool IsApproximately(this Vector3 a, Vector3 b, float delta = float.Epsilon)
	{
		return a.X.IsApproximately(b.X, delta)
			&& a.Y.IsApproximately(b.Y, delta)
			&& a.Z.IsApproximately(b.Z, delta);
	}

	public static Vector2 ToVector2(this Vector3 self)
	{
		return new Vector2(self.X, self.Y);
	}

	public static hkVector4f ToHkVector(this Vector3 self)
	{
		hkVector4f val = default;
		val.X = self.X;
		val.Y = self.Y;
		val.Z = self.Z;
		return val;
	}

	public static Point ToDrawingPoint(this Vector3 self)
	{
		return new Point((int)self.X, (int)self.Y);
	}
}
