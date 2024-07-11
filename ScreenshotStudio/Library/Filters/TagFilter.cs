namespace ScreenshotStudio.Library.Filters;

using ScreenshotStudio.Tags;

internal class TagFilter : FilterBase
{
    public TagCollection? Tags;

    public override void Clear()
    {
        this.Tags = null;
    }

    public void Add(Tag tag)
    {
        if(this.Tags == null)
            this.Tags = new();

        this.Tags.Add(tag);
    }

    public override bool Filter(IEntryBase entry)
    {
        if(this.Tags == null)
            return true;

        if(entry.Tags == null)
            return false;

        if(entry.Tags.Matches(this.Tags))
            return true;

        return false;
    }
}
