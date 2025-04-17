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

using FFXIVClientStructs.FFXIV.Client.Graphics.Environment;
using System.Runtime.InteropServices;

// Modified from Ktisis Source - 2025-04-17
// Ktisis is licensed under the GNU AFFERO GENERAL PUBLIC LICENSE v3
// https://github.com/ktisis-tools/Ktisis/blob/v0.3/main/Ktisis/Structs/Env/EnvManagerEx.cs
[StructLayout(LayoutKind.Explicit, Size = 0x900)]
public struct EnvManagerEx
{
	[FieldOffset(39)] public byte ActiveWeather;
	[FieldOffset(40)] public float TransitionTime;
	[FieldOffset(0x058)] public EnvState State;

	public static unsafe EnvManagerEx* Instance()
	{
		return (EnvManagerEx*)EnvManager.Instance();
	}
}