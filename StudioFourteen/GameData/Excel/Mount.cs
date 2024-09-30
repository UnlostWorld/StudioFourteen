namespace StudioFourteen.GameData.Excel;

using Lumina.Data;
using Lumina.Excel;

using static FFXIVClientStructs.FFXIV.Client.Game.Character.DrawDataContainer;

[Sheet("Mount", 0x304b5115)]
public class Mount : LibraryExcelRow
{
	public ModelChara? ModelChara { get; protected set; }
	public MountCustomize? MountCustomize { get; protected set; }
	public Item? Head { get; protected set; }
	public Item? Body { get; protected set; }
	public Item? Legs { get; protected set; }
	public Item? Feet { get; protected set; }
	public ImageReference? Icon { get; private set; }

	public override void PopulateData(RowParser parser, Lumina.GameData gameData, Language language)
	{
		base.PopulateData(parser, gameData, language);

		this.Name = parser.ReadString(0);
		this.ModelChara = parser.ReadRowReference<int, ModelChara>(8);
		this.MountCustomize = parser.ReadRowReference<byte, MountCustomize>(16);

		this.Head = GameDataService.BuddyEquips?.Find(EquipmentSlot.Head, parser.ReadColumn<int>(25));
		this.Body = GameDataService.BuddyEquips?.Find(EquipmentSlot.Head, parser.ReadColumn<int>(26));
		this.Legs = GameDataService.BuddyEquips?.Find(EquipmentSlot.Head, parser.ReadColumn<int>(27));
		this.Feet = GameDataService.BuddyEquips?.Find(EquipmentSlot.Head, parser.ReadColumn<int>(28));
		this.Icon = parser.ReadImageReference<ushort>(30);
	}
}
