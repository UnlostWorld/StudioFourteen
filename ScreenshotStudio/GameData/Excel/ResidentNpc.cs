// © XivTools.
// Licensed under the MIT license.

namespace ScreenshotStudio.GameData.Excel;

using Lumina.Data;
using Lumina.Excel;

[Sheet("ENpcResident", 0xf74fa88c)]
public class ResidentNpc : LibraryExcelRow
{
	private EventNpc? eventNpc;

	public string? Name { get; protected set; }
	public string? Description { get; protected set; }

	public override void PopulateData(RowParser parser, Lumina.GameData gameData, Language language)
	{
		base.PopulateData(parser, gameData, language);

		this.Name = parser.ReadString(0);
		this.Description = parser.ReadString(8) ?? string.Empty;

		this.eventNpc = GameDataService.GetRow<EventNpc>(this.RowId);
	}

	public EventNpc? GetEventNpc()
	{
		if (this.eventNpc == null)
			this.eventNpc = GameDataService.GetRow<EventNpc>(this.RowId);

		return this.eventNpc;
	}
}
