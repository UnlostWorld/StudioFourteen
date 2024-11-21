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

namespace StudioFourteen.Library.Filters;

using WpfUtils;

public class SearchQueryFilter : FilterBase
{
	public string[]? Query;

	private string? search;

	public string? Search
	{
		get => this.search;
		set
		{
			this.search = value;

			if (string.IsNullOrWhiteSpace(value))
			{
				this.Query = null;
			}
			else
			{
				this.Query = SearchUtility.ToQuery(value);
			}
		}
	}

	public override bool IsEmpty => this.Query == null;

	public override void Clear()
	{
		this.Query = null;
		this.search = null;
	}

	public override bool Filter(LibraryEntryBase entry)
	{
		if(this.Query == null)
			return true;

		return entry.Search(this.Query);
	}
}
