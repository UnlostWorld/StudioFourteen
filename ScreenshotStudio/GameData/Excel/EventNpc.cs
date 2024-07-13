namespace ScreenshotStudio.GameData.Excel;

using Dalamud.Game.ClientState.Objects.Enums;
using Lumina.Data;
using Lumina.Excel;
using ScreenshotStudio.Structs;
using ScreenshotStudio.Data;

[Sheet("ENpcBase", 0x464052cd)]
public class EventNpc : NpcBase
{
	public NpcEquip? NpcEquip { get; protected set; }

	public override NpcEquipment? BackupEquipment => this.NpcEquip?.Equipment;
	public override string Key => $"E:{this.RowId.ToString(DataService.NpcNamesIdFormat)}";

	public override void PopulateData(RowParser parser, Lumina.GameData gameData, Language language)
	{
		base.PopulateData(parser, gameData, language);

		this.SetName();

		// Customize
		this.Scale = parser.ReadColumn<float>(34);
		this.ModelChara = parser.ReadRowReference<ushort, ModelChara>(35);

		Customize c;

		for (int i = 0; i < Customize.NumOptions; i++)
		{
			CustomizeIndex index = (CustomizeIndex)i;
			int row = 36 + i;
			byte val = parser.ReadColumn<byte>(row);

			c.SetValue(index, val);
		}

		this.Customize = c;
		this.Equipment.Parse(parser, 65);
		this.NpcEquip = parser.ReadRowReference<ushort, NpcEquip>(63);

		this.GenerateTags();
		this.GenerateAppearanceHash();
	}
}