//// Ktisis
//// https://github.com/ktisis-tools/Ktisis/
//// https://github.com/ktisis-tools/Ktisis/blob/main/Ktisis/Structs/Actor/Actor.cs
//// https://github.com/ktisis-tools/Ktisis/blob/main/Ktisis/Structs/Actor/ActorDrawData.cs

//// Anamnesis
//// https://github.com/imchillin/Anamnesis/blob/master/Anamnesis/Memory/ActorCustomizeMemory.cs

namespace ScreenshotStudio.Structs;

using Dalamud.Game.ClientState.Objects.Enums;
using ScreenshotStudio.GameData;
using ScreenshotStudio.GameData.Excel;
using System;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Explicit, Size = NumOptions)]
public unsafe struct Customize
{
	public const int NumOptions = (int)CustomizeIndex.FacepaintColor + 1;

	[FieldOffset(0)] private fixed byte options[NumOptions];

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

	public Race? Race => GameDataService.GetRow<Race>(this.GetValue(CustomizeIndex.Race));
	public Tribe? Tribe => GameDataService.GetRow<Tribe>(this.GetValue(CustomizeIndex.Tribe));
	public Genders Gender => (Genders)this.GetValue(CustomizeIndex.Gender);

	public byte GetValue(CustomizeIndex option)
	{
		return this.options[(int)option];
	}

	public void SetValue(CustomizeIndex option, byte value)
	{
		this.options[(int)option] = value;
	}

	public void Import(Customize other)
	{
		for (int i = 0; i < NumOptions; i++)
		{
			CustomizeIndex index = (CustomizeIndex)i;
			this.SetValue(index, other.GetValue(index));
		}
	}
}