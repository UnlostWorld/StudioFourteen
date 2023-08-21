// © XivTools.
// Licensed under the MIT license.

//// Ktisis
//// https://github.com/ktisis-tools/Ktisis/
//// https://github.com/ktisis-tools/Ktisis/blob/main/Ktisis/Structs/Actor/Actor.cs
//// https://github.com/ktisis-tools/Ktisis/blob/main/Ktisis/Structs/Actor/ActorDrawData.cs

//// Anamnesis
//// https://github.com/imchillin/Anamnesis/blob/master/Anamnesis/Memory/ActorCustomizeMemory.cs

namespace ScreenshotStudio.Structs;

using System.Runtime.InteropServices;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;

public unsafe class ActorDrawDataViewModel
{
	public ActorDrawDataViewModel(ActorDrawData drawData)
	{
		this.Equipment = new(drawData.Equipment);
	}

	public EquipmentViewModel Equipment { get; init; }
}

[StructLayout(LayoutKind.Explicit)]
public struct ActorDrawData
{
	[FieldOffset(0x010)] public Weapon MainHand;
	[FieldOffset(0x078)] public Weapon OffHand;
	[FieldOffset(0x0E0)] public Weapon Prop;

	[FieldOffset(0x148)] public Equipment Equipment;
	[FieldOffset(0x170)] public Customize Customize;
}
