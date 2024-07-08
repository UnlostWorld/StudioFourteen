namespace ScreenshotStudio.GameData.Excel;

using Lumina.Data;
using Lumina.Excel;
using ScreenshotStudio.Tags;

[Sheet("EquipRaceCategory", 0xf914b198)]
public class EquipRaceCategory : StudioExcelRow
{
	private readonly bool[] races = new bool[(int)Race.RaceRows.Count - 1];

	public bool Male { get; private set; }
	public bool Female { get; private set; }

	public override void PopulateData(RowParser parser, Lumina.GameData gameData, Language language)
	{
		base.PopulateData(parser, gameData, language);

		for (var i = 0; i < (int)Race.RaceRows.Count - 1; i++)
		{
			this.races[i] = parser.ReadColumn<bool>(i);
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

		return this.races[race.RowId];
	}

	public TagCollection ToTags()
	{
		TagCollection tags = new();

		if (this.Male)
			tags.Add("Masculine");

		if (this.Female)
			tags.Add("Feminine");

		for (int i = 0; i < this.races.Length; i++)
		{
			if (this.races[i] == true)
			{
				Race? race = GameDataService.GetRow<Race>(i + 1);
				if (race != null)
				{
					tags.Add(race.Name);
				}
			}
		}

		return tags;
	}
}
