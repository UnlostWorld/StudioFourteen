// © XivTools.
// Licensed under the MIT license.

namespace ScreenshotStudio.GameData.Excel;

using Lumina.Data;
using Lumina.Excel;

[Sheet("Companion", 0x776048c3)]
public class Companion : LibraryExcelRow
{
	public string? Name { get; protected set; }
	public ModelChara? ModelChara { get; protected set; }
	public byte Scale { get; protected set; }
	public ImageReference? Icon { get; protected set; }

	public override void PopulateData(RowParser parser, Lumina.GameData gameData, Language language)
	{
		base.PopulateData(parser, gameData, language);

		this.Name = parser.ReadString(0);
		this.ModelChara = parser.ReadRowReference<ushort, ModelChara>(8);
		this.Scale = parser.ReadColumn<byte>(9);
		this.Icon = parser.ReadImageReference<ushort>(26);
	}
}