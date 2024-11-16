namespace StudioFourteen.GameData.Sheets;

using Dalamud.Game.ClientState.Objects.Enums;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using Lumina.Text.ReadOnly;

public static class CharaMakeTypeExtensions
{
	public static CharaMakeType? GetMakeType(this ExcelSheet<CharaMakeType> self, Tribe tribe, Genders gender)
	{
		foreach (CharaMakeType makeType in self)
		{
			if (!makeType.Tribe.IsRow(tribe.RowId) || makeType.Gender != (sbyte)gender)
				continue;

			return makeType;
		}

		return null;
	}

	public static CharaMakeType? GetMakeType(this ExcelSheet<CharaMakeType> self, byte tribe, byte gender)
	{
		foreach (CharaMakeType makeType in self)
		{
			if (!makeType.Tribe.IsRow(tribe) || makeType.Gender != (sbyte)gender)
				continue;

			return makeType;
		}

		return null;
	}

	public static CharaMakeType.CharaMakeMenu? GetMenu(this CharaMakeType self, CustomizeIndex index)
	{
		foreach (CharaMakeType.CharaMakeMenu menu in self.CharaMakeStruct)
		{
			if (menu.Customize == (uint)index)
			{
				return menu;
			}
		}

		return null;
	}
}