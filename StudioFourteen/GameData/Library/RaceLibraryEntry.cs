namespace StudioFourteen.GameData.Library;

using Lumina.Excel.Sheets;
using Lumina.Text.ReadOnly;
using StudioFourteen.Library.Sources;

public class RaceLibraryEntry : ExcelLibraryEntry
{
	public readonly Race Race;
	public RaceLibraryEntry(SourceBase source, Race race)
		: base(source, race.RowId)
	{
		this.Race = race;

		if (this.Name != null)
		{
			this.Tags.Add("Named");
		}
	}

	public override string? Name => this.Race.Feminine.GetString() ?? this.Race.Masculine.GetString();
}
