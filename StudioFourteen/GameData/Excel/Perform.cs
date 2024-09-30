namespace StudioFourteen.GameData.Excel;

using Lumina.Data;
using Lumina.Excel;
using Lumina.Text;
using static FFXIVClientStructs.FFXIV.Client.Game.Character.DrawDataContainer;

[Sheet("Perform", 0x7bf81fa9)]
public class Perform : LibraryExcelRow
{
	public string Description { get; private set; } = string.Empty;

	public ushort ModelSet { get; private set; }
	public ushort ModelBase { get; private set; }
	public ushort ModelVariant { get; private set; }

	public ImageReference? Icon => null;
	public bool HasSubModel => false;
	public ushort SubModelSet => 0;
	public ushort SubModelBase => 0;
	public ushort SubModelVariant => 0;
	public byte EquipLevel => 0;

	public bool FitsInSlot(EquipmentSlot slot) => false;
	public bool FitsInSlot(WeaponSlot slot) => slot == WeaponSlot.MainHand;

	public override void PopulateData(RowParser parser, Lumina.GameData gameData, Language language)
	{
		base.PopulateData(parser, gameData, language);

		this.Name = parser.ReadColumn<SeString>(0) ?? string.Empty;

		this.Tags.Add("Performance");
		this.Tags.Add("MainHand");

		ulong mainModel = parser.ReadColumn<ulong>(2);
		this.ModelSet = (ushort)mainModel;
		this.ModelBase = (ushort)(mainModel >> 16);
		this.ModelVariant = (ushort)(mainModel >> 32);
		this.Name = parser.ReadColumn<SeString>(9) ?? string.Empty;
	}
}
