// © XivTools.
// Licensed under the MIT license.

namespace ScreenshotStudio.GameData.Excel;

using Lumina.Data;
using Lumina.Excel;

[Sheet("BNpcBase", 0xe136dda3)]
public class BattleNpc : ExcelRow
{
	public string Name { get; protected set; } = string.Empty;
	public byte Battalion { get; protected set; }
	public byte LinkRace { get; protected set; }
	public byte Rank { get; protected set; }
	public float Scale { get; protected set; } = 1.0f;
	public ModelChara? ModelChara { get; protected set; }
	public BattleNpcCustomize? Customize { get; protected set; }
	public NpcEquip? Equipment { get; protected set; }

	public override void PopulateData(RowParser parser, Lumina.GameData gameData, Language language)
	{
		base.PopulateData(parser, gameData, language);

		this.Name = $"Battle NPC #{this.RowId}";

		this.Battalion = parser.ReadColumn<byte>(1);
		this.LinkRace = parser.ReadColumn<byte>(2);
		this.Rank = parser.ReadColumn<byte>(3);
		this.Scale = parser.ReadColumn<float>(4);
		this.ModelChara = parser.ReadRowReference<ushort, ModelChara>(5);
		this.Customize = parser.ReadRowReference<ushort, BattleNpcCustomize>(6);
		this.Equipment = parser.ReadRowReference<ushort, NpcEquip>(7);
	}
}
