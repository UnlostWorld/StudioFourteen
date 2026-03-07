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
	public readonly List<SceneObjectBase> Objects = new();
	public readonly List<SceneObjectBase> Selection = new();

	private bool needsDefaultObjects = true;

	public SceneService()
	{
		Studio.Tick.Add(TickChannels.EarlyGame, this.OnEarlyGameTick);
		Studio.Tick.Add(TickChannels.Game, this.OnGameTick);
	}

	public delegate void SceneChanged(SceneObjectBase obj);

	public event SceneChanged? ObjectAdded;
	public event SceneChanged? ObjectRemoved;
	public event SceneChanged? ObjectSelected;
	public event SceneChanged? ObjectDeselected;

	public SceneObjectBase? PrimarySelection => this.Selection.Count > 0 ? this.Selection[0] : null;

	public void Dispose()
	{
		Studio.Tick.Remove(TickChannels.EarlyGame, this.OnEarlyGameTick);
		Studio.Tick.Remove(TickChannels.Game, this.OnGameTick);

		this.Selection.Clear();

		List<SceneObjectBase> objects = new(this.Objects);
		foreach (SceneObjectBase obj in objects)
		{
			this.ObjectRemoved?.Invoke(obj);
			obj.Dispose();
		}

		this.Objects.Clear();
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
		lock (this.Objects)
		{
			this.Objects.Add(obj);
		}

		Studio.Log.Information($"Adding object to scene: {obj}");
		this.ObjectAdded?.Invoke(obj);
	}

	public void RemoveObject(SceneObjectBase obj)
	{
		lock (this.Objects)
		{
			this.Objects.Remove(obj);
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
		lock (this.Objects)
		{
			List<T> results = new();
			foreach (SceneObjectBase obj in this.Objects)
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

		lock (this.Objects)
		{
			foreach (SceneObjectBase obj in this.Objects)
			{
				if (obj is GameObject gameObject && gameObject.ObjectIndex == index)
				{
					return gameObject;
				}
			}

			return null;
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

	public void Select(SceneObjectBase obj, bool clearCurrent = true)
	{
		if (clearCurrent)
			this.ClearSelection();

		this.Selection.Add(obj);
		obj.OnSelected(true);
		this.ObjectSelected?.Invoke(obj);
	}

	public void Deselect(SceneObjectBase obj)
	{
		this.Selection.Remove(obj);
		obj.OnSelected(false);
		this.ObjectDeselected?.Invoke(obj);
	}

	public void ClearSelection()
	{
		foreach (SceneObjectBase obj in this.Selection.ToArray())
		{
			this.ObjectDeselected?.Invoke(obj);
			obj.OnSelected(false);
		}

		this.Selection.Clear();
	}

	public unsafe GameObject? GetOrAddGameObject(int objectIndex)
	{
		GameObject? gameObject = this.Get(objectIndex);
		if (gameObject != null)
			return gameObject;

		TickService.VerifyGameTickThread();
		var indexSorted = XivGameObjectManager.Instance()->Objects.IndexSorted;

		if (objectIndex >= indexSorted.Length)
			return null;

		XivGameObject* pGameObject = indexSorted[objectIndex];
		if (pGameObject == null)
			return null;

		string name = pGameObject->NameString;
		if (string.IsNullOrEmpty(name))
			return null;

		gameObject = this.Create(pGameObject);
		if (gameObject == null)
			return null;

		gameObject.Name = name;

		this.AddObject(gameObject);
		return gameObject;
	}

	private void VerifyDefaultSceneObjects()
	{
		GameObject? player = this.GetOrAddGameObject(0);
		if (player != null)
		{
			player?.WasAddedAsDefaultObject = true;
			this.needsDefaultObjects = false;
		}
	}

	private unsafe void OnGameTick()
	{
		lock (this.Objects)
		{
			var indexSorted = XivGameObjectManager.Instance()->Objects.IndexSorted;

			/*if (Studio.GroupPose.IsGroupPosing && gameObject->ObjectIndex < GroupPoseService.GPoseFirstCharacter)
				continue;

			if (!Studio.GroupPose.GroupPose.IsGroupPosing && gameObject->ObjectIndex >= GroupPoseService.GPoseFirstCharacter)
				continue;*/

			HashSet<GameObject> toRemove = new();
			foreach (var obj in this.Objects)
			{
				if (obj is GameObject gameObject)
				{
					if (gameObject.ObjectIndex >= indexSorted.Length)
					{
						toRemove.Add(gameObject);
						continue;
					}

					// a bit sloppy, maybe pointers or some other internal Id would be better?
					XivGameObject* xivObj = indexSorted[gameObject.ObjectIndex];
					if (xivObj == null || xivObj->NameString != gameObject.Name)
					{
						toRemove.Add(gameObject);
						continue;
					}
				}
			}

			foreach (GameObject gameObject in toRemove)
			{
				this.needsDefaultObjects = gameObject.WasAddedAsDefaultObject;
				this.RemoveObject(gameObject);
			}
		}

		if (this.needsDefaultObjects)
		{
			this.VerifyDefaultSceneObjects();
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
		lock (this.Objects)
		{
			foreach (SceneObjectBase obj in this.Objects.ToArray())
			{
				obj.OnGameTick();
			}
		}
	}
}