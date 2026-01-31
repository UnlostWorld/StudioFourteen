// .                    @@             _____ _______ _    _ _____ _____ ____
//          @       @@@@@             / ____|__   __| |  | |  __ \_   _/ __ \
//         @@@  @@@@                 | (___    | |  | |  | | |  | || || |  | |
//         @@@@@@@@@  @    @          \___ \   | |  | |  | | |  | || || |  | |
//        @@@@       @@@@@@@          ____) |  | |  | |__| | |__| || || |__| |
//    @@@@@             @@@          |_____/   |_|   \____/|_____/_____\____/
//     @@@      @@@      @@        ___     _    _   _  __   _____  ___  ___  _  _
//      @@    @@@@@@@    @@       |  _|  / _ \ | | | || _ \|_   _|| __|| __|| \| |
//      @@    @@@@@@@    @   @    | __| | (_) || |_| ||   /  | |  | _| | _| | .` |
//    @@@@      @@@      @@@@     |_|    \___/  \___/ |_|_\  |_|  |___||___||_|\_|
//     @@@@             @@@        https://github.com/UnlostWorld/StudioFourteen
//       @@@@@      @@@@@
//        @@@@@@@@@@@@@@                This software is licensed under the
//            @@@@  @                  GNU AFFERO GENERAL PUBLIC LICENSE v3

namespace FFXIVClientStructs.FFXIV.Client.Game.Character;

using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Plugin.Services;
using global::System;
using global::System.Text;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using StudioFourteen;
using StudioFourteen.Services.Library.GameData;
using StudioFourteen.Services.Library.GameData.Sheets;

using CharaMakeType = StudioFourteen.Services.Library.GameData.Sheets.CharaMakeType;
using HairMakeType = StudioFourteen.Services.Library.GameData.Sheets.HairMakeType;

public static class CustomizeDataExtensions
{
	public const int NumOptions = (int)CustomizeIndex.FacepaintColor + 1;
	public static readonly CustomizeData Empty;

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

	public static Race? GetRace(this CustomizeData self) => Studio.DataManager.GetRow<Race>(self.GetValue(CustomizeIndex.Race));
	public static Tribe? GetTribe(this CustomizeData self) => Studio.DataManager.GetRow<Tribe>(self.GetValue(CustomizeIndex.Tribe));
	public static Genders GetGender(this CustomizeData self) => (Genders)self.GetValue(CustomizeIndex.Gender);

	public static byte GetValue(ref readonly this CustomizeData self, CustomizeIndex option)
	{
		return self.Data[(int)option];
	}

	public static void SetValue(ref this CustomizeData self, byte option, byte value)
	{
		// Ensure a valid tribe is set whenever changing race.
		if (option == 0)
		{
			Race? oldRace = Studio.DataManager.GetRow<Race>(self.GetValue(CustomizeIndex.Race));
			int tribeIndex = -1;
			if (oldRace != null && oldRace.Value.RowId != 0)
				tribeIndex = oldRace.Value.GetTribeIndex(self.GetValue(CustomizeIndex.Tribe));

			if (tribeIndex < 0)
				tribeIndex = 0;

			Race? newRace = Studio.DataManager.GetRow<Race>(value);
			if (newRace != null && value != 0)
			{
				Tribe[] validTribes = newRace.Value.GetTribes();

				if (tribeIndex > validTribes.Length)
					tribeIndex = 0;

				Tribe newTribe = validTribes[tribeIndex];
				self.Data[(int)CustomizeIndex.Tribe] = (byte)newTribe.RowId;
			}
		}

		self.Data[option] = value;
	}

	public static void SetValue(ref this CustomizeData self, CustomizeIndex option, byte value)
	{
		self.SetValue((byte)option, value);
	}

	public static void Import(ref this CustomizeData self, CustomizeData other)
	{
		for (int i = 0; i < NumOptions; i++)
		{
			CustomizeIndex index = (CustomizeIndex)i;
			self.SetValue(index, other.GetValue(index));
		}
	}

	public static CharaMakeType? GetMakeType(ref readonly this CustomizeData self)
	{
		ExcelSheet<CharaMakeType>? charaMakeTypeSheet = Studio.DataManager.GetExcelSheet<CharaMakeType>();
		if (charaMakeTypeSheet == null)
			return null;

		byte tribe = self.GetValue(CustomizeIndex.Tribe);
		byte gender = self.GetValue(CustomizeIndex.Gender);
		return charaMakeTypeSheet.GetMakeType(tribe, gender);
	}

	public static ImageReference? GetIcon(this CustomizeData self)
	{
		ExcelSheet<HairMakeType>? hairMakeTypeSheet = Studio.DataManager.GetExcelSheet<HairMakeType>();
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

	public static void GetHash(this CustomizeData self, ref StringBuilder stringBuilder)
	{
		foreach (byte b in self.Data)
		{
			stringBuilder.Append(b.ToString("X2"));
		}
	}
}
