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

public class ItemEquipViewModel
{
	private ItemEquip item;

	public ItemEquipViewModel(ItemEquip item)
	{
		this.item = item;
	}

	public ushort Id
	{
		get => this.item.Id;
		set => this.item.Id = value;
	}

	public byte Variant
	{
		get => this.item.Variant;
		set => this.item.Variant = value;
	}

	public byte Dye
	{
		get => this.item.Dye;
		set => this.item.Dye = value;
	}

	public override string ToString()
	{
		return $"{this.Id}, {this.Variant} ({this.Dye})";
	}
}

[StructLayout(LayoutKind.Explicit, Size = 0x4)]
public struct ItemEquip
{
	[FieldOffset(0)] public ushort Id;
	[FieldOffset(2)] public byte Variant;
	[FieldOffset(3)] public byte Dye;
}
