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

namespace StudioFourteen.Services.Scene;

using System;
using System.Collections.Generic;
using FFXIVClientStructs.Interop;
using StudioFourteen.Scene;
using StudioFourteen.Services.Tick;

using XivGameObject = FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject;
using XivGameObjectManager = FFXIVClientStructs.FFXIV.Client.Game.Object.GameObjectManager;

public class SceneService : IService
{
	private readonly Dictionary<ushort, GameObject?> gameObjectLookup = new();
	private readonly List<SceneObjectBase> objects = new();

	public SceneService()
	{
		Studio.Tick.Add(TickChannels.EarlyGame, this.OnEarlyGameTick);
		Studio.Tick.Add(TickChannels.Game, this.OnGameTick);
	}

	public delegate void SceneChanged(SceneObjectBase obj);

	public event SceneChanged? ObjectAdded;
	public event SceneChanged? ObjectRemoved;

	public void Dispose()
	{
		Studio.Tick.Remove(TickChannels.EarlyGame, this.OnEarlyGameTick);
		Studio.Tick.Remove(TickChannels.Game, this.OnGameTick);

		List<SceneObjectBase> objects = new(this.objects);
		foreach (SceneObjectBase obj in objects)
		{
			this.ObjectRemoved?.Invoke(obj);
			obj.Dispose();
		}

		this.objects.Clear();
		this.gameObjectLookup.Clear();
	}

	public T AddObject<T>(string? name = null)
		where T : SceneObjectBase, new()
	{
		T obj = new();
		if (string.IsNullOrEmpty(name))
			name = $"New {name}";

		obj.Name = name;
		this.AddObject(obj);
		return obj;
	}

	public void AddObject(SceneObjectBase obj)
	{
		lock (this.objects)
		{
			this.objects.Add(obj);
		}

		Studio.Log.Information($"Adding object to scene: {obj}");
		this.ObjectAdded?.Invoke(obj);
	}

	public void RemoveObject(SceneObjectBase obj)
	{
		lock (this.objects)
		{
			this.objects.Remove(obj);
		}

		this.ObjectRemoved?.Invoke(obj);
		obj.Dispose();
	}

	public SceneObjectBase? GetObject(string id)
	{
		throw new NotImplementedException();
	}

	public List<T> FindObjects<T>()
		where T : SceneObjectBase
	{
		lock (this.objects)
		{
			List<T> results = new();
			foreach (SceneObjectBase obj in this.objects)
			{
				if (obj is T tObj)
				{
					results.Add(tObj);
				}
			}

			return results;
		}
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

		if (index < 0)
			return null;

		Span<Pointer<XivGameObject>> indexSorted = XivGameObjectManager.Instance()->Objects.IndexSorted;

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

				/*if (Studio.GroupPose.IsGroupPosing && gameObject->ObjectIndex < GroupPoseService.GPoseFirstCharacter)
					continue;

				if (!Studio.GroupPose.GroupPose.IsGroupPosing && gameObject->ObjectIndex >= GroupPoseService.GPoseFirstCharacter)
					continue;*/

				if (!gameObject->IsReadyToDraw() || gameObject->DrawObject == null)
					continue;

				ushort index = gameObject->ObjectIndex;
				toRemove.Remove(index);

				if (this.gameObjectLookup.ContainsKey(index))
					continue;

				GameObject? obj = this.Create(gameObject);
				if (obj == null)
					continue;

				obj.Name = gameObject->NameString;

				this.AddObject(obj);
				this.gameObjectLookup.Add(index, obj);
			}

			foreach (ushort index in toRemove)
			{
				GameObject? obj = this.gameObjectLookup[index];
				if (obj != null)
					this.RemoveObject(obj);

				this.gameObjectLookup.Remove(index);
			}
		}
	}

	private unsafe GameObject? Create(XivGameObject* pGameObject)
	{
		int objectIndex = pGameObject->ObjectIndex;

		switch (pGameObject->GetObjectKind())
		{
			case FFXIVClientStructs.FFXIV.Client.Game.Object.ObjectKind.Pc: return new PlayerCharacter(objectIndex);
			case FFXIVClientStructs.FFXIV.Client.Game.Object.ObjectKind.BattleNpc: return new BattleNpc(objectIndex);
			case FFXIVClientStructs.FFXIV.Client.Game.Object.ObjectKind.EventNpc: return new EventNpc(objectIndex);
			case FFXIVClientStructs.FFXIV.Client.Game.Object.ObjectKind.Retainer: return new Retainer(objectIndex);
			case FFXIVClientStructs.FFXIV.Client.Game.Object.ObjectKind.Mount:
			case FFXIVClientStructs.FFXIV.Client.Game.Object.ObjectKind.Companion:
			case FFXIVClientStructs.FFXIV.Client.Game.Object.ObjectKind.Ornament:
				return null;
		}

		return null;
	}

	private void OnEarlyGameTick()
	{
		lock (this.objects)
		{
			foreach (SceneObjectBase obj in this.objects.ToArray())
			{
				obj.OnGameTick();
			}
		}
	}
}