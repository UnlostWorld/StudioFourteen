namespace ScreenshotStudio.GameData.Excel;

using Lumina.Data;
using Lumina.Excel;
using ScreenshotStudio.Library;
using ScreenshotStudio.Structs;
using WpfUtils;

[Sheet("ENpcResident", 0xf74fa88c)]
public class ResidentNpc : LibraryExcelRow, IActorAppearance
{
	public string? Description { get; protected set; }
	public EventNpc? EventNpc { get; protected set; }

	// don't show duplicates in the library
	public override bool IsValid => base.IsValid && this.EventNpc?.DuplicateRow == null;

	public override void PopulateData(RowParser parser, Lumina.GameData gameData, Language language)
	{
		base.PopulateData(parser, gameData, language);

		this.Name = parser.ReadString(0);
		this.Description = parser.ReadString(8);

		this.EventNpc = GameDataService.GetRow<EventNpc>(this.RowId);

		this.Tags.Add("NPC");

		if (this.Name != null)
		{
			this.Tags.Add("Named");

			if (this.EventNpc != null)
			{
				this.EventNpc.Name = this.Name;
				this.EventNpc.Tags.Add("Named");
			}
		}

		if (!string.IsNullOrEmpty(this.Description))
		{
			this.Tags.Add("Described");
		}

		if (this.EventNpc != null)
		{
			this.Tags.Add(this.EventNpc.Tags);
		}
	}

	public override bool Search(string[]? query)
	{
		bool matches = base.Search(query);
		matches |= SearchUtility.Matches(this.Description, query);

		if (this.EventNpc != null)
			matches |= this.EventNpc.Search(query);

		return matches;
	}

	public unsafe void Apply(Actor* actor)
	{
		this.EventNpc?.Apply(actor);
	}
}
