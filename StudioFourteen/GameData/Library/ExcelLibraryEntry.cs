// .                    @@             _____ _______ _    _ _____ _____ ____
//          @       @@@@@             / ____|__   __| |  | |  __ \_   _/ __ \
//         @@@  @@@@                 | (___    | |  | |  | | |  | || || |  | |
//         @@@@@@@@@  @    @          \___ \   | |  | |  | | |  | || || |  | |
//        @@@@       @@@@@@@          ____) |  | |  | |__| | |__| || || |__| |
//    @@@@@             @@@          |_____/   |_|   \____/|_____/_____\____/
//     @@@      @@@      @@        ___     _    _   _  __   _____  ___  ___  _  _
//      @@    @@@@@@@    @@       |  _|  / _ \ | | | || _ \|_   _|| __|| __|| \| |
//      @@    @@@@@@@    @   @    | __| | (_) || |_| ||   /  | |  | _| | _| | .` |
//    @@@@      @@@      @@@@     |_|    \___/  \___/ |_|_\  |_|  |___||___||_|\_|
//     @@@@             @@@        https://github.com/UnlostWorld/StudioFourteen
//       @@@@@      @@@@@
//        @@@@@@@@@@@@@@                This software is licensed under the
//            @@@@  @                  GNU AFFERO GENERAL PUBLIC LICENSE v3

namespace StudioFourteen.GameData.Library;

using System;
using StudioFourteen.Library;
using StudioFourteen.Library.Sources;
using WpfUtils;

public abstract class ExcelLibraryEntry(SourceBase source, uint rowId)
	: LibraryEntryBase(source)
{
	public uint RowId => rowId;

	public override string? SubTitle => $"#{rowId}";
	public override IComparable DefaultSortValue => this.RowId;

	public override string ToString() => $"#{rowId}";

	public override bool Search(string[] query)
	{
		if (query.Length == 1 && query[0].StartsWith("#"))
		{
			string idStr = query[0].Substring(1);
			return idStr == rowId.ToString();
		}

		if (SearchUtility.Matches(rowId, query))
			return true;

		if (base.Search(query))
			return true;

		return false;
	}

	protected override string GetInternalId() => $"{this.GetType().Name}_{rowId}";
}
