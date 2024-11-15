namespace StudioFourteen.GameData.Library;

using Lumina.Excel.Sheets;
using Lumina.Text.ReadOnly;
using StudioFourteen.Library.Sources;

public class OrnamentLibraryEntry(SourceBase source, Ornament ornament)
	: ExcelLibraryEntry(source, ornament.RowId)
{
	public override string? Name => ornament.Singular.GetString();
	public ImageReference? Icon => new ImageReference(ornament.Icon);
}
