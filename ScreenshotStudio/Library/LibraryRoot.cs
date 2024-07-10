namespace ScreenshotStudio.Library;

internal class LibraryRoot : GroupEntryBase
{
    public LibraryRoot()
        : base(null)
    {
    }

    public override string Name => "Library";
    ////public override IDalamudTextureWrap? Icon => null;

    protected override string GetInternalId()
    {
        return "Root";
    }
}
