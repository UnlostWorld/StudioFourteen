namespace ScreenshotStudio.GameData.Excel;

using System.Collections.Generic;
using Lumina.Data;
using Lumina.Excel;
using Lumina.Excel.GeneratedSheets;
using WpfUtils;

[Sheet("TerritoryType", 0x5baa595e)]
public class Territory : LibraryExcelRow
{
	private static readonly HashSet<uint> HousingTerritories = new()
	{
		282,
		283,
		284,
		342,
		343,
		344,
		345,
		346,
		347,
		384,
		385,
		386,
		608,
		609,
		610,
		649,
		650,
		651,
		652,
	};

	public string? Background { get; protected set; }
	public PlaceName? Place { get; protected set; }
	public PlaceName? Region { get; protected set; }
	public PlaceName? Zone { get; protected set; }
	public List<Weather?> Weathers { get; init; } = new();

	public bool IsHouse => HousingTerritories.Contains(this.RowId);

	public override void PopulateData(RowParser parser, Lumina.GameData gameData, Language language)
	{
		base.PopulateData(parser, gameData, language);

		this.Name = parser.ReadString(0);
		this.Background = parser.ReadString(1);
		this.Region = parser.ReadRowReference<ushort, PlaceName>(3);
		this.Zone = parser.ReadRowReference<ushort, PlaceName>(4);
		this.Place = parser.ReadRowReference<ushort, PlaceName>(5);

		if (this.Zone != null)
			this.Tags.Add(this.Zone.Name.RawString);

		if (this.Region != null)
			this.Tags.Add(this.Region.Name.RawString);

		WeatherRate? weatherRate = parser.ReadRowReference<byte, WeatherRate>(12);

		this.Weathers.Clear();
		if (weatherRate != null && weatherRate.Weathers != null)
		{
			foreach (WeatherRate.WeatherInstance wr in weatherRate.Weathers)
			{
				if (wr.Weather == 0)
					continue;

				this.Weathers.Add(GameDataService.GetRow<Weather>(wr.Weather));
			}
		}
	}

	public override bool Search(string[]? query)
	{
		if (SearchUtility.Matches(this.Name, query))
			return true;

		if (SearchUtility.Matches(this.Background, query))
			return true;

		if (this.Place != null)
		{
			if (SearchUtility.Matches(this.Place.Name.RawString, query))
				return true;
		}

		return base.Search(query);
	}
}
