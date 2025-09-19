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

namespace StudioFourteen.GameData.Sheets;

using Dalamud.Game.ClientState.Objects.Enums;
using Lumina.Excel;
using Lumina.Excel.Sheets;

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