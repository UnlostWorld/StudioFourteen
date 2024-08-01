namespace ScreenshotStudio.GameData.Excel;

using ScreenshotStudio.Library;
using ScreenshotStudio.Library.Executors;
using ScreenshotStudio.Library.Sources;
using ScreenshotStudio.Tags;
using WpfUtils;

public abstract class LibraryExcelRow : StudioExcelRow, ILibraryEntry
{
	public event EntryEvent? ExecuteRequested;

	public TagCollection Tags { get; init; } = new();
	public string? Name { get; set; }
	public bool IsVisible { get; set; }
	public SourceBase? Source { get; set; }
	public string? SourceInfo { get; set; }
	public string Identifier => $"{this.GetType().Name} #{this.RowId}";

	public void Dispose()
	{
	}

	public void Execute()
	{
		this.ExecuteRequested?.Invoke();
	}

	public virtual bool Search(string[]? query)
	{
		bool result = false;
		result |= SearchUtility.Matches(this.RowId, query);
		result |= SearchUtility.Matches(this.Name, query);
		return result;
	}

	public virtual EntryExecutor? GetExecutor()
	{
		return null;
	}
}