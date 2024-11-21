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
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using Lumina.Excel;

[Sheet("BNpcCustomize", 0x18F060D4)]
public readonly struct BNpcCustomize(ExcelPage page, uint offset, uint row)
	: IExcelRow<BNpcCustomize>
{
	public uint RowId => row;

	public readonly CustomizeData Data
	{
		get
		{
			CustomizeData c = default;

			for (int i = 0; i < CustomizeDataExtensions.NumOptions; i++)
			{
				CustomizeIndex index = (CustomizeIndex)i;
				byte val = page.ReadUInt8((nuint)(offset + i));
				c.SetValue(index, val);
			}

			return c;
		}
	}

	static BNpcCustomize IExcelRow<BNpcCustomize>.Create(ExcelPage page, uint offset, uint row) => new(page, offset, row);
}