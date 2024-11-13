namespace StudioFourteen.GameData.Library;

using Lumina.Excel.Sheets;
using StudioFourteen.Library.Sources;

public class ItemLibraryEntry(SourceBase source, Item item)
	: ExcelLibraryEntry(source, item.RowId)
{
	public ImageReference? Icon => new ImageReference(item.Icon);
	public string? Description => item.Description.ToString();

	public override string? Name
	{
		get
		{
			string text = item.Singular.ExtractText();
			if (string.IsNullOrEmpty(text))
				return null;

			return text;
		}
	}

	public override string ToString() => $"Item #{item.RowId}";
	protected override string GetInternalId() => $"Item_{item.RowId}";
}
