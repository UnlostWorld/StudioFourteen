namespace ScreenshotStudio.GameData.Sheets;

using ScreenshotStudio.GameData.Excel;
using System.Collections;

public class ResidentNpcSheet : DataSheet<ResidentNpc>
{
	// TODO: we may want to perform the deduplication during Initialize()
	// and try to flatten the Name properties together.
	// Since the same appearance can be used for multiple NPCs with
	// different names, eg: 'Estinien' and 'Estinien Wyrmblood' are
	// duplicates.

	// Skips resident NPCs
	public override IEnumerator GetEnumerator()
	{
		if (this.Sheet != null)
		{
			foreach (ResidentNpc resident in this.Sheet)
			{
				if (resident.EventNpc?.DuplicateRow != null)
					continue;

				yield return resident;
			}
		}
	}
}
