namespace FFXIVClientStructs.FFXIV.Client.Game.Character;

using Dalamud.Game.ClientState.Objects.Enums;
using global::System;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using StudioFourteen;
using StudioFourteen.GameData;
using StudioFourteen.GameData.Sheets;

using CharaMakeType = StudioFourteen.GameData.Sheets.CharaMakeType;

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
		// Ensure a valid tribe is set whenever changing race.
		if (option == CustomizeIndex.Race)
		{
			Race? oldRace = ServiceManager.Instance.GameData.GetRow<Race>(self.GetValue(CustomizeIndex.Race));
			int tribeIndex = -1;
			if (oldRace != null)
				tribeIndex = oldRace.Value.GetTribeIndex(self.GetValue(CustomizeIndex.Tribe));

			if (tribeIndex < 0)
				tribeIndex = 0;

			Race? newRace = ServiceManager.Instance.GameData.GetRow<Race>(value);
			if (newRace != null)
			{
				Tribe[] validTribes = newRace.Value.GetTribes();

				if (tribeIndex > validTribes.Length)
					tribeIndex = 0;

				Tribe newTribe = validTribes[tribeIndex];
				self.Data[(int)CustomizeIndex.Tribe] = (byte)newTribe.RowId;
			}
		}

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

	public static CharaMakeType? GetMakeType(ref readonly this CustomizeData self)
	{
		ExcelSheet<CharaMakeType>? charaMakeTypeSheet = ServiceManager.Instance.GameData.GetSheet<CharaMakeType>();
		if (charaMakeTypeSheet == null)
			return null;

		byte tribe = self.GetValue(CustomizeIndex.Tribe);
		byte gender = self.GetValue(CustomizeIndex.Gender);
		return charaMakeTypeSheet.GetMakeType(tribe, gender);
	}

	public static ImageReference? GetIcon(ref readonly this CustomizeData self)
	{
		return null;
	}
}
