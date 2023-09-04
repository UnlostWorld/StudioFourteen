// © XivTools.
// Licensed under the MIT license.

namespace ScreenshotStudio.GameData.Sheets;

using ScreenshotStudio.GameData.Excel;
using System.Collections;

public class TerritoryTypeSheet : DataSheet<Territory>
{
	public override IEnumerator GetEnumerator()
	{
		if (this.Sheet != null)
		{
			foreach (Territory t in this.Sheet)
			{
				if (t != null && t.Background != null)
				{
					yield return t;
				}
			}
		}
	}
}
