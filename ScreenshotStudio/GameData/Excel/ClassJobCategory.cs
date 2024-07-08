namespace ScreenshotStudio.GameData.Excel;

using Lumina.Data;
using Lumina.Excel;
using ScreenshotStudio.Tags;
using System.Collections.Generic;

[Sheet("ClassJobCategory", 0x65bbdb12)]
public class ClassJobCategory : LibraryExcelRow
{
	private readonly bool[] classJobs = new bool[(int)ClassJob.ClassJobRows.Count];

	public string? Name { get; set; }

	public List<Entry> ClassJobs { get; init; } = new();

	public bool IsAllClasses => this.RowId == 1;
	public bool IsDiscipleOfWar => this.RowId == 30;
	public bool IsDiscipleOfMagic => this.RowId == 31;
	public bool IsDiscipleOfTheLand => this.RowId == 32;
	public bool IsDiscipleOfTheHand => this.RowId == 33;
	public bool IsDiscipleOfWarOrMagic => this.RowId == 34 || this.RowId == 110 || this.RowId == 192;
	public bool IsDiscipleOfTheLandOrHand => this.RowId == 45;

	public bool IsTanks => this.RowId == 59;
	public bool IsHealers => this.RowId == 64;

	public override void PopulateData(RowParser parser, Lumina.GameData gameData, Language language)
	{
		base.PopulateData(parser, gameData, language);
		this.Name = parser.ReadString(0);

		////ADV = ((parser.ReadColumn<bool>(1) ? ((byte)1) : ((byte)0)) != 0);

		for (var i = 1; i < (int)ClassJob.ClassJobRows.Count; i++)
		{
			bool val = parser.ReadColumn<bool>(i + 1);

			this.classJobs[i - 1] = val;
			this.ClassJobs.Add(new((ClassJob.ClassJobRows)i, val));
		}
	}

	public bool Contains(ClassJob.ClassJobRows classJob)
	{
		// >=(
		if (classJob == ClassJob.ClassJobRows.Count)
			return false;

		return this.classJobs[(int)classJob];
	}

	public bool Contains(ClassJob classJob)
	{
		ClassJob.ClassJobRows classJobRow = (ClassJob.ClassJobRows)classJob.RowId;
		return this.Contains(classJobRow);
	}

	public TagCollection ToTags()
	{
		TagCollection tags = new();

		for (var i = 1; i < (int)ClassJob.ClassJobRows.Count; i++)
		{
			if (this.classJobs[i - 1])
			{
				ClassJob? classJob = GameDataService.GetRow<ClassJob>(i);

				if (classJob == null)
				{
					this.Log.Error($"Unable to find class job row: {i}");
					continue;
				}

				tags.Add(classJob.ToTags());
			}
		}

		// Special tags
		if (this.IsDiscipleOfWar)
		{
			tags.Add("DOW");
		}
		else if (this.IsDiscipleOfMagic)
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

	public class Entry(ClassJob.ClassJobRows classJob, bool enabled)
	{
		public ClassJob.ClassJobRows ClassJobRow { get; private set; } = classJob;
		public bool Enabled { get; private set; } = enabled;

		public ClassJob? ClassJob => GameDataService.GetRow<ClassJob>((int)this.ClassJobRow);
	}
}
