namespace StudioFourteen.GameData.Excel;

using Lumina.Data;
using Lumina.Excel;

[Sheet("Ornament", 0x3d312c8f)]
public class Ornament : LibraryExcelRow
{
	public ModelChara? ModelChara { get; protected set; }
	public byte AttachPoint { get; protected set; }
	public ImageReference? Icon { get; private set; }

	public override bool IsValid => base.IsValid && this.RowId > 0 && this.ModelChara?.IsValid == true;

	public override void PopulateData(RowParser parser, Lumina.GameData gameData, Language language)
	{
		base.PopulateData(parser, gameData, language);

		this.ModelChara = parser.ReadRowReference<ushort, ModelChara>(0);
		this.AttachPoint = parser.ReadColumn<byte>(1);
		this.Icon = parser.ReadImageReference<ushort>(6);
		this.Name = parser.ReadString(8);
	}
}
