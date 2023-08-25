// © XivTools.
// Licensed under the MIT license.

namespace ScreenshotStudio.GameData.Excel;

using System.Collections.Generic;
using Lumina.Data;
using Lumina.Excel;
using Lumina.Excel.GeneratedSheets;

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

	public string? Name { get; protected set; }
	public string? Place { get; protected set; }
	public string? Region { get; protected set; }
	public string? Zone { get; protected set; }
	public List<Weather?> Weathers { get; init; } = new();

	public bool IsHouse => HousingTerritories.Contains(this.RowId);

	public override void PopulateData(RowParser parser, Lumina.GameData gameData, Language language)
	{
		base.PopulateData(parser, gameData, language);

		this.Name = parser.ReadString(0);
		this.Region = parser.ReadRowReference<ushort, PlaceName>(3)?.Name ?? "Unknown";
		this.Zone = parser.ReadRowReference<ushort, PlaceName>(4)?.Name ?? "Unknown";
		this.Place = parser.ReadRowReference<ushort, PlaceName>(5)?.Name ?? "Unknown";

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
}
