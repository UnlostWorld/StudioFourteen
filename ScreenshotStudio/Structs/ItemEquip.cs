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
using ScreenshotStudio.GameData;

[StructLayout(LayoutKind.Explicit, Size = 0x4)]
public struct ItemEquip
{
	[FieldOffset(0)] public ushort Base;
	[FieldOffset(2)] public byte Variant;
	[FieldOffset(3)] public byte Dye;
}
