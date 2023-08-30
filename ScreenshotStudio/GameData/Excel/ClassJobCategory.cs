// © XivTools.
// Licensed under the MIT license.

namespace ScreenshotStudio.GameData.Excel;

using Lumina.Data;
using Lumina.Excel;
using ScreenshotStudio.Tags;

[Sheet("ClassJobCategory", 2091841742u)]
public class ClassJobCategory : LibraryExcelRow
{
	private readonly bool[] classJobs = new bool[(int)ClassJob.ClassJobRows.Count];

	public string? Name { get; set; }

	public override void PopulateData(RowParser parser, Lumina.GameData gameData, Language language)
	{
		base.PopulateData(parser, gameData, language);
		this.Name = parser.ReadString(0);

		////ADV = ((parser.ReadColumn<bool>(1) ? ((byte)1) : ((byte)0)) != 0);

		for (var i = 0; i < (int)ClassJob.ClassJobRows.Count; i++)
		{
			this.classJobs[i] = parser.ReadColumn<bool>(i + 2);
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

		for (var i = 0; i < (int)ClassJob.ClassJobRows.Count; i++)
		{
			if (this.classJobs[i])
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

		return tags;
	}
}
