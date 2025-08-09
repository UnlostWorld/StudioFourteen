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

namespace StudioFourteen.Interop;

using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Client.System.Input;
using FFXIVClientStructs.FFXIV.Client.UI;
using SharpDX.Direct3D9;
using StudioFourteen.Interop.Structs.Environment;
using StudioFourteen.Plugin;
using System;
using System.Runtime.InteropServices;
using Windows.Win32.UI.WindowsAndMessaging;

public static unsafe class Hooks
{
	#pragma warning disable SA1201

	// Brio Signatures:
	// 		https://github.com/Etheirys/Brio
	// 		Special thanks to @Minmoose @AsgardXIV

	// https://github.com/Etheirys/Brio/blob/main/Brio/Game/Actor/ActorAppearanceService.cs#L58
	internal delegate byte EnforceKindRestrictionsDelegate(nint a1, nint a2);
	internal static readonly SignatureHook<EnforceKindRestrictionsDelegate> EnforceKind = new("E8 ?? ?? ?? ?? 41 B0 ?? 48 8B D6 48 8B");

	// https://github.com/Etheirys/Brio/blob/main/Brio/Game/Camera/CameraService.cs#L60
	internal delegate nint SceneCameraUpdateDelegate(FFXIVClientStructs.FFXIV.Client.Graphics.Scene.Camera* sceneCamera);
	internal static readonly SignatureHook<SceneCameraUpdateDelegate> SceneCameraUpdate = new("48 ?? ?? ?? ?? ?? 48 81 EC ?? ?? ?? ?? F6 81 F0 ?? ?? ?? ?? 48 8B ??");

	// https://github.com/Etheirys/Brio/blob/main/Brio/Game/Camera/CameraService.cs#L64
	internal delegate void CameraMatrixLoadDelegate(FFXIVClientStructs.FFXIV.Client.Graphics.Render.Camera* camera, nint a1);
	internal static readonly SignatureHook<CameraMatrixLoadDelegate> CameraMatrixLoad = new("E8 ?? ?? ?? ?? 48 8B 93 90 02 ?? ?? 48 8D 4C 24 40");

	// https://github.com/Etheirys/Brio/blob/main/Brio/Game/Camera/CameraService.cs#L56
	internal delegate nint GPoseCameraUpdateDelegate(StudioFourteen.Scene.Cameras.GroupPoseCamera* camera);
	internal static readonly SignatureHook<GPoseCameraUpdateDelegate> GPoseCameraUpdate = new("40 55 53 57 48 8D 6C 24 A0 48 81 EC ?? ?? ?? ?? 48 8B 1D");

	// https://github.com/Etheirys/Brio/blob/main/Brio/Game/Posing/SkeletonService.cs#L59
	internal delegate nint UpdateBonePhysicsDelegate(nint a1);
	internal static readonly SignatureHook<UpdateBonePhysicsDelegate> UpdateBonePhysics = new("48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 41 54 41 56 48 83 EC ?? 48 8B 59 ?? 45 33 E4");

	// https://github.com/Etheirys/Brio/blob/main/Brio/Game/Posing/SkeletonService.cs#L63
	internal delegate void FinalizeSkeletonsDelegate(nint a1);
	internal static readonly SignatureHook<FinalizeSkeletonsDelegate> FinalizeSkeletons = new("40 53 55 57 41 55 48 83 EC 68");

	// https://github.com/Etheirys/Brio/blob/main/Brio/Game/Core/ObjectMonitorService.cs#L40
	internal delegate nint CharacterEventDelegate(Character* character);
	internal static readonly SignatureHook<CharacterEventDelegate> CharacterInitialize = new("E8 ?? ?? ?? ?? 8D 57 ?? C6 83");

	// https://github.com/Etheirys/Brio/blob/main/Brio/Game/Core/ObjectMonitorService.cs#L44
	internal static readonly SignatureHook<CharacterEventDelegate> CharacterFinalize = new("48 89 5C 24 ?? 48 89 74 24 ?? 57 48 83 EC ?? 48 8D 05 ?? ?? ?? ?? 48 8B D9 48 89 01 48 8D 05 ?? ?? ?? ?? 48 89 81 ?? ?? ?? ?? 48 81 C1");

	// https://github.com/Etheirys/Brio/blob/main/Brio/Game/World/TimeService.cs#L98
	internal delegate void UpdateEorzeaTimeDelegate(IntPtr a1, IntPtr a2);
	internal static readonly SignatureHook<UpdateEorzeaTimeDelegate> UpdateEorzeaTime = new("48 89 5C 24 ?? 57 48 83 EC ?? 48 8B F9 48 8B DA 48 81 C1 ?? ?? ?? ?? E8 ?? ?? ?? ?? 4C");

	// https://github.com/Etheirys/Brio/blob/main/Brio/Game/Actor/ActionTimelineService.cs#L41
	internal static readonly SignatureHook<TimelineContainer.Delegates.CalculateAndApplyOverallSpeed> CalculateAndApplyOverallSpeedHook = new ("E8 ?? ?? ?? ?? 48 8D 8B ?? ?? ?? ?? 48 8B 01 FF 50 ?? 48 8D 8B ?? ?? ?? ?? 48 8B 01 FF 50 ?? F6 83");

	// Client Struct Hooks
	// 		https://github.com/aers/FFXIVClientStructs
	// 		Big thanks to everyone in the Client Structs team. ❤
	internal unsafe delegate bool TickDelegate(Framework* pFramework);
	internal static readonly AddressHook<TickDelegate> Tick = new(() => (nint)Framework.StaticVirtualTablePointer->Tick);

	internal delegate bool EnterGroupPoseDelegate(UIModule* uiModule);
	internal static readonly AddressHook<EnterGroupPoseDelegate> EnterGroupPose = new(() => (nint)Framework.Instance()->UIModule->VirtualTable->EnterGPose);

	internal delegate void ExitGroupPoseDelegate(UIModule* uiModule);
	internal static readonly AddressHook<ExitGroupPoseDelegate> ExitGroupPose = new(() => (nint)Framework.Instance()->UIModule->VirtualTable->ExitGPose);

	internal static readonly AddressHook<PadDevice.Delegates.Poll> PadDevicePoll = new(() => (nint)PadDevice.StaticVirtualTablePointer->Poll);
	internal static readonly AddressHook<GameObject.Delegates.SetPosition> SetPosition = new(() => GameObject.Addresses.SetPosition.Value);

	// Dalamud Signatures:
	//  	https://github.com/goatcorp/Dalamud
	// 		Special thanks to @goaaats
	[UnmanagedFunctionPointer(CallingConvention.ThisCall)]
	internal delegate int DxgiSwapChainPresentDelegate(nint* swapChain, uint syncInterval, uint flags);

	// https://github.com/goatcorp/Dalamud/blob/master/Dalamud/Interface/Internal/InterfaceManager.cs#L1043
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	internal delegate IntPtr SetUser32CursorDelegate(HCURSOR hCursor);
	internal static readonly ImportHook<SetUser32CursorDelegate> SetCursor = new(null, "user32.dll", "SetCursor", 0);

	// Ktisis Signatures:
	// 		https://github.com/ktisis-tools/Ktisis/
	// 		Special thanks to @chirpxiv

	// https://github.com/ktisis-tools/Ktisis/blob/v0.3/main/Ktisis/Scene/Modules/EnvModule.cs#L97
	internal unsafe delegate nint EnvStateCopyDelegate(EnvState* dest, EnvState* src);
	internal static readonly SignatureHook<EnvStateCopyDelegate> EnvStateCopy = new("E8 ?? ?? ?? ?? 49 3B F5 75 0D");

	// Title Edit Signatures:
	// 		https://github.com/Caraxi/TitleEditPlugin
	// 		Special thanks to @Caraxi

	// https://github.com/Caraxi/TitleEditPlugin/blob/master/TitleEdit/TitleEditAddressResolver.cs#L40
	internal delegate int CreateSceneDelegate(string p1, uint p2, IntPtr p3, uint p4, IntPtr p5, int p6, uint p7);
	internal static readonly SignatureHook<CreateSceneDelegate> CreateScene = new("E8 ?? ?? ?? ?? 66 89 1D ?? ?? ?? ?? E9 ?? ?? ?? ??");
}