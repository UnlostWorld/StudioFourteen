//// Ktisis
//// https://github.com/ktisis-tools/Ktisis/
//// https://github.com/ktisis-tools/Ktisis/blob/main/Ktisis/Structs/Actor/Actor.cs
//// https://github.com/ktisis-tools/Ktisis/blob/main/Ktisis/Structs/Actor/ActorDrawData.cs

//// Anamnesis
//// https://github.com/imchillin/Anamnesis/blob/master/Anamnesis/Memory/ActorCustomizeMemory.cs

namespace ScreenshotStudio.Structs;

using Dalamud.Game.ClientState.Objects.Enums;
using System;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Explicit, Size = 0x1A)]
public unsafe struct Customize
{
	[FieldOffset(0)] private fixed byte options[0x1A];

	[Flags]
	public enum FacialFeatures : byte
	{
		None = 0x00,
		First = 0x01,
		Second = 0x02,
		Third = 0x04,
		Fourth = 0x08,
		Fifth = 0x10,
		Sixth = 0x20,
		Seventh = 0x40,
		LegacyTattoo = 0x80,
	}

	public byte GetValue(CustomizeIndex option)
	{
		return this.options[(int)option];
	}

	public byte SetValue(CustomizeIndex option, byte value)
	{
		return this.options[(int)option] = value;
	}
}