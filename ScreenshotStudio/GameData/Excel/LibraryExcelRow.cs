// © XivTools.
// Licensed under the MIT license.

namespace ScreenshotStudio.GameData.Excel;

using Lumina.Excel;
using ScreenshotStudio.Library;
using ScreenshotStudio.Tags;

public class LibraryExcelRow : ExcelRow, ILibraryItem
{
	public TagCollection Tags { get; init; } = new();

	public bool Search(TagCollection tags)
	{
		throw new System.NotImplementedException();
	}
}
