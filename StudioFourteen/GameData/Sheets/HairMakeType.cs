// .                      @@             _____ _______ _    _ _____ _____ ____
//            @       @@@@@             / ____|__   __| |  | |  __ \_   _/ __ \
//           @@@  @@@@                 | (___    | |  | |  | | |  | || || |  | |
//           @@@@@@@@@  @    @          \___ \   | |  | |  | | |  | || || |  | |
//          @@@@       @@@@@@@          ____) |  | |  | |__| | |__| || || |__| |
//      @@@@@             @@@          |_____/   |_|   \____/|_____/_____\____/
//       @@@      @@@      @@        ___     _    _   _  __   _____  ___  ___  _  _
//        @@    @@@@@@@    @@       |  _|  / _ \ | | | || _ \|_   _|| __|| __|| \| |
//        @@    @@@@@@@    @   @    | __| | (_) || |_| ||   /  | |  | _| | _| | .` |
//      @@@@      @@@      @@@@     |_|    \___/  \___/ |_|_\  |_|  |___||___||_|\_|
//       @@@@             @@@
//         @@@@@      @@@@@               This software is licensed under the
//          @@@@@@@@@@@@@@                 GNU AFFERO GENERAL PUBLIC LICENSE
//              @@@@  @                       Version 3, 19 November 2007

namespace StudioFourteen.GameData.Sheets;

using Lumina.Excel;
using Lumina.Excel.Sheets;

[Sheet("HairMakeType", 0x12B0D41A)]
public readonly struct HairMakeType(ExcelPage page, uint offset, uint row)
	: IExcelRow<HairMakeType>
{
	public const int EntryCount = 100;

	public uint RowId => row;

	public readonly RowRef<Race> Race => new(page.Module, (uint)page.ReadInt32(offset + 4076), page.Language);
	public readonly RowRef<Tribe> Tribe => new(page.Module, (uint)page.ReadInt32(offset + 4080), page.Language);
	public readonly sbyte Gender => page.ReadInt8(offset + 4084);

	public RowRef<CharaMakeCustomize>[] HairStyles
	{
		get
		{
			RowRef<CharaMakeCustomize>[] results = new RowRef<CharaMakeCustomize>[EntryCount];
			for (int i = 0; i < EntryCount; i++)
			{
				uint id = page.ReadUInt32((nuint)(offset + 12 + (4 * i)));
				if (id == 0)
					break;

				results[i] = new RowRef<CharaMakeCustomize>(page.Module, id, page.Language);
			}

			return results;
		}
	}

	public RowRef<CharaMakeCustomize>[] FacePaints
	{
		get
		{
			RowRef<CharaMakeCustomize>[] results = new RowRef<CharaMakeCustomize>[EntryCount];
			for (int i = 0; i < EntryCount; i++)
			{
				uint id = page.ReadUInt32((nuint)(offset + 3008 + (4 * i)));
				if (id == 0)
					break;

				results[i] = new RowRef<CharaMakeCustomize>(page.Module, id, page.Language);
			}

			return results;
		}
	}

	static HairMakeType IExcelRow<HairMakeType>.Create(ExcelPage page, uint offset, uint row) =>
		new(page, offset, row);
}