namespace StudioFourteen.GameData.Excel;

using System;
using Lumina.Data;
using Lumina.Excel;
using Lumina.Text;
using StudioFourteen.GameData.Sheets;
using WpfUtils;
using MediaColor = System.Windows.Media.Color;

[Sheet("Stain", 0x97c471bd)]
public class Stain : LibraryExcelRow
{
	public byte Id { get; protected set; }
	public byte Shade { get; protected set; }
	public MediaColor? Color { get; protected set; }
	public Item? Item { get; protected set; } = null;

	public override void PopulateData(RowParser parser, Lumina.GameData gameData, Language language)
	{
		base.PopulateData(parser, gameData, language);

		byte[] colorBytes = BitConverter.GetBytes(parser.ReadColumn<uint>(0));
		this.Color = MediaColor.FromRgb(colorBytes[2], colorBytes[1], colorBytes[0]);

		this.Shade = parser.ReadColumn<byte>(1);
		this.Name = parser.ReadColumn<SeString>(3) ?? string.Empty;
		this.Id = (byte)this.RowId;

		if (this.RowId == 0)
			return;

		uint itemKey = StainsSheet.StainToItemRow(this.RowId);

		if (itemKey != 0)
		{
			this.Item = GameDataService.GetRow<Item>(itemKey);
		}

		if (!string.IsNullOrEmpty(this.Name))
		{
			this.Tags.Add("Named");
		}
	}

	public override bool Search(string[]? query)
	{
		bool result = base.Search(query);
		result |= SearchUtility.Matches(this.Name, query);
		result |= SearchUtility.Matches(this.Shade, query);
		return result;
	}
}
