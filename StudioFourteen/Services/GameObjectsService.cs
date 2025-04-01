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

namespace StudioFourteen.Services;

using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.Object;

public class GameObjectsService : ServiceBase
{
	public unsafe GameObject* Get(int objectTableIndex)
	{
		TickService.VerifyGameTickThread();
		return GameObjectManager.Instance()->Objects.IndexSorted[objectTableIndex];
	}

	public unsafe Character* GetCharacter(int objectTableIndex)
	{
		TickService.VerifyGameTickThread();

		GameObject* pGameObject = this.Get(objectTableIndex);
		if (pGameObject == null)
			return null;

		Character* pCharacter = (Character*)pGameObject;
		if (pCharacter == null)
			return null;

		// why are we doing this here?
		if (pCharacter->ObjectKind == ObjectKind.Ornament
			|| pCharacter->ObjectKind == ObjectKind.Mount)
			return null;

		return pCharacter;
	}
}
