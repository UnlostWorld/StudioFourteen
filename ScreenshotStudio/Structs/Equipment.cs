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

public class EquipmentViewModel
{
	public EquipmentViewModel(Equipment equipment)
	{
		this.Head = new(equipment.Head);
		this.Chest = new(equipment.Chest);
		this.Hands = new(equipment.Hands);
		this.Legs = new(equipment.Legs);
		this.Feet = new(equipment.Feet);
		this.Earring = new(equipment.Earring);
		this.Necklace = new(equipment.Necklace);
		this.Bracelet = new(equipment.Bracelet);
		this.RingRight = new(equipment.RingRight);
		this.RingLeft = new(equipment.RingLeft);
	}

	public ItemEquipViewModel Head { get; init; }
	public ItemEquipViewModel Chest { get; init; }
	public ItemEquipViewModel Hands { get; init; }
	public ItemEquipViewModel Legs { get; init; }
	public ItemEquipViewModel Feet { get; init; }
	public ItemEquipViewModel Earring { get; init; }
	public ItemEquipViewModel Necklace { get; init; }
	public ItemEquipViewModel Bracelet { get; init; }
	public ItemEquipViewModel RingRight { get; init; }
	public ItemEquipViewModel RingLeft { get; init; }
}

[StructLayout(LayoutKind.Explicit)]
public struct Equipment
{
	[FieldOffset(0x00)] public ItemEquip Head;
	[FieldOffset(0x04)] public ItemEquip Chest;
	[FieldOffset(0x08)] public ItemEquip Hands;
	[FieldOffset(0x0C)] public ItemEquip Legs;
	[FieldOffset(0x10)] public ItemEquip Feet;
	[FieldOffset(0x14)] public ItemEquip Earring;
	[FieldOffset(0x18)] public ItemEquip Necklace;
	[FieldOffset(0x1C)] public ItemEquip Bracelet;
	[FieldOffset(0x20)] public ItemEquip RingRight;
	[FieldOffset(0x24)] public ItemEquip RingLeft;
}
