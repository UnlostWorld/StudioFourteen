namespace ScreenshotStudio.Library.Filters;

public abstract class FilterBase
{
    public abstract void Clear();
    public abstract bool Filter(IEntryBase entry);
}
