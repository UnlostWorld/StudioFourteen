// © XivTools.
// Licensed under the MIT license.

namespace ScreenshotStudio.GameData.Excel;

using Lumina.Data;
using Lumina.Excel;

[Sheet("Lobby", columnHash: 0x54075F2E)]
public class Lobby : StudioExcelRow
{
	public string? Text { get; set; }
	public string? Unknown4 { get; set; }
	public string? Unknown5 { get; set; }

	public override void PopulateData(RowParser parser, Lumina.GameData gameData, Language language)
	{
		base.PopulateData(parser, gameData, language);
		////TYPE = parser.ReadColumn<uint>(0);
		////PARAM = parser.ReadColumn<uint>(1);
		////LINK = parser.ReadColumn<uint>(2);
		this.Text = parser.ReadString(3);
		this.Unknown4 = parser.ReadString(4);
		this.Unknown5 = parser.ReadString(5);
	}
}