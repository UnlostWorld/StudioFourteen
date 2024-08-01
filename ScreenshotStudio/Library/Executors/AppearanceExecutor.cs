namespace ScreenshotStudio.Library.Executors;
using ScreenshotStudio.Library;

public class AppearanceExecutor : EntryExecutor
{
	private readonly ICharacterAppearance appearance;

	public AppearanceExecutor(ILibraryEntry entry, ICharacterAppearance appearance)
		: base(entry)
	{
		this.appearance = appearance;
	}
}