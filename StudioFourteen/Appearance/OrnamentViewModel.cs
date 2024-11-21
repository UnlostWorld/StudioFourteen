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

namespace StudioFourteen.Appearance;

using StudioFourteen.GameData.Library;
using StudioFourteen.Mvm;

public class OrnamentViewModel : ExcelRowItemViewModel<OrnamentLibraryEntry>
{
	public OrnamentViewModel()
	{
	}

	protected unsafe override ushort LiveValue
	{
		get
		{
			if (this.Target->OrnamentData.OrnamentObject == null)
				return 0;

			return this.Target->OrnamentData.OrnamentId;
		}
		set
		{
			// Can only set 0 (no ornament) while in group pose, setting to 0 outside
			// of group pose crashes the game.
			if (value == 0 && !this.Services.GroupPose.IsGroupPosing)
				return;

			this.Target->OrnamentData.SetupOrnament((short)value, 0);
		}
	}

	protected override string GetSearchTitle() => Resources.Find("LOC_Ornament", "Fashion Accessories");
}
