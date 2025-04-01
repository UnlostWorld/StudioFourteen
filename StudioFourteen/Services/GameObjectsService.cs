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

using FFXIVClientStructs.FFXIV.Client.Game.Object;

public class GameObjectsService : ServiceBase
{
	public unsafe GameObject* Get(int objectTableIndex)
	{
		TickService.VerifyGameTickThread();
		return GameObjectManager.Instance()->Objects.IndexSorted[objectTableIndex];
	}

	public unsafe GameObject* Get(GameObjectId objectId)
	{
		TickService.VerifyGameTickThread();
		return GameObjectManager.Instance()->Objects.GetObjectByGameObjectId(objectId);
	}

	public unsafe GameObject* Get(uint entityId)
	{
		TickService.VerifyGameTickThread();
		return GameObjectManager.Instance()->Objects.GetObjectByEntityId(entityId);
	}

	public unsafe T* Get<T>(int objectTableIndex)
		where T : unmanaged
	{
		return (T*)this.Get(objectTableIndex);
	}

	public unsafe T* Get<T>(GameObjectId objectId)
		where T : unmanaged
	{
		return (T*)this.Get(objectId);
	}

	public unsafe T* Get<T>(uint entityId)
		where T : unmanaged
	{
		return (T*)this.Get(entityId);
	}
}
