namespace FFXIVClientStructs.FFXIV.Client.Game.Character;

using Dalamud.Game.ClientState.Objects.Enums;
using global::System;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using StudioFourteen;
using StudioFourteen.GameData;

using HairMakeType = StudioFourteen.GameData.Sheets.HairMakeType;

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

	public static Race? GetRace(ref readonly this CustomizeData self) => ServiceManager.Instance.GameData.GetRow<Race>(self.GetValue(CustomizeIndex.Race));
	public static Tribe? GetTribe(ref readonly this CustomizeData self) => ServiceManager.Instance.GameData.GetRow<Tribe>(self.GetValue(CustomizeIndex.Tribe));
	public static Genders GetGender(ref readonly this CustomizeData self) => (Genders)self.GetValue(CustomizeIndex.Gender);

	public static byte GetValue(ref readonly this CustomizeData self, CustomizeIndex option)
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

	public static CharaMakeType? GetCharaMakeType(ref readonly this CustomizeData self)
	{
		Tribe? tribe = self.GetTribe();
		Genders gender = self.GetGender();

		ExcelSheet<CharaMakeType>? charaMakeTypeSheet = ServiceManager.Instance.GameData.GetSheet<CharaMakeType>();
		if (charaMakeTypeSheet == null)
			return null;

		foreach (CharaMakeType set in charaMakeTypeSheet)
		{
			if (!set.Tribe.IsRow(tribe) || set.Gender != (sbyte)gender)
				continue;

			return set;
		}

		return null;
	}

	public static ImageReference? GetIcon(ref readonly this CustomizeData self)
	{
		ExcelSheet<HairMakeType>? hairMakeTypeSheet = ServiceManager.Instance.GameData.GetSheet<HairMakeType>();
		if (hairMakeTypeSheet == null)
			return null;

		Race? race = self.GetRace();
		Tribe? tribe = self.GetTribe();
		Genders gender = self.GetGender();

		byte hair = self.GetValue(CustomizeIndex.HairStyle);

		foreach (HairMakeType hairMakeType in hairMakeTypeSheet)
		{
			if (!hairMakeType.Race.IsRow(race) || !hairMakeType.Tribe.IsRow(tribe) || hairMakeType.Gender != (sbyte)gender)
				continue;

			RowRef<CharaMakeCustomize>[] makeCustomizeOptions = hairMakeType.HairStyles;

			int length = (byte)makeCustomizeOptions.Length;
			for (byte j = 0; j < length; ++j)
			{
				if (!makeCustomizeOptions[j].IsValid)
					continue;

				CharaMakeCustomize makeCustomize = makeCustomizeOptions[j].Value;
				if (makeCustomize.Icon == 0)
					continue;

				if (makeCustomize.FeatureID == hair)
				{
					return new ImageReference(makeCustomize.Icon);
				}
			}
		}

		return null;
	}
}
