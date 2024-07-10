namespace ScreenshotStudio.Library.Sources;

public abstract class SourceBase : GroupEntryBase
{
    public SourceBase()
        : base(null)
    {
    }

    public abstract string? Description { get; }
    public abstract void Scan();
}
