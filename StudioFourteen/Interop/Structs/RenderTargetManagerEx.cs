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

namespace FFXIVClientStructs.FFXIV.Client.Graphics.Render;

using FFXIVClientStructs.FFXIV.Client.Graphics.Kernel;
using global::System.Runtime.InteropServices;

[StructLayout(LayoutKind.Explicit)]
public unsafe partial struct RenderTargetManagerEx
{
	[FieldOffset(0x4D8)] internal Texture* BeforeUIBuffer;
	[FieldOffset(0x570)] internal Texture* BackBuffer;
	[FieldOffset(0x578)] internal Texture* DepthStencil;
	[FieldOffset(0x5A8)] internal Texture* ScreenSpaceReflectionsBuffer;
	[FieldOffset(0x648)] internal Texture* MotionVectors;

	public static RenderTargetManagerEx* Instance()
	{
		return (RenderTargetManagerEx*)RenderTargetManager.Instance();
	}
}
