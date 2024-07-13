namespace ScreenshotStudio.GameData.Excel;

using Dalamud.Game.ClientState.Objects.Enums;
using Lumina;
using Lumina.Data;
using Lumina.Excel;
using ScreenshotStudio.Structs;

[Sheet("BNpcCustomize", 0x18f060d4)]
public class BattleNpcCustomize : StudioExcelRow
{
	public Customize Customize { get; private set; }

	public override void PopulateData(RowParser parser, GameData gameData, Language language)
	{
		base.PopulateData(parser, gameData, language);

		Customize c;

		for (int i = 0; i < Customize.NumOptions; i++)
		{
			CustomizeIndex index = (CustomizeIndex)i;
			byte val = parser.ReadColumn<byte>(i);

			c.SetValue(index, val);
		}

		this.Customize = c;
	}
}