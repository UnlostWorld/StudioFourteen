namespace ScreenshotStudio.Library.Executors;

public class FileEntryExecutor : EntryExecutor
{
	public FileEntryExecutor(ILibraryEntry entry)
		: base(entry)
	{
		this.Label = "Open the File!";
	}
}
