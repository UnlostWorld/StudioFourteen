namespace StudioFourteen.GameData.Sheets;

using StudioFourteen.GameData.Excel;
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
