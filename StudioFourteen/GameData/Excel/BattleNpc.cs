namespace StudioFourteen.GameData.Excel;

using FFXIVClientStructs.FFXIV.Client.Game.Character;
using Lumina.Data;
using Lumina.Excel;
using StudioFourteen.Data;

[Sheet("BNpcBase", 0x86278126)]
public class BattleNpc : NpcBase
{
	public byte Battalion { get; protected set; }
	public byte LinkRace { get; protected set; }
	public byte Rank { get; protected set; }

	public override string Key => $"B:{this.RowId.ToString(DataService.NpcNamesIdFormat)}";

	public unsafe override void PopulateData(RowParser parser, Lumina.GameData gameData, Language language)
	{
		base.PopulateData(parser, gameData, language);

		this.SetName();

		this.Battalion = parser.ReadColumn<byte>(1);
		this.LinkRace = parser.ReadColumn<byte>(2);
		this.Rank = parser.ReadColumn<byte>(3);
		this.Scale = parser.ReadColumn<float>(4);
		this.ModelChara = parser.ReadRowReference<ushort, ModelChara>(5);

		BattleNpcCustomize? battleCustomize = parser.ReadRowReference<ushort, BattleNpcCustomize>(6);
		if (battleCustomize != null)
			this.Customize = battleCustomize.Customize;

		NpcEquip? npcEquip = parser.ReadRowReference<ushort, NpcEquip>(7);
		if (npcEquip != null)
			this.Equipment = npcEquip.Equipment;

		this.Icon = this.Customize.GetIcon();

		this.GenerateTags();
		this.GenerateAppearanceHash();
	}
}
