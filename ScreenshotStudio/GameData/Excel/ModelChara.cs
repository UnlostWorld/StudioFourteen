namespace ScreenshotStudio.GameData.Excel;

using Lumina.Data;
using Lumina.Excel;

[Sheet("ModelChara", 0x8d35f5ed)]
public partial class ModelChara : StudioExcelRow
{
	public byte Type { get; protected set; }
	public ushort Model { get; protected set; }
	public byte Base { get; protected set; }
	public byte Variant { get; protected set; }

	public override void PopulateData(RowParser parser, Lumina.GameData gameData, Language language)
	{
		base.PopulateData(parser, gameData, language);

		this.Type = parser.ReadColumn<byte>(0);
		this.Model = parser.ReadColumn<ushort>(1);
		this.Base = parser.ReadColumn<byte>(2);
		this.Variant = parser.ReadColumn<byte>(3);
	}
}