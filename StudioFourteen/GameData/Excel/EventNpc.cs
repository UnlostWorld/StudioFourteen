namespace StudioFourteen.GameData.Excel;

using Dalamud.Game.ClientState.Objects.Enums;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using Lumina.Data;
using Lumina.Excel;
using StudioFourteen.Data;
using System;

[Sheet("ENpcBase", 0x464052cd)]
public class EventNpc : NpcBase
{
	public NpcEquip? NpcEquip { get; protected set; }

	public override NpcEquipment? BackupEquipment => this.NpcEquip?.Equipment;

	public override void PopulateData(RowParser parser, Lumina.GameData gameData, Language language)
	{
		base.PopulateData(parser, gameData, language);

		// Customize
		this.Scale = parser.ReadColumn<float>(34);
		this.ModelChara = parser.ReadRowReference<ushort, ModelChara>(35);

		foreach(CustomizeIndex index in Enum.GetValues<CustomizeIndex>())
		{
			int row = 36 + (int)index;
			byte val = parser.ReadColumn<byte>(row);

			this.Customize.SetValue(index, val);
		}

		this.Equipment.Parse(parser, 65);
		this.NpcEquip = parser.ReadRowReference<ushort, NpcEquip>(63);
		this.Icon = this.Customize.GetIcon();

		this.GenerateTags();
		this.GenerateAppearanceHash();
	}
}