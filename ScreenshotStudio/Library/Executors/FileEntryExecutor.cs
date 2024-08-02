namespace ScreenshotStudio.Library.Executors;

public class FileEntryExecutor(ILibraryEntry entry)
	: EntryExecutor(entry)
{
	public override string? Label => "Open Me!";
	public override bool CanRevert => false;
	public override bool CanExecute => false;
}
