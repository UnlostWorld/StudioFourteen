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

namespace StudioFourteen.GameData.Library;

using Lumina.Excel.Sheets;
using Lumina.Text.ReadOnly;
using StudioFourteen.Library.Sources;
using System.Collections.Generic;

public class TribeLibraryEntry : ExcelLibraryEntry
{
	public readonly Tribe Tribe;

	public TribeLibraryEntry(SourceBase source, Tribe tribe)
		: base(source, tribe.RowId)
	{
		this.Tribe = tribe;

		this.ModelTypes = new();

		if (this.RowId > 0)
		{
			foreach (ModelTypes modelType in tribe.GetModelTypes())
			{
				this.ModelTypes.Add(modelType);
			}
		}
	}

	public override string? Name => this.Tribe.Feminine.GetString() ?? this.Tribe.Masculine.GetString();

	public List<ModelTypes> ModelTypes { get; init; }
}
