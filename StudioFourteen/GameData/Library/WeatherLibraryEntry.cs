namespace StudioFourteen.GameData.Library;

using Lumina.Excel.Sheets;
using Lumina.Text.ReadOnly;
using StudioFourteen.Library.Sources;

public class WeatherLibraryEntry(SourceBase source, Weather weather)
	: ExcelLibraryEntry(source, weather.RowId)
{
	public Weather Excel => weather;

	public override string? Name => weather.Name.GetString();
	public string? Description => null;
	public ImageReference? Icon => new ImageReference(weather.Icon);
}
