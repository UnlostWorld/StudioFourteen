namespace ScreenshotStudio.Library;

using ScreenshotStudio.Library.Filters;

internal class LibraryFavoritesFilter : FilterBase
{
    public LibraryFavoritesFilter()
        : base("Favorites")
    {
    }

    public override void Clear()
    {
    }

    public override bool Filter(EntryBase entry)
    {
        // TODO
        ////if(Favorites.Contains(entry.Identifier))
        ////    return true;

        return false;
    }
}
