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

using Lumina.Excel;
using Lumina.Excel.Sheets;
using Lumina.Text.ReadOnly;
using StudioFourteen.GameData.Extensions;
using StudioFourteen.Tags;

[Sheet("ClassJobCategory", 0x6733E334)]
public readonly struct ClassJobCategory(ExcelPage page, uint offset, uint row)
	: IExcelRow<ClassJobCategory>
{
	public uint RowId => row;

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

	public readonly TagCollection ToTags()
	{
		TagCollection tags = new();

		ExcelSheet<ClassJob>? classJobSheet = ServiceManager.Instance.GameData.GetSheet<ClassJob>();
		if (classJobSheet == null)
			return tags;

		int numJobs = 0;
		ClassJob? exclusiveJob = null;
		foreach (ClassJob classJob in classJobSheet)
		{
			if (this.Contains(classJob))
			{
				numJobs++;
				tags.Add(classJob.ToTags());
				exclusiveJob = classJob;
			}
		}

		if (numJobs == 1 && exclusiveJob != null)
		{
			Tag? tag = exclusiveJob.Value.ToExclusiveTag();
			if (tag != null)
			{
				tags.Add(tag);
			}
		}

		// Special tags
		if (this.IsDiscipleOfWar)
		{
			tags.Add("DOW");
		}
		else if (this.IsDiscipleOfWarOrMagic)
		{
			tags.Add("DOM");
		}
		else if (this.IsDiscipleOfTheLand)
		{
			tags.Add("DOL");
		}
		else if (this.IsDiscipleOfTheHand)
		{
			tags.Add("DOH");
		}
		else if (this.IsDiscipleOfWarOrMagic)
		{
			tags.Add("DOW");
			tags.Add("DOM");
		}
		else if (this.IsDiscipleOfTheLandOrHand)
		{
			tags.Add("DOL");
			tags.Add("DOH");
		}

		return tags;
	}
}