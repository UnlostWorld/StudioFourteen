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

namespace StudioFourteen.Interop.Structs.Environment;

using System.Runtime.InteropServices;
using System.Numerics;

// Modified from Ktisis Source - 2025-04-17
// Ktisis is licensed under the GNU AFFERO GENERAL PUBLIC LICENSE v3
// https://github.com/ktisis-tools/Ktisis/blob/v0.3/main/Ktisis/Structs/Env/Weather/EnvRain.cs
[StructLayout(LayoutKind.Explicit, Size = 0x34)]
public struct EnvRain
{
	[FieldOffset(0x00)] public float Raindrops;
	[FieldOffset(0x04)] public float Intensity;
	[FieldOffset(0x08)] public float Weight;
	[FieldOffset(0x0C)] public float Scatter;
	[FieldOffset(0x10)] public float Unknown1;
	[FieldOffset(0x14)] public float Size;
	[FieldOffset(0x18)] public Vector4 Color;
	[FieldOffset(0x28)] public float Unknown2;
	[FieldOffset(0x2C)] public float Unknown3;
	[FieldOffset(0x30)] public uint Unknown4;
}