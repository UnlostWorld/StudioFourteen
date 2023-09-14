// © XivTools.
// Licensed under the MIT license.

namespace ScreenshotStudio.GameData.Excel;

using Lumina.Data;
using Lumina.Excel;

[Sheet("BNpcName", 0x77a72da0)]
public class BattleNpcName : StudioExcelRow
{
	public string Name { get; private set; } = string.Empty;
	public string? Description => $"N:{this.RowId.ToString("D7")}";

	public override void PopulateData(RowParser parser, Lumina.GameData gameData, Language language)
	{
		base.PopulateData(parser, gameData, language);

		this.Name = parser.ReadString(0) ?? string.Empty;
	}
}
