//// Ktisis
//// https://github.com/ktisis-tools/Ktisis/
//// https://github.com/ktisis-tools/Ktisis/blob/main/Ktisis/Structs/Actor/Actor.cs
//// https://github.com/ktisis-tools/Ktisis/blob/main/Ktisis/Structs/Actor/ActorDrawData.cs

//// Anamnesis
//// https://github.com/imchillin/Anamnesis/blob/master/Anamnesis/Memory/ActorCustomizeMemory.cs

namespace ScreenshotStudio.Structs;

using ScreenshotStudio.GameData;
using System;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Explicit)]
public struct Equipment
{
	[FieldOffset(0x000)] public ItemEquip Head;
	[FieldOffset(0x008)] public ItemEquip Chest;
	[FieldOffset(0x010)] public ItemEquip Hands;
	[FieldOffset(0x018)] public ItemEquip Legs;
	[FieldOffset(0x020)] public ItemEquip Feet;
	[FieldOffset(0x028)] public ItemEquip Earring;
	[FieldOffset(0x030)] public ItemEquip Necklace;
	[FieldOffset(0x038)] public ItemEquip Bracelet;
	[FieldOffset(0x040)] public ItemEquip RingRight;
	[FieldOffset(0x048)] public ItemEquip RingLeft;

	public enum EquipIndex : uint
	{
		Head,
		Chest,
		Hands,
		Legs,
		Feet,
		Earring,
		Necklace,
		Bracelet,
		RingRight,
		RingLeft,

		Count,
	}

	public ItemEquip GetItem(EquipIndex index)
	{
		switch (index)
		{
			case EquipIndex.Head: return this.Head;
			case EquipIndex.Chest: return this.Chest;
			case EquipIndex.Hands: return this.Hands;
			case EquipIndex.Legs: return this.Legs;
			case EquipIndex.Feet: return this.Feet;
			case EquipIndex.Earring: return this.Earring;
			case EquipIndex.Necklace: return this.Necklace;
			case EquipIndex.Bracelet: return this.Bracelet;
			case EquipIndex.RingRight: return this.RingRight;
			case EquipIndex.RingLeft: return this.RingLeft;
		}

		throw new Exception($"Invalid equip index: {index}");
	}

	public void SetItem(EquipIndex index, ItemEquip item)
	{
		switch (index)
		{
			case EquipIndex.Head:
			{
				this.Head = item;
				break;
			}

			case EquipIndex.Chest:
			{
				this.Chest = item;
				break;
			}

			case EquipIndex.Hands:
			{
				this.Hands = item;
				break;
			}

			case EquipIndex.Legs:
			{
				this.Legs = item;
				break;
			}

			case EquipIndex.Feet:
			{
				this.Feet = item;
				break;
			}

			case EquipIndex.Earring:
			{
				this.Earring = item;
				break;
			}

			case EquipIndex.Necklace:
			{
				this.Necklace = item;
				break;
			}

			case EquipIndex.Bracelet:
			{
				this.Bracelet = item;
				break;
			}

			case EquipIndex.RingRight:
			{
				this.RingRight = item;
				break;
			}

			case EquipIndex.RingLeft:
			{
				this.RingLeft = item;
				break;
			}
		}

		throw new Exception($"Invalid equip index: {index}");
	}
}
