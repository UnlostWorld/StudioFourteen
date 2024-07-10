namespace ScreenshotStudio.GameData.Excel;

using Lumina.Data;
using Lumina.Excel;

[Sheet("ItemUICategory", 0xdc1f7844)]
public class ItemUICategory : LibraryExcelRow
{
	public ImageReference? Icon { get; private set; }

	public override void PopulateData(RowParser parser, Lumina.GameData gameData, Language language)
	{
		base.PopulateData(parser, gameData, language);

		this.Name = parser.ReadStringOffset(0);
		this.Icon = parser.ReadImageReferenceOffset<int>(4);
	}
}
