namespace FFXIVClientStructs.FFXIV.Client.Game.Character;

using Dalamud.Game.ClientState.Objects.Enums;
using global::System;
using ScreenshotStudio;
using ScreenshotStudio.GameData;
using ScreenshotStudio.GameData.Excel;

public static class CustomizeDataExtensions
{
	public const int NumOptions = (int)CustomizeIndex.FacepaintColor + 1;

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

	public static Race? GetRace(ref this CustomizeData self) => GameDataService.GetRow<Race>(self.GetValue(CustomizeIndex.Race));
	public static Tribe? GetTribe(ref this CustomizeData self) => GameDataService.GetRow<Tribe>(self.GetValue(CustomizeIndex.Tribe));
	public static Genders GetGender(ref this CustomizeData self) => (Genders)self.GetValue(CustomizeIndex.Gender);

	public static byte GetValue(ref this CustomizeData self, CustomizeIndex option)
	{
		return self.Data[(int)option];
	}

	public static void SetValue(ref this CustomizeData self, CustomizeIndex option, byte value)
	{
		self.Data[(int)option] = value;
	}

	public static void Import(ref this CustomizeData self, CustomizeData other)
	{
		for (int i = 0; i < NumOptions; i++)
		{
			CustomizeIndex index = (CustomizeIndex)i;
			self.SetValue(index, other.GetValue(index));
		}
	}

	public static CharaMakeType? GetCharaMakeType(ref this CustomizeData self)
	{
		Tribe? tribe = self.GetTribe();
		Genders gender = self.GetGender();

		DataSheet<CharaMakeType>? charaMakeTypeSheet = ServiceManager.Instance.GameData.GetSheet<CharaMakeType>();
		if (charaMakeTypeSheet == null)
			return null;

		foreach (CharaMakeType set in charaMakeTypeSheet)
		{
			if (set.Tribe != tribe || set.Gender != gender)
				continue;

			return set;
		}

		return null;
	}

	public static ImageReference? GetIcon(ref this CustomizeData self)
	{
		DataSheet<HairMakeType>? hairMakeTypeSheet = GameDataService.Get<HairMakeType>();
		if (hairMakeTypeSheet == null)
			return null;

		Race? race = self.GetRace();
		Tribe? tribe = self.GetTribe();
		Genders gender = self.GetGender();

		byte hair = self.GetValue(CustomizeIndex.HairStyle);

		foreach (HairMakeType? hairMakeType in hairMakeTypeSheet)
		{
			if (hairMakeType == null)
				continue;

			if (hairMakeType.Race != race || hairMakeType.Tribe != tribe || hairMakeType.Gender != gender)
				continue;

			CharaMakeCustomize?[] makeCustomizeOptions = hairMakeType.HairStyles;

			int length = (byte)makeCustomizeOptions.Length;
			for (byte j = 0; j < length; ++j)
			{
				CharaMakeCustomize? makeCustomize = makeCustomizeOptions[j];
				if (makeCustomize == null || makeCustomize.Icon == null || makeCustomize.Icon.ImageId == 0)
					continue;

				if (makeCustomize.FeatureId == hair)
				{
					return makeCustomize.Icon;
				}
			}
		}

		return null;
	}
}
