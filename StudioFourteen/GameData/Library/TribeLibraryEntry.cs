namespace StudioFourteen.GameData.Library;

using Lumina.Excel.Sheets;
using Lumina.Text.ReadOnly;
using StudioFourteen.Library.Sources;

public class TribeLibraryEntry(SourceBase source, Tribe tribe)
	: ExcelLibraryEntry(source, tribe.RowId)
{
	public Tribe Tribe => tribe;

	public override string? Name => tribe.Feminine.GetString() ?? tribe.Masculine.GetString();
}
