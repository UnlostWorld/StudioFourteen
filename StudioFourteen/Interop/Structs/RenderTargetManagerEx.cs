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
	[FieldOffset(0x0)] internal RenderTargetManager Base;

	[FieldOffset(0x4D8)] internal Texture* BeforeUIBuffer;
	[FieldOffset(0x570)] internal Texture* BackBuffer;
	[FieldOffset(0x578)] internal Texture* DepthStencil;
	[FieldOffset(0x5A8)] internal Texture* ScreenSpaceReflectionsBuffer;
	[FieldOffset(0x648)] internal Texture* MotionVectors;

	/*[FieldOffset(0x70)] internal Texture* Unk70;
	[FieldOffset(0x78)] internal Texture* Unk78; // depends on byte at GraphicsConfig+0x43
	[FieldOffset(0x80)] internal Texture* Unk80;
	[FieldOffset(0x88)] internal Texture* Unk88;
	[FieldOffset(0x90)] internal Texture* Unk90;
	[FieldOffset(0xE8)] internal Texture* UnkE8;
	[FieldOffset(0xF0)] internal Texture* UnkF0; // null?
	[FieldOffset(0xF8)] internal Texture* UnkF8; // null?
	[FieldOffset(0x100)] internal Texture* Unk100;
	[FieldOffset(0x108)] internal Texture* Unk108;
	[FieldOffset(0x110)] internal Texture* Unk110;
	[FieldOffset(0x118)] internal Texture* Unk118;
	[FieldOffset(0x120)] internal Texture* Unk120;
	[FieldOffset(0x128)] internal Texture* Unk128;
	[FieldOffset(0x130)] internal Texture* Unk130;
	[FieldOffset(0x138)] internal Texture* Unk138;
	[FieldOffset(0x140)] internal Texture* Unk140;
	[FieldOffset(0x148)] internal Texture* Unk148;
	[FieldOffset(0x150)] internal Texture* Unk150; // null?
	[FieldOffset(0x158)] internal Texture* Unk158;
	[FieldOffset(0x180)] internal Texture* Unk180;
	[FieldOffset(0x228)] internal Texture* Unk228;
	[FieldOffset(0x250)] internal Texture* Unk250;
	[FieldOffset(0x258)] internal Texture* Unk258;
	[FieldOffset(0x260)] internal Texture* Unk260;
	[FieldOffset(0x268)] internal Texture* Unk268;
	[FieldOffset(0x270)] internal Texture* Unk270;
	[FieldOffset(0x278)] internal Texture* Unk278;
	[FieldOffset(0x280)] internal Texture* Unk280;
	[FieldOffset(0x288)] internal Texture* Unk288;
	[FieldOffset(0x290)] internal Texture* Unk290;
	[FieldOffset(0x298)] internal Texture* Unk298;
	[FieldOffset(0x2C8)] internal Texture* Unk2C8;

	// CharaView targets:
	[FieldOffset(0x310)] internal Texture* Unk310;
	[FieldOffset(0x318)] internal Texture* Unk318;
	[FieldOffset(0x320)] internal Texture* Unk320;
	[FieldOffset(0x328)] internal Texture* Unk328;
	[FieldOffset(0x330)] internal Texture* Unk330;
	[FieldOffset(0x338)] internal Texture* Unk338; // null?
	[FieldOffset(0x340)] internal Texture* Unk340; // null?
	[FieldOffset(0x348)] internal Texture* Unk348; // null?
	[FieldOffset(0x350)] internal Texture* Unk350; // null?
	[FieldOffset(0x358)] internal Texture* Unk358; // null?
	[FieldOffset(0x360)] internal Texture* Unk360;
	[FieldOffset(0x368)] internal Texture* Unk368;
	[FieldOffset(0x370)] internal Texture* Unk370;
	[FieldOffset(0x378)] internal Texture* Unk378; // temp target for ui when changing graphic settings

	// CharaView hair targets:
	[FieldOffset(0x380)] internal Texture* Unk380;
	[FieldOffset(0x388)] internal Texture* Unk388;
	[FieldOffset(0x390)] internal Texture* Unk390;
	[FieldOffset(0x398)] internal Texture* Unk398; // null?
	[FieldOffset(0x3A0)] internal Texture* Unk3A0; // null?
	[FieldOffset(0x3A8)] internal Texture* Unk3A8; // null?
	[FieldOffset(0x3B0)] internal Texture* Unk3B0; // null?
	[FieldOffset(0x3B8)] internal Texture* Unk3B8; // null?
	[FieldOffset(0x3C0)] internal Texture* Unk3C0; // null?
	[FieldOffset(0x3C8)] internal Texture* Unk3C8; // null?
	[FieldOffset(0x3D0)] internal Texture* Unk3D0;
	[FieldOffset(0x3D8)] internal Texture* Unk3D8;
	[FieldOffset(0x3E0)] internal Texture* Unk3E0;
	[FieldOffset(0x420)] internal Texture* Unk420;
	////[FieldOffset(0x428)] internal Texture* Unk428; // Crash

	[FieldOffset(0x4A8)] internal Texture* Unk4A8;
	[FieldOffset(0x4B0)] internal Texture* Unk4B0;
	[FieldOffset(0x4C0)] internal Texture* Unk4C0;
	[FieldOffset(0x4C8)] internal Texture* Unk4C8;
	[FieldOffset(0x4D0)] internal Texture* Unk4D0;
	[FieldOffset(0x4D8)] internal Texture* Unk4D8;
	[FieldOffset(0x4E0)] internal Texture* Unk4E0;
	[FieldOffset(0x4E8)] internal Texture* Unk4E8;
	[FieldOffset(0x528)] internal Texture* Unk528;
	[FieldOffset(0x530)] internal Texture* Unk530;
	[FieldOffset(0x540)] internal Texture* Unk540;
	[FieldOffset(0x548)] internal Texture* Unk548;
	[FieldOffset(0x550)] internal Texture* Unk550;
	[FieldOffset(0x558)] internal Texture* Unk558;
	[FieldOffset(0x560)] internal Texture* Unk560;
	[FieldOffset(0x568)] internal Texture* Unk568;
	[FieldOffset(0x570)] internal Texture* Unk570;
	[FieldOffset(0x578)] internal Texture* Unk578;
	[FieldOffset(0x580)] internal Texture* Unk580;
	[FieldOffset(0x588)] internal Texture* Unk588;
	[FieldOffset(0x590)] internal Texture* Unk590;
	[FieldOffset(0x598)] internal Texture* Unk598;
	[FieldOffset(0x5A0)] internal Texture* Unk5A0;
	[FieldOffset(0x5A8)] internal Texture* Unk5A8;
	[FieldOffset(0x5B0)] internal Texture* Unk5B0;
	[FieldOffset(0x5B8)] internal Texture* Unk5B8;
	[FieldOffset(0x5C0)] internal Texture* Unk5C0;
	[FieldOffset(0x5C8)] internal Texture* Unk5C8;
	[FieldOffset(0x5D0)] internal Texture* Unk5D0;
	[FieldOffset(0x5D8)] internal Texture* Unk5D8;
	[FieldOffset(0x5E0)] internal Texture* Unk5E0;
	[FieldOffset(0x5E8)] internal Texture* Unk5E8;
	[FieldOffset(0x5F0)] internal Texture* Unk5F0;
	[FieldOffset(0x5F8)] internal Texture* Unk5F8;
	[FieldOffset(0x600)] internal Texture* Unk600;
	[FieldOffset(0x608)] internal Texture* Unk608;
	[FieldOffset(0x610)] internal Texture* Unk610;
	[FieldOffset(0x618)] internal Texture* Unk618;
	[FieldOffset(0x620)] internal Texture* Unk620;
	[FieldOffset(0x628)] internal Texture* Unk628;
	[FieldOffset(0x630)] internal Texture* Unk630;
	[FieldOffset(0x638)] internal Texture* Unk638;
	[FieldOffset(0x640)] internal Texture* Unk640;
	[FieldOffset(0x648)] internal Texture* Unk648;
	[FieldOffset(0x650)] internal Texture* Unk650;
	[FieldOffset(0x658)] internal Texture* Unk658;
	[FieldOffset(0x660)] internal Texture* Unk660;
	[FieldOffset(0x668)] internal Texture* Unk668;
	[FieldOffset(0x670)] internal Texture* Unk670;
	[FieldOffset(0x678)] internal nint Unk678;
	[FieldOffset(0x680)] internal nint Unk680;*/

	public static RenderTargetManagerEx* Instance()
	{
		return (RenderTargetManagerEx*)RenderTargetManager.Instance();
	}

	/*public void TestMe()
	{
		var fields = this.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		foreach (FieldInfo field in fields)
		{
			if (field.FieldType == typeof(Texture*))
			{
				object? val = field.GetValue(this);

				if (val is not Pointer pVal)
					continue;

				Texture* pTex = (Texture*)Pointer.Unbox(pVal);
				if (pTex == null)
					continue;

				if (pTex->ActualHeight != ServiceManager.Instance.Rendering.Height ||
					pTex->ActualWidth != ServiceManager.Instance.Rendering.Width)
					continue;

				Logging.Shared.Information($"> {field.Name} {pTex->ActualWidth}x{pTex->ActualHeight} {pTex->TextureFormat}");
			}
		}
	}*/
}
