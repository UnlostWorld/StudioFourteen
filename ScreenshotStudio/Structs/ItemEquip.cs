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

[StructLayout(LayoutKind.Explicit, Size = 0x4)]
public struct ItemEquip
{
	[FieldOffset(0)] public ushort Id;
	[FieldOffset(2)] public byte Variant;
	[FieldOffset(3)] public byte Dye;

	public static explicit operator ItemEquip(uint num) => new()
	{
		Id = (ushort)(num & 0xFFFF),
		Variant = (byte)(num >> 16 & 0xFF),
		Dye = (byte)(num >> 24),
	};

	public bool Equals(ItemEquip other) => this.Id == other.Id && this.Variant == other.Variant;
}
