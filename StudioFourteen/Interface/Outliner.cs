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

namespace StudioFourteen.Interface;

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Numerics;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StudioFourteen.Services.Scene;
using StudioFourteen.Services.Tick;

using XibObjectKind = FFXIVClientStructs.FFXIV.Client.Game.Object.ObjectKind;
using XivGameObject = FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject;
using XivGameObjectManager = FFXIVClientStructs.FFXIV.Client.Game.Object.GameObjectManager;

public partial class Outliner : OverlayReference
{
	public Outliner()
		: base("UI/Outliner.ui", new Vector2(0, 0))
	{
		this.SceneObjects = new();
		this.AvailableCharacters = new();

		this.AvailableObjects = new();
		this.AvailableObjects.Add(new("Character", "Icons/Character.svg"));
		this.AvailableObjects.Add(new("Camera", "Icons/Camera.svg"));
		this.AvailableObjects.Add(new("Light", "Icons/Light On.svg"));

		Studio.Scene.ObjectRemoved += this.OnSceneObjectRemoved;

		Studio.Tick.Dispatch(TickChannels.Ui, () =>
		{
			lock (Studio.Scene.Objects)
			{
				foreach (SceneObjectBase obj in Studio.Scene.Objects)
				{
					this.SceneObjects.Add(obj);
				}
			}

			Studio.Scene.ObjectAdded += this.OnSceneObjectAdded;
		});
	}

	[ObservableProperty]
	public partial ObservableCollection<SceneObjectBase> SceneObjects { get; private set; }

	[ObservableProperty]
	public partial ObservableCollection<AvailableCharacter> AvailableCharacters { get; private set; }

	[ObservableProperty]
	public partial ObservableCollection<AvailableObject> AvailableObjects { get; private set; }

	[RelayCommand]
	public void CloseStudio()
	{
		Studio.Close();
	}

	[RelayCommand]
	public async Task GetAvailableCharacters()
	{
		this.AvailableCharacters.Clear();

		await TickService.GameTick();

		List<AvailableCharacter> actors = new();

		unsafe
		{
			var indexSorted = XivGameObjectManager.Instance()->Objects.IndexSorted;
			foreach (XivGameObject* obj in indexSorted)
			{
				if (obj == null)
					continue;

				if (string.IsNullOrEmpty(obj->NameString))
					continue;

				if (obj->ObjectKind != XibObjectKind.Pc
					&& obj->ObjectKind != XibObjectKind.BattleNpc
					&& obj->ObjectKind != XibObjectKind.EventNpc
					&& obj->ObjectKind != XibObjectKind.Mount
					&& obj->ObjectKind != XibObjectKind.Companion
					&& obj->ObjectKind != XibObjectKind.Retainer)
					continue;

				if (Studio.Scene.HasGameObject(obj->ObjectIndex))
					continue;

				AvailableCharacter actor = new(obj->ObjectIndex);
				actor.Name = obj->NameString;
				actors.Add(actor);
			}
		}

		await TickService.UiTick();

		this.AvailableCharacters.Clear();
		foreach (AvailableCharacter actor in actors)
		{
			this.AvailableCharacters.Add(actor);
		}
	}

	private void OnSceneObjectAdded(SceneObjectBase obj)
	{
		Studio.Tick.Dispatch(TickChannels.Ui, () => this.SceneObjects.Add(obj));
	}

	private void OnSceneObjectRemoved(SceneObjectBase obj)
	{
		Studio.Tick.Dispatch(TickChannels.Ui, () => this.SceneObjects.Remove(obj));
	}
}

public partial class AvailableCharacter : ObservableObject
{
	public readonly int ObjectIndex = -1;

	public AvailableCharacter(int objectIndex)
	{
		this.ObjectIndex = objectIndex;
	}

	public AvailableCharacter(int objectIndex, string name, string iconPath)
	{
		this.ObjectIndex = objectIndex;
		this.Name = name;
		this.IconPath = iconPath;
	}

	[ObservableProperty]
	public partial string? Name { get; set; }

	[ObservableProperty]
	public partial string? IconPath { get; set; }

	[RelayCommand]
	public void Add()
	{
		Studio.Tick.Dispatch(TickChannels.Game, () =>
		{
			Studio.Scene.GetOrAddGameObject(this.ObjectIndex);
		});
	}
}

public partial class AvailableObject : ObservableObject
{
	public AvailableObject(string name, string iconPath)
	{
		this.Name = name;
		this.IconPath = iconPath;
	}

	[ObservableProperty]
	public partial string? Name { get; set; }

	[ObservableProperty]
	public partial string? IconPath { get; set; }

	[RelayCommand]
	public void Add()
	{
		/*Studio.Tick.Dispatch(TickChannels.Game, () =>
		{
			Studio.Scene.AddObject<T>();
		});*/
	}
}