namespace StudioFourteen.Library;

using FontAwesome.Sharp;

internal class LibraryRoot()
	: GroupEntryBase(null)
{
	public override IconChar Icon => IconChar.Book;
	public override string Name => "Library";
	protected override string GetInternalId() => "Root";
}
