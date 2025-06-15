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

namespace StudioFourteen.Scene.GameObjects;

using System;
using System.Collections.Generic;
using FFXIVClientStructs.Interop;
using StudioFourteen.Services;

using XivGameObject = FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject;
using XivGameObjectManager = FFXIVClientStructs.FFXIV.Client.Game.Object.GameObjectManager;

public class GameObjectService : ServiceBase
{
	private readonly Dictionary<ushort, GameObject> gameObjectLookup = new();

	public override void Attach()
	{
		this.Services.Tick.Add(TickService.Channels.GameTick, this.OnGameTick);
		base.Attach();
	}

	public override void Detach()
	{
		this.Services.Tick.Remove(TickService.Channels.GameTick, this.OnGameTick);
		base.Detach();
	}

	public GameObject? GetGameObject(int index)
	{
		if (index < 0)
			return null;

		if (index >= ushort.MaxValue)
			return null;

		lock (this.gameObjectLookup)
		{
			this.gameObjectLookup.TryGetValue((ushort)index, out GameObject? obj);
			return obj;
		}
	}

	public unsafe T* Get<T>(int objectTableIndex)
		where T : unmanaged
	{
		return (T*)this.GetXivGameObject(objectTableIndex);
	}

	public unsafe XivGameObject* GetXivGameObject(int index)
	{
		TickService.VerifyGameTickThread();

		Span<Pointer<XivGameObject>> indexSorted = XivGameObjectManager.Instance()->Objects.IndexSorted;

		if (index < 0)
			return null;

		if (index >= indexSorted.Length)
			return null;

		return indexSorted[index];
	}

	private unsafe void OnGameTick()
	{
		lock (this.gameObjectLookup)
		{
			HashSet<ushort> toRemove = new(this.gameObjectLookup.Keys);
			XivGameObjectManager.ObjectArrays objs = XivGameObjectManager.Instance()->Objects;
			foreach (XivGameObject* gameObject in objs.IndexSorted)
			{
				if (gameObject == null)
					continue;

				ushort index = gameObject->ObjectIndex;
				toRemove.Remove(index);

				if (this.gameObjectLookup.ContainsKey(index))
					continue;

				GameObject obj = new(index);
				this.Services.Scene.AddObject(obj);
				this.gameObjectLookup.Add(index, obj);
			}

			foreach (ushort index in toRemove)
			{
				this.Services.Scene.RemoveObject(this.gameObjectLookup[index]);
				this.gameObjectLookup.Remove(index);
			}
		}
	}
}