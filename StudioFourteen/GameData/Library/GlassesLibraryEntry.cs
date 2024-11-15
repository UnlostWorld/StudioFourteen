namespace StudioFourteen.GameData.Library;

using Lumina.Excel.Sheets;
using Lumina.Text.ReadOnly;
using StudioFourteen.Library.Sources;

public class GlassesLibraryEntry(SourceBase source, Glasses glasses)
	: ExcelLibraryEntry(source, glasses.RowId)
{
	public override string? Name => glasses.Singular.GetString();
	public string? Description => glasses.Description.GetString();
	public ImageReference? Icon => new ImageReference(glasses.Icon);
}
