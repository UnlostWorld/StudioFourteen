namespace StudioFourteen.GameData.Excel;

using Lumina.Data;
using Lumina.Excel;

[Sheet("CharaMakeCustomize", 0xc30e9b73)]
public class CharaMakeCustomize : StudioExcelRow
{
	public ImageReference? Icon { get; private set; }
	public Item? Item { get; private set; }
	public byte FeatureId { get; private set; }
	public bool IsPurchasable { get; private set; }
	public byte FaceType { get; private set; }

	public override void PopulateData(RowParser parser, Lumina.GameData gameData, Language language)
	{
		base.PopulateData(parser, gameData, language);
		this.FeatureId = parser.ReadColumn<byte>(0);
		this.Icon = parser.ReadImageReference<uint>(1);
		this.IsPurchasable = parser.ReadColumn<bool>(3);
		this.Item = parser.ReadRowReference<uint, Item>(5);
		this.FaceType = parser.ReadColumn<byte>(6);
	}
}
