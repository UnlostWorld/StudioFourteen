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

namespace StudioFourteen.Scripting.Instance;

using System.Collections.Generic;
using StudioFourteen.GameData.Library;
using StudioFourteen.Library;
using StudioFourteen.Library.Sources;

public class LibraryInterface : ScriptServiceBase
{
	// Studio
	// TODO: split these up by file type.
	public IEnumerable<FileEntry> Files => this.Get<FileEntry>();

	// Game Data
	public IEnumerable<BNpcBaseLibraryEntry> BNpcBases => this.Get<BNpcBaseLibraryEntry>();
	public IEnumerable<CharaMakeCustomizeLibraryEntry> CharaMakeCustomize => this.Get<CharaMakeCustomizeLibraryEntry>();
	public IEnumerable<ENpcResidentLibraryEntry> ENpcResidents => this.Get<ENpcResidentLibraryEntry>();
	public IEnumerable<GlassesLibraryEntry> Glasses => this.Get<GlassesLibraryEntry>();
	public IEnumerable<ItemLibraryEntry> Items => this.Get<ItemLibraryEntry>();
	public IEnumerable<OrnamentLibraryEntry> Ornaments => this.Get<OrnamentLibraryEntry>();
	public IEnumerable<RaceLibraryEntry> Races => this.Get<RaceLibraryEntry>();
	public IEnumerable<StainLibraryEntry> Stains => this.Get<StainLibraryEntry>();
	public IEnumerable<TerritoryTypeLibraryEntry> TerritoryTypes => this.Get<TerritoryTypeLibraryEntry>();
	public IEnumerable<TribeLibraryEntry> Tribes => this.Get<TribeLibraryEntry>();
	public IEnumerable<WeatherLibraryEntry> Weathers => this.Get<WeatherLibraryEntry>();

	private List<T> Get<T>()
		where T : LibraryEntryBase
	{
		return ServiceManager.Instance.Library.GetAll<T>();
	}
}