// .                      @@             _____ _______ _    _ _____ _____ ____
//            @       @@@@@             / ____|__   __| |  | |  __ \_   _/ __ \
//           @@@  @@@@                 | (___    | |  | |  | | |  | || || |  | |
//           @@@@@@@@@  @    @          \___ \   | |  | |  | | |  | || || |  | |
//          @@@@       @@@@@@@          ____) |  | |  | |__| | |__| || || |__| |
//      @@@@@             @@@          |_____/   |_|   \____/|_____/_____\____/
//       @@@      @@@      @@        ___     _    _   _  __   _____  ___  ___  _  _
//        @@    @@@@@@@    @@       |  _|  / _ \ | | | || _ \|_   _|| __|| __|| \| |
//        @@    @@@@@@@    @   @    | __| | (_) || |_| ||   /  | |  | _| | _| | .` |
//      @@@@      @@@      @@@@     |_|    \___/  \___/ |_|_\  |_|  |___||___||_|\_|
//       @@@@             @@@
//         @@@@@      @@@@@               This software is licensed under the
//          @@@@@@@@@@@@@@                 GNU AFFERO GENERAL PUBLIC LICENSE
//              @@@@  @                       Version 3, 19 November 2007

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
