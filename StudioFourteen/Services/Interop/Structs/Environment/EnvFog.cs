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

namespace StudioFourteen.Services.Interop.Structs.Environment;

using System.Runtime.InteropServices;
using System.Numerics;

// Modified from Ktisis Source - 2025-04-17
// Ktisis is licensed under the GNU AFFERO GENERAL PUBLIC LICENSE v3
// https://github.com/ktisis-tools/Ktisis/blob/v0.3/main/Ktisis/Structs/Env/Weather/EnvFog.cs
[StructLayout(LayoutKind.Explicit, Size = 0x28)]
public struct EnvFog
{
	[FieldOffset(0x00)] public Vector4 Color;
	[FieldOffset(0x10)] public float Distance;
	[FieldOffset(0x14)] public float Thickness;
	[FieldOffset(0x18)] public float Unknown1;
	[FieldOffset(0x1C)] public float Unknown2;
	[FieldOffset(0x20)] public float Opacity;
	[FieldOffset(0x24)] public float SkyVisibility;
}