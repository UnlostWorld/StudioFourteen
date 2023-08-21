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

public class ItemEquipViewModel : StructViewModelBase<ItemEquip>
{
	public ushort Id
	{
		get => this.GetValue<ushort>();
		set => this.SetValue(value);
	}

	public byte Variant
	{
		get => this.GetValue<byte>();
		set => this.SetValue(value);
	}

	public byte Dye
	{
		get => this.GetValue<byte>();
		set => this.SetValue(value);
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
