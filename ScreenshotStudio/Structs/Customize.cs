// © XivTools.
// Licensed under the MIT license.

//// Ktisis
//// https://github.com/ktisis-tools/Ktisis/
//// https://github.com/ktisis-tools/Ktisis/blob/main/Ktisis/Structs/Actor/Actor.cs
//// https://github.com/ktisis-tools/Ktisis/blob/main/Ktisis/Structs/Actor/ActorDrawData.cs

//// Anamnesis
//// https://github.com/imchillin/Anamnesis/blob/master/Anamnesis/Memory/ActorCustomizeMemory.cs

namespace ScreenshotStudio.Structs;

using System;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Explicit, Size = 0x1A)]
public unsafe struct Customize
{
	[FieldOffset(0x000)] public Races Race;
	[FieldOffset(0x001)] public Genders Gender;
	[FieldOffset(0x002)] public Ages Age;
	[FieldOffset(0x003)] public byte Height;
	[FieldOffset(0x004)] public Tribes Tribe;
	[FieldOffset(0x005)] public byte Face;
	[FieldOffset(0x006)] public byte Hair;
	[FieldOffset(0x007)] public byte HighlightType;
	[FieldOffset(0x008)] public byte SkinTone;
	[FieldOffset(0x009)] public byte RightEyeColor;
	[FieldOffset(0x00a)] public byte HairTone;
	[FieldOffset(0x00b)] public byte Highlights;
	[FieldOffset(0x00c)] public FacialFeatures FacialFeature;
	[FieldOffset(0x00d)] public byte FacialFeatureColor;
	[FieldOffset(0x00e)] public byte Eyebrows;
	[FieldOffset(0x00f)] public byte LeftEyeColor;
	[FieldOffset(0x010)] public byte Eyes;
	[FieldOffset(0x011)] public byte Nose;
	[FieldOffset(0x012)] public byte Jaw;
	[FieldOffset(0x013)] public byte MouthId;
	[FieldOffset(0x014)] public byte LipsToneFurPattern;
	[FieldOffset(0x015)] public byte EarMuscleTailSize;
	[FieldOffset(0x016)] public byte TailEarsType;
	[FieldOffset(0x017)] public byte Bust;
	[FieldOffset(0x018)] public byte FacePaintId;
	[FieldOffset(0x019)] public byte FacePaintColor;

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

	public enum Genders : byte
	{
		Masculine,
		Feminine,
	}

	public enum Races : byte
	{
		Hyur = 1,
		Elezen = 2,
		Lalafell = 3,
		Miqote = 4,
		Roegadyn = 5,
		AuRa = 6,
		Hrothgar = 7,
		Viera = 8,
	}

	public enum Tribes : byte
	{
		Midlander = 1,
		Highlander = 2,
		Wildwood = 3,
		Duskwight = 4,
		Plainsfolk = 5,
		Dunesfolk = 6,
		SunSeeker = 7,
		MoonKeeper = 8,
		SeaWolf = 9,
		Hellsguard = 10,
		Raen = 11,
		Xaela = 12,
		Helion = 13,
		Lost = 14,
		Rava = 15,
		Veena = 16,
	}

	public enum Ages : byte
	{
		Normal = 1,
		Old = 3,
		Young = 4,
	}
}