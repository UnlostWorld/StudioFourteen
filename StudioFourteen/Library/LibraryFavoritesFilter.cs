namespace StudioFourteen.Library;

using StudioFourteen.Library.Filters;

internal class LibraryFavoritesFilter : FilterBase
{
	public override bool IsEmpty => false;

	public override void Clear()
	{
	}

	public override bool Filter(ILibraryEntry entry)
	{
		// TODO
		////if(Favorites.Contains(entry.Identifier))
		////    return true;

		return false;
	}
}
