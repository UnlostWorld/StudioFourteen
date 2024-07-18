namespace ScreenshotStudio.GameData.Excel;

using ScreenshotStudio.Library;
using ScreenshotStudio.Library.Sources;
using ScreenshotStudio.Tags;
using System;
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

	[Obsolete]
	public double Search(TagCollection tags, string[]? query)
	{
		if (!this.Tags.Matches(tags))
			return 0;

		return this.Search(query);
	}

	public virtual double Search(string[]? query)
	{
		double result = 0;
		result += SearchUtility.Search(this.RowId, query);
		result += SearchUtility.Search(this.Name, query);
		return result;
	}
}