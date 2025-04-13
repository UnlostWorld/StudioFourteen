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

using System;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.Interop;

public class GameObjectsService : ServiceBase
{
	public unsafe GameObject* Get(int objectTableIndex)
	{
		TickService.VerifyGameTickThread();

		Span<Pointer<GameObject>> indexSorted = GameObjectManager.Instance()->Objects.IndexSorted;

		if (objectTableIndex < 0)
			return null;

		if (objectTableIndex >= indexSorted.Length)
			return null;

		return indexSorted[objectTableIndex];
	}

	public unsafe int GetTableLength()
	{
		TickService.VerifyGameTickThread();

		Span<Pointer<GameObject>> indexSorted = GameObjectManager.Instance()->Objects.IndexSorted;
		return indexSorted.Length;
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
