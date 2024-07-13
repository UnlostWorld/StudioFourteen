namespace ScreenshotStudio.GameData.Excel;

using ScreenshotStudio.Library;
using ScreenshotStudio.Library.Sources;
using ScreenshotStudio.Tags;
using WpfUtils;

public abstract class LibraryExcelRow : StudioExcelRow, IEntryBase
{
	public TagCollection Tags { get; init; } = new();
	public string? Name { get; set; }
	public bool IsVisible { get; set; }
	public SourceBase? Source { get; set; }
	public string? SourceInfo { get; set; }
	public string Identifier => $"{this.GetType().Name} #{this.RowId}";

	public virtual bool IsValid => true;

	public void Dispose()
	{
	}

	public bool Search(TagCollection tags, string[]? query)
	{
		if (!this.Tags.Matches(tags))
			return false;

		if (!this.Search(query))
			return false;

		return true;
	}

	public virtual bool Search(string[]? query)
	{
		bool result = false;
		result |= SearchUtility.Matches(this.RowId, query);
		result |= SearchUtility.Matches(this.Name, query);
		return result;
	}
}