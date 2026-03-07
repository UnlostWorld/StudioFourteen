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

using System.Collections.ObjectModel;
using System.Numerics;
using CommunityToolkit.Mvvm.ComponentModel;
using StudioFourteen.Services.Scene;
using StudioFourteen.Services.Tick;

public partial class Hierarchy : OverlayReference
{
	[ObservableProperty] private ObservableCollection<SceneObjectBase> sceneObjects = new();

	public Hierarchy()
		: base("UI/Hierarchy.ui", new Vector2(0, 0))
	{
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

	private void OnSceneObjectAdded(SceneObjectBase obj)
	{
		Studio.Tick.Dispatch(TickChannels.Ui, () => this.SceneObjects.Add(obj));
	}

	private void OnSceneObjectRemoved(SceneObjectBase obj)
	{
		Studio.Tick.Dispatch(TickChannels.Ui, () => this.SceneObjects.Remove(obj));
	}
}