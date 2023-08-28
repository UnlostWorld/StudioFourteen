// © XivTools.
// Licensed under the MIT license.

namespace ScreenshotStudio.GameData.Excel;

using ScreenshotStudio.Library;
using ScreenshotStudio.Tags;
using XivToolsWpf;

public class LibraryExcelRow : StudioExcelRow, ILibraryItem
{
	public TagCollection Tags { get; init; } = new();

	public bool Search(TagCollection tags)
	{
		if (!this.Tags.Matches(tags))
			return false;

		if (!this.Search(tags.Query))
			return false;

		return true;
	}

	public virtual bool Search(string[]? query) => SearchUtility.Matches(this.RowId, query);
}
