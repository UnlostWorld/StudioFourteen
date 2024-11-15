namespace StudioFourteen.Library;

using StudioFourteen.Library.Filters;

internal class LibraryFavoritesFilter : FilterBase
{
	public override bool IsEmpty => false;

	public static bool GetIsFavorite(LibraryEntryBase entry)
	{
		return ServiceManager.Instance.Settings.Current.Favorites.Contains(entry.Identifier);
	}

	public static void SetIsFavorite(LibraryEntryBase entry, bool favorite)
	{
		if (favorite)
		{
			ServiceManager.Instance.Settings.Current.Favorites.Add(entry.Identifier);
		}
		else
		{
			ServiceManager.Instance.Settings.Current.Favorites.Remove(entry.Identifier);
		}
	}

	public override void Clear()
	{
	}

	public override bool Filter(LibraryEntryBase entry)
	{
		return entry.IsFavorite;
	}
}
