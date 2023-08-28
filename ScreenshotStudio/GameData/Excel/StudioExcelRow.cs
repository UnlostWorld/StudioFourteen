// © XivTools.
// Licensed under the MIT license.

namespace ScreenshotStudio.GameData.Excel;

using Lumina.Excel;

public class StudioExcelRow : ExcelRow
{
	public string RowName => $"{this.GetType().Name} #{this.RowId}";

	public override string ToString()
	{
		return this.RowName;
	}
}