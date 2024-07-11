namespace ScreenshotStudio.Library.Filters;

internal class SearchQueryFilter : FilterBase
{
    public string[]? Query;

    public override void Clear()
    {
        this.Query = null;
    }

    public override bool Filter(IEntryBase entry)
    {
        if(this.Query == null)
            return false;

        return entry.Search(this.Query);
    }
}
