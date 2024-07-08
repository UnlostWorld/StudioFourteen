namespace ScreenshotStudio.GameData.Excel;

using Lumina.Data;
using Lumina.Excel;
using ScreenshotStudio.Tags;

[Sheet("EquipRaceCategory", 0xf914b198)]
public class EquipRaceCategory : StudioExcelRow
{
	private readonly bool[] races = new bool[(int)Race.RaceRows.Count];

	public bool Male { get; private set; }
	public bool Female { get; private set; }

	public override void PopulateData(RowParser parser, Lumina.GameData gameData, Language language)
	{
		base.PopulateData(parser, gameData, language);

		for (var i = 1; i < (int)Race.RaceRows.Count; i++)
		{
			this.races[i] = parser.ReadColumn<bool>(i - 1);
		}

		this.Male = parser.ReadColumn<bool>(8);
		this.Female = parser.ReadColumn<bool>(9);
	}

	public bool CanEquip(Race race, Genders gender)
	{
		if (!this.Male && gender == Genders.Masculine)
			return false;

		if (!this.Female && gender == Genders.Feminine)
			return false;

		return this.races[race.RowId - 1];
	}

	public TagCollection ToTags()
	{
		TagCollection tags = new();

		if (this.Male)
			tags.Add("Masculine");

		if (this.Female)
			tags.Add("Feminine");

		for(int i = 1; i < (int)Race.RaceRows.Count; i++)
		{
			if (i == (int)Race.RaceRows.Hyur)
			{
				this.Log.Information($"{i} = {this.races[i - 1]}");
			}

			if (this.races[i - 1] == true)
			{
				Race? race = GameDataService.GetRow<Race>(i);
				if (race != null)
				{
					tags.Add(race.Name);
				}
			}
		}

		return tags;
	}
}
