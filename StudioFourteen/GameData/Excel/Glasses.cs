namespace StudioFourteen.GameData.Excel;

using Lumina.Data;
using Lumina.Excel;

[Sheet("Glasses", 0x2faac2c1)]
public class Glasses : LibraryExcelRow
{
	public ImageReference? Icon { get; protected set; }
	public string? Description { get; protected set; }

	public override void PopulateData(RowParser parser, Lumina.GameData gameData, Language language)
	{
		base.PopulateData(parser, gameData, language);

		this.Icon = parser.ReadImageReference<int>(2);
		this.Description = parser.ReadString(12);
		this.Name = parser.ReadString(13);
	}
}
