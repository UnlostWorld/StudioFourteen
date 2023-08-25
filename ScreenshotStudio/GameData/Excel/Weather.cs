// © XivTools.
// Licensed under the MIT license.

namespace ScreenshotStudio.GameData.Excel;

using Lumina.Data;
using Lumina.Excel;

[Sheet("Weather", 0x02cf2541)]
public class Weather : ExcelRow
{
	public string Name { get; private set; } = string.Empty;
	public string Description { get; private set; } = string.Empty;
	public ushort WeatherId => (ushort)this.RowId;

	public ImageReference? Icon { get; private set; }

	public override void PopulateData(RowParser parser, Lumina.GameData gameData, Language language)
	{
		base.PopulateData(parser, gameData, language);
		this.Icon = parser.ReadImageReference<int>(0);
		this.Name = parser.ReadString(1) ?? $"Weather #{this.RowId}";
		this.Description = parser.ReadString(2) ?? string.Empty;
	}
}
