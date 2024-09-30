namespace StudioFourteen.GameData.Excel;

using Dalamud.Game.ClientState.Objects.Enums;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using Lumina;
using Lumina.Data;
using Lumina.Excel;

[Sheet("BNpcCustomize", 0x18f060d4)]
public class BattleNpcCustomize : StudioExcelRow
{
	public CustomizeData Customize { get; private set; }

	public override void PopulateData(RowParser parser, GameData gameData, Language language)
	{
		base.PopulateData(parser, gameData, language);

		CustomizeData c = default;

		for (int i = 0; i < CustomizeDataExtensions.NumOptions; i++)
		{
			CustomizeIndex index = (CustomizeIndex)i;
			byte val = parser.ReadColumn<byte>(i);

			c.SetValue(index, val);
		}

		this.Customize = c;
	}
}