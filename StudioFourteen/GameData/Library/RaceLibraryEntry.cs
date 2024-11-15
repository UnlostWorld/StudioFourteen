namespace StudioFourteen.GameData.Library;

using Lumina.Excel.Sheets;
using Lumina.Text.ReadOnly;
using StudioFourteen.Library.Sources;
using System.Collections.Generic;

public class RaceLibraryEntry : ExcelLibraryEntry
{
	public readonly Race Race;

	public RaceLibraryEntry(SourceBase source, Race race)
		: base(source, race.RowId)
	{
		this.Race = race;

		this.Tribes = new();
		this.Genders = new();

		if (this.RowId > 0)
		{
			ExcelSheetLibrarySource<TribeLibraryEntry>? tribeSource = ServiceManager.Instance.GameData.GetLibrarySource<TribeLibraryEntry>();
			if (tribeSource != null)
			{
				foreach (Tribe tribe in race.GetTribes())
				{
					TribeLibraryEntry? tribeEntry = tribeSource.GetRow(tribe.RowId);
					if (tribeEntry == null)
						continue;

					this.Tribes.Add(tribeEntry);
				}
			}

			// Every race gets every gender! wild.
			this.Genders.Add(StudioFourteen.GameData.Genders.Masculine);
			this.Genders.Add(StudioFourteen.GameData.Genders.Feminine);

			if (this.Name != null)
			{
				this.Tags.Add("Named");
			}
		}
	}

	public override string? Name => this.Race.Feminine.GetString() ?? this.Race.Masculine.GetString();

	public List<TribeLibraryEntry> Tribes { get; init; }
	public List<Genders> Genders { get; init; }

	public int GetTribeIndex(TribeLibraryEntry entry)
	{
		for (int i = 0; i < this.Tribes.Count; i++)
		{
			if (this.Tribes[i] == entry)
			{
				return i;
			}
		}

		return -1;
	}
}
