namespace ScreenshotStudio.Library.Filters;

public abstract class FilterBase
{
    public readonly string Name;

    public FilterBase(string name)
    {
        this.Name = name;
    }

    public abstract void Clear();
    public abstract bool Filter(EntryBase entry);
}
