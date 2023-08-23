// © XivTools.
// Licensed under the MIT license.

//// Ktisis
//// https://github.com/ktisis-tools/Ktisis/
//// https://github.com/ktisis-tools/Ktisis/blob/main/Ktisis/Ktisis.cs

//// Brio
//// https://github.com/AsgardXIV/Brio
//// https://github.com/AsgardXIV/Brio/blob/main/Brio/UI/Components/Actor/ActorTab.cs

namespace ScreenshotStudio.Services;

using FFXIVClientStructs.FFXIV.Client.Game.Control;
using ScreenshotStudio.Plugin;
using ScreenshotStudio.Structs;

using NativeObject = FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject;

public class TargetService : ServiceBase
{
	private const int GPoseActorCount = 39;
	private const int GPoseFirstActor = 201;

	private static readonly unsafe TargetSystem* Targets = TargetSystem.Instance();

	public unsafe Actor* Target => (Actor*)Targets->GPoseTarget;
	public bool IsInGPose => DalamudServices.PluginInterface.UiBuilder.GposeActive;
}