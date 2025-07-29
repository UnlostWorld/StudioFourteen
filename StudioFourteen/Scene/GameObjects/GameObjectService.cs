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
using StudioFourteen.Scene.GameObjects.Characters;
using StudioFourteen.Services;

using XivGameObject = FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject;
using XivGameObjectManager = FFXIVClientStructs.FFXIV.Client.Game.Object.GameObjectManager;

public class GameObjectService : ServiceBase
{
	private readonly Dictionary<ushort, GameObject?> gameObjectLookup = new();

	public override void Attach()
	{
		this.Services.Tick.Add(TickService.Channels.GameTick, this.OnGameTick);
		base.Attach();
	}

	public override void Detach()
	{
		this.Services.Tick.Remove(TickService.Channels.GameTick, this.OnGameTick);

		this.gameObjectLookup.Clear();

		base.Detach();
	}

	public T? Get<T>(int index)
	{
		GameObject? obj = this.Get(index);
		if (obj is T tObj)
			return tObj;

		return default;
	}

	public GameObject? Get(int index)
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

	public unsafe T* GetXivObject<T>(int objectTableIndex)
		where T : unmanaged
	{
		return (T*)this.GetXivObject(objectTableIndex);
	}

	public unsafe XivGameObject* GetXivObject(int index)
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

				if (this.Services.GroupPose.IsGroupPosing && gameObject->ObjectIndex < GroupPoseService.GPoseFirstCharacter)
					continue;

				if (!this.Services.GroupPose.IsGroupPosing && gameObject->ObjectIndex >= GroupPoseService.GPoseFirstCharacter)
					continue;

				if (!gameObject->IsReadyToDraw() || gameObject->DrawObject == null)
					continue;

				ushort index = gameObject->ObjectIndex;
				toRemove.Remove(index);

				if (this.gameObjectLookup.ContainsKey(index))
					continue;

				GameObject? obj = this.Create(gameObject);
				if (obj == null)
					continue;

				this.Services.Scene.AddObject(obj);
				this.gameObjectLookup.Add(index, obj);
			}

			foreach (ushort index in toRemove)
			{
				GameObject? obj = this.gameObjectLookup[index];
				if (obj != null)
					this.Services.Scene.RemoveObject(obj);

				this.gameObjectLookup.Remove(index);
			}
		}
	}

	private unsafe GameObject? Create(XivGameObject* pGameObject)
	{
		int objectIndex = pGameObject->ObjectIndex;

		switch (pGameObject->GetObjectKind())
		{
			case FFXIVClientStructs.FFXIV.Client.Game.Object.ObjectKind.Pc:
			case FFXIVClientStructs.FFXIV.Client.Game.Object.ObjectKind.BattleNpc:
			case FFXIVClientStructs.FFXIV.Client.Game.Object.ObjectKind.EventNpc:
			case FFXIVClientStructs.FFXIV.Client.Game.Object.ObjectKind.Retainer:
				return new Character(objectIndex);

			// TODO:
			case FFXIVClientStructs.FFXIV.Client.Game.Object.ObjectKind.Mount:
			case FFXIVClientStructs.FFXIV.Client.Game.Object.ObjectKind.Companion:
			case FFXIVClientStructs.FFXIV.Client.Game.Object.ObjectKind.Ornament:
				return null;
		}

		return null;
	}
}