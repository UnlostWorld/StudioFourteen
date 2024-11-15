namespace StudioFourteen.GameData.Library;

using Lumina.Excel.Sheets;
using Lumina.Text.ReadOnly;
using StudioFourteen.Library.Sources;
using System.Collections.Generic;

public class TribeLibraryEntry : ExcelLibraryEntry
{
	public readonly Tribe Tribe;

	public TribeLibraryEntry(SourceBase source, Tribe tribe)
		: base(source, tribe.RowId)
	{
		this.Tribe = tribe;

		this.ModelTypes = new();

		if (this.RowId > 0)
		{
			foreach (ModelTypes modelType in tribe.GetModelTypes())
			{
				this.ModelTypes.Add(modelType);
			}
		}

		this.Log.Information($">> {this.Name} {this.ModelTypes.Count}");
	}

	public override string? Name => this.Tribe.Feminine.GetString() ?? this.Tribe.Masculine.GetString();

	public List<ModelTypes> ModelTypes { get; init; }
}
