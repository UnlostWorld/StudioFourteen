//// Brio
//// https://github.com/Etheirys/Brio/blob/main/Brio/Resources/Sheets/BrioCharaMakeType.cs

namespace StudioFourteen.GameData.Excel;

using Lumina.Data;
using Lumina.Excel;

[Sheet("HairMakeType")]
public class HairMakeType : StudioExcelRow
{
	public const int EntryCount = 100;

	public Race? Race { get; private set; }
	public Tribe? Tribe { get; private set; }
	public Genders Gender { get; private set; }

	public CharaMakeCustomize?[] HairStyles { get; init; } = new CharaMakeCustomize?[EntryCount];
	public CharaMakeCustomize?[] FacePaints { get; init; } = new CharaMakeCustomize?[EntryCount];

	public override void PopulateData(RowParser parser, Lumina.GameData gameData, Language language)
	{
		base.PopulateData(parser, gameData, language);

		this.Race = parser.ReadRowReference<int, Race>(0);
		this.Tribe = parser.ReadRowReference<int, Tribe>(1);
		this.Gender = (Genders)parser.ReadColumn<sbyte>(2);

		for (int i = 0; i < EntryCount; i++)
			this.HairStyles[i] = parser.ReadRowReference<uint, CharaMakeCustomize>(66 + (i * 9));

		for (int i = 0; i < EntryCount; i++)
			this.FacePaints[i] = parser.ReadRowReference<uint, CharaMakeCustomize>(73 + (i * 9));
	}
}
