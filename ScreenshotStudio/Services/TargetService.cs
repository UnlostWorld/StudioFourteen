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
using System;
using NativeObject = FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject;

public class TargetService : ServiceBase
{
	public const int GPoseActorCount = 39;
	public const int GPoseFirstActor = 201;

	public bool IsInGPose => DalamudServices.PluginInterface.UiBuilder.GposeActive;

	public unsafe Actor* GPoseTarget
	{
		get => (Actor*)TargetSystem.Instance()->GPoseTarget;
		set => TargetSystem.Instance()->GPoseTarget = (NativeObject*)value;
	}

	public IntPtr GetObjectTable(int index) => DalamudServices.ObjectTable.GetObjectAddress(index);
}