namespace StudioFourteen.GameData.Library;

using Lumina.Excel.Sheets;
using Lumina.Text.ReadOnly;
using StudioFourteen.Library.Sources;

public class TerritoryTypeLibraryEntry(SourceBase source, TerritoryType territory)
	: ExcelLibraryEntry(source, territory.RowId)
{
	public TerritoryType Excel => territory;

	public override string? Name => territory.Name.GetString();
}
