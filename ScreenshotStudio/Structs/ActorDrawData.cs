// © XivTools.
// Licensed under the MIT license.

//// Ktisis
//// https://github.com/ktisis-tools/Ktisis/
//// https://github.com/ktisis-tools/Ktisis/blob/main/Ktisis/Structs/Actor/Actor.cs
//// https://github.com/ktisis-tools/Ktisis/blob/main/Ktisis/Structs/Actor/ActorDrawData.cs
//// https://github.com/ktisis-tools/Ktisis/blob/main/Ktisis/Interop/Methods.cs

//// Anamnesis
//// https://github.com/imchillin/Anamnesis/blob/master/Anamnesis/Memory/ActorCustomizeMemory.cs

namespace ScreenshotStudio.Structs;

using System.Runtime.InteropServices;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using ScreenshotStudio.GameData;
using ScreenshotStudio.Plugin;
using static ScreenshotStudio.Structs.Equipment;

public static class ActorDrawDataExtensions
{
	private static ChangeEquipDelegate? actorChangeEquip;
	private unsafe delegate void ChangeEquipDelegate(ActorDrawData* writeTo, Equipment.EquipIndex index, ItemEquip item);

	public static unsafe void ChangeEquip(ActorDrawData* drawData, Equipment.EquipIndex index, ItemEquip item)
	{
		if (actorChangeEquip == null)
			actorChangeEquip = DalamudServices.DelegateFromSignature<ChangeEquipDelegate>("E8 ?? ?? ?? ?? 41 B5 01 FF C6");

		actorChangeEquip.Invoke(drawData, index, item);
	}

	public static unsafe void ChangeEquip(ActorDrawData* drawData, ItemSlots slot, ItemEquip item)
	{
		EquipIndex? index = slot switch
		{
			ItemSlots.Head => EquipIndex.Head,
			ItemSlots.Chest => EquipIndex.Chest,
			ItemSlots.Hands => EquipIndex.Hands,
			ItemSlots.Legs => EquipIndex.Legs,
			ItemSlots.Feet => EquipIndex.Feet,
			ItemSlots.Earring => EquipIndex.Earring,
			ItemSlots.Necklace => EquipIndex.Necklace,
			ItemSlots.Bracelet => EquipIndex.Bracelet,
			ItemSlots.RingLeft => EquipIndex.RingLeft,
			ItemSlots.RingRight => EquipIndex.RingRight,
			_ => null,
		};

		if (index == null)
		{
			Logging.Shared.Error($"Attempt to change equipment on unsupported slot: {slot}");
			return;
		}

		ChangeEquip(drawData, (EquipIndex)index, item);
	}
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
