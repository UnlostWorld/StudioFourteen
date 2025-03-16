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

namespace StudioFourteen.Posing.Body;

using System.Threading.Tasks;
using Dalamud.Game.ClientState.Objects.Enums;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using StudioFourteen.GameData;
using StudioFourteen.Utilities;
using WpfUtils;

public class BodySimpleView : SimpleView
{
	protected override async Task UpdateTargetsAsync()
	{
		await Threads.FrameworkThread();

		string key;

		unsafe
		{
			Character* pCharacter = this.Services.Target.GetCharacter(this.ObjectTableIndex);
			if (pCharacter == null)
				return;

			byte tribe = pCharacter->GetCustomizeValue(CustomizeIndex.Tribe);
			byte gender = pCharacter->GetCustomizeValue(CustomizeIndex.Gender);
			byte vieraEars = pCharacter->GetCustomizeValue(CustomizeIndex.RaceFeatureType);

			key = $"Body_{tribe}_{gender}";
			/*if (tribe == 15 || tribe == 16)
			{
				key = $"Body_{tribe}_{gender}_{vieraEars}";
			}*/
		}

		await this.MainThread();

		if (this.Services.Data.SimplePoseLayouts?.ContainsKey(key) != true)
			return;

		this.LayoutName = key;
		await base.UpdateTargetsAsync();
	}
}