// © XivTools.
// Licensed under the MIT license.

//// Ktisis
//// https://github.com/ktisis-tools/Ktisis/
//// https://github.com/ktisis-tools/Ktisis/blob/main/Ktisis/Structs/Actor/Actor.cs
//// https://github.com/ktisis-tools/Ktisis/blob/main/Ktisis/Structs/Actor/ActorDrawData.cs

//// Anamnesis
//// https://github.com/imchillin/Anamnesis/blob/master/Anamnesis/Memory/ActorCustomizeMemory.cs

namespace ScreenshotStudio.Structs;

using ScreenshotStudio.GameData;
using System.Runtime.InteropServices;

public class EquipmentViewModel : StructViewModelBase<Equipment>
{
	public ItemEquipViewModel Head { get; init; } = new(EquipSlots.Head);
	public ItemEquipViewModel Chest { get; init; } = new(EquipSlots.Chest);
	public ItemEquipViewModel Hands { get; init; } = new(EquipSlots.Hands);
	public ItemEquipViewModel Legs { get; init; } = new(EquipSlots.Legs);
	public ItemEquipViewModel Feet { get; init; } = new(EquipSlots.Feet);
	public ItemEquipViewModel Earring { get; init; } = new(EquipSlots.Earring);
	public ItemEquipViewModel Necklace { get; init; } = new(EquipSlots.Necklace);
	public ItemEquipViewModel Bracelet { get; init; } = new(EquipSlots.Bracelet);
	public ItemEquipViewModel RingRight { get; init; } = new(EquipSlots.RingRight);
	public ItemEquipViewModel RingLeft { get; init; } = new(EquipSlots.RingLeft);
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
