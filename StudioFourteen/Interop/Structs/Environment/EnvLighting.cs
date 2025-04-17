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
// https://github.com/ktisis-tools/Ktisis/blob/v0.3/main/Ktisis/Structs/Env/Weather/EnvLighting.cs
[StructLayout(LayoutKind.Explicit, Size = 0x40)]
public struct EnvLighting
{
	[FieldOffset(0x00)] public Vector3 SunLightColor;
	[FieldOffset(0x0C)] public Vector3 MoonLightColor;
	[FieldOffset(0x18)] public Vector3 Ambient;
	[FieldOffset(0x24)] public float Unknown1;
	[FieldOffset(0x28)] public float AmbientSaturation;
	[FieldOffset(0x2C)] public float Temperature;
	[FieldOffset(0x30)] public float Unknown2;
	[FieldOffset(0x34)] public float Unknown3;
	[FieldOffset(0x38)] public float Unknown4;
}