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

namespace StudioFourteen.Services.Library.GameData.Sheets;

using System.Collections.Generic;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using Lumina.Text.ReadOnly;

[Sheet("ClassJobCategory", 0x6733E334)]
public readonly struct ClassJobCategory(ExcelPage page, uint offset, uint row)
	: IExcelRow<ClassJobCategory>
{
	private readonly List<ClassJob> classJobs = new();

	public ExcelPage ExcelPage => page;
	public uint RowOffset => offset;
	public uint RowId => row;

	public List<ClassJob> ClassJobs
	{
		get
		{
			if (this.classJobs.Count == 0)
			{
				foreach (ClassJob classJob in Studio.DataManager.GetExcelSheet<ClassJob>())
				{
					if (classJob.RowId == 0)
						continue;

					if (this.Contains(classJob))
					{
						this.classJobs.Add(classJob);
					}
				}
			}

			return this.classJobs;
		}
	}

	public readonly ReadOnlySeString Name => page.ReadString(offset, offset);
	public readonly bool IsAllClasses => row == 1;
	public readonly bool IsDiscipleOfWar => row == 30;
	public readonly bool IsDiscipleOfMagic => row == 31;
	public readonly bool IsDiscipleOfTheLand => row == 32;
	public readonly bool IsDiscipleOfTheHand => row == 33;
	public readonly bool IsDiscipleOfWarOrMagic => row == 34 || row == 110 || row == 192;
	public readonly bool IsDiscipleOfTheLandOrHand => row == 45;
	public readonly bool IsTanks => row == 59;
	public readonly bool IsHealers => row == 64;

	static ClassJobCategory IExcelRow<ClassJobCategory>.Create(ExcelPage page, uint offset, uint row) =>
		new(page, offset, row);

	public readonly bool Contains(ClassJob classJob) => page.ReadBool(offset + 4 + classJob.RowId);
}