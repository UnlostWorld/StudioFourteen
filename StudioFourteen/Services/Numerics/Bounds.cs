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

public struct Bounds
{
	public static Bounds Zero = default;

	public Vector3 Min = Vector3.Zero;
	public Vector3 Max = Vector3.Zero;

	public Bounds()
	{
	}

	public Vector3 Center
	{
		get
		{
			return (this.Min + this.Max) / 2;
		}
	}

	public Vector3 Extents
	{
		get
		{
			return (this.Max - this.Min) / 2;
		}
	}

	public static Bounds FromExtents(Vector3 center, Vector3 extents)
	{
		Bounds b = default;
		b.Min = center;
		b.Max = center;
		b.Encompass(center + extents);
		b.Encompass(center - extents);
		return b;
	}

	public void Encompass(Vector3 pos)
	{
		this.Min.X = MathF.Min(pos.X, this.Min.X);
		this.Min.Y = MathF.Min(pos.Y, this.Min.Y);
		this.Min.Z = MathF.Min(pos.Z, this.Min.Z);

		this.Max.X = MathF.Max(pos.X, this.Max.X);
		this.Max.Y = MathF.Max(pos.Y, this.Max.Y);
		this.Max.Z = MathF.Max(pos.Z, this.Max.Z);
	}
}