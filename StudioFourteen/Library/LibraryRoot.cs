namespace StudioFourteen.Library;

internal class LibraryRoot : GroupEntryBase
{
	public LibraryRoot()
		: base(null)
	{
	}

	public override string Name => "Library";
	public bool IsRoot => true;

	protected override string GetInternalId()
	{
		return "Root";
	}
}
