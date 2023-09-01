// © XivTools.
// Licensed under the MIT license.

namespace ScreenshotStudio.GameData.Excel;

using Lumina.Data;
using Lumina.Excel;
using ScreenshotStudio.Library;
using ScreenshotStudio.Structs;
using XivToolsWpf;

[Sheet("ENpcResident", 0xf74fa88c)]
public class ResidentNpc : LibraryExcelRow, IActorAppearance
{
	public string? Name { get; protected set; }
	public string? Description { get; protected set; }

	public EventNpc? EventNpc { get; protected set; }

	public override void PopulateData(RowParser parser, Lumina.GameData gameData, Language language)
	{
		base.PopulateData(parser, gameData, language);

		this.Name = parser.ReadString(0);
		this.Description = parser.ReadString(8);

		this.EventNpc = GameDataService.GetRow<EventNpc>(this.RowId);

		this.Tags.Add("NPC");
		this.Tags.Add("Resident");

		if (this.Name != null)
		{
			this.Tags.Add("Named");
		}
		else
		{
			this.Tags.Add("Unnamed");
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
		if (SearchUtility.Matches(this.Name, query))
			return true;

		if (SearchUtility.Matches(this.Description, query))
			return true;

		if (this.EventNpc?.Search(query) == true)
			return true;

		return base.Search(query);
	}

	public unsafe void Apply(Actor* actor)
	{
		this.EventNpc?.Apply(actor);
	}
}
