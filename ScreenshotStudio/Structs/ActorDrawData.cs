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
	private unsafe delegate void ChangeEquipDelegate(ActorDrawData* writeTo, Equipment.EquipIndex index, ItemEquip item, bool force);

	public static unsafe void ChangeEquip(ActorDrawData* drawData, Equipment.EquipIndex index, ItemEquip item, bool force)
	{
		if (actorChangeEquip == null)
			actorChangeEquip = DalamudServices.DelegateFromSignature<ChangeEquipDelegate>("E8 ?? ?? ?? ?? B1 01 41 FF C6");

		actorChangeEquip?.Invoke(drawData, index, item, force);
	}

	public static unsafe void ChangeEquip(ActorDrawData* drawData, ItemSlots slot, ItemEquip item, bool force)
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

		ChangeEquip(drawData, (EquipIndex)index, item, force);
	}
}

[StructLayout(LayoutKind.Explicit)]
public struct ActorDrawData
{
	[FieldOffset(0x010)] public Weapon MainHand;
	[FieldOffset(0x080)] public Weapon OffHand;
	[FieldOffset(0x0F0)] public Weapon Prop;

	[FieldOffset(0x160)] public Equipment Equipment;
	[FieldOffset(0x1B0)] public Customize Customize;

	[FieldOffset(0x1D0)] public ushort Glasses;
}
