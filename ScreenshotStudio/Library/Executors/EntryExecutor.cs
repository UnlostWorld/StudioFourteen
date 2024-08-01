namespace ScreenshotStudio.Library.Executors;

public abstract class EntryExecutor
{
	public ILibraryEntry Entry;

	public EntryExecutor(ILibraryEntry entry)
	{
		this.Entry = entry;
	}

	public string? Label { get; set; }
	public bool CanExecute { get; set; }
	public bool CanRevert { get; set; }

	public virtual void OnFrameworkUpdate()
	{
	}

	public virtual void Execute()
	{
	}

	public virtual void Revert()
	{
	}
}