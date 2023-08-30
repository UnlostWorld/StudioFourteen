// © XivTools.
// Licensed under the MIT license.

namespace ScreenshotStudio.GameData.Excel;

using Lumina.Data;
using Lumina.Excel;
using ScreenshotStudio.Tags;
using System;

[Sheet("ClassJobCategory", 2091841742u)]
public class ClassJobCategory : LibraryExcelRow
{
	private readonly bool[] classJobs = new bool[(int)ClassJob.ClassJobRows.Count];

	public enum ClassJobCategoryRows : uint
	{
		None,
		AllClasses,

		DiscipleOfWar = 30,
		DiscipleOfMagic = 31,
		DiscipleOfTheLand = 32,
		DiscipleOfTheHand = 33,
		DiscipleOfWarOrMagic = 34,
		DiscipleOfTheLandOrHand = 45,
		DiscipleOfWarOrMagic2 = 110,
		DiscipleOfWarOrMagic3 = 192,
	}

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

		// Special tags
		if (Enum.IsDefined((ClassJobCategoryRows)this.RowId))
		{
			ClassJobCategoryRows row = (ClassJobCategoryRows)this.RowId;

			if (this.RowId == (uint)ClassJobCategoryRows.DiscipleOfWar)
			{
				tags.Add(row.ToString()).WithAlias("DOW");
			}
			else if (this.RowId == (uint)ClassJobCategoryRows.DiscipleOfMagic)
			{
				tags.Add(row.ToString()).WithAlias("DOM");
			}
			else if (this.RowId == (uint)ClassJobCategoryRows.DiscipleOfTheLand)
			{
				tags.Add(row.ToString()).WithAlias("DOL");
			}
			else if (this.RowId == (uint)ClassJobCategoryRows.DiscipleOfTheHand)
			{
				tags.Add(row.ToString()).WithAlias("DOH");
			}
			else if (this.RowId == (uint)ClassJobCategoryRows.DiscipleOfWarOrMagic
				|| this.RowId == (uint)ClassJobCategoryRows.DiscipleOfWarOrMagic2
				|| this.RowId == (uint)ClassJobCategoryRows.DiscipleOfWarOrMagic3)
			{
				tags.Add(ClassJobCategoryRows.DiscipleOfWar.ToString()).WithAlias("DOW");
				tags.Add(ClassJobCategoryRows.DiscipleOfMagic.ToString()).WithAlias("DOM");
			}
			else if (this.RowId == (uint)ClassJobCategoryRows.DiscipleOfTheLandOrHand)
			{
				tags.Add(ClassJobCategoryRows.DiscipleOfTheLand.ToString()).WithAlias("DOL");
				tags.Add(ClassJobCategoryRows.DiscipleOfTheHand.ToString()).WithAlias("DOH");
			}
		}

		return tags;
	}
}
