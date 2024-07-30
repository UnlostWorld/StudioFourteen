namespace ScreenshotStudio.GameData.Excel;

using Lumina.Data;
using Lumina.Excel;
using ScreenshotStudio.Tags;

[Sheet("ModelChara", 0x25b9b3e2)]
public partial class ModelChara : StudioExcelRow
{
	public byte Type { get; protected set; }
	public ushort Model { get; protected set; }
	public byte Base { get; protected set; }
	public byte Variant { get; protected set; }

	public override bool IsValid => base.IsValid && this.RowId > 0;

	public override void PopulateData(RowParser parser, Lumina.GameData gameData, Language language)
	{
		base.PopulateData(parser, gameData, language);

		this.Type = parser.ReadColumn<byte>(0);
		this.Model = parser.ReadColumn<ushort>(1);
		this.Base = parser.ReadColumn<byte>(2);
		this.Variant = parser.ReadColumn<byte>(3);
	}

	public void GetTags(TagCollection tags)
	{
		if (this.Type == 1)
		{
			tags.Add("Humanoid");
		}
		else
		{
			tags.Add("Monster");
		}
	}
}