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

namespace StudioFourteen.Scene;

using System;
using System.Collections.Generic;
using StudioFourteen.Services;

public class SceneService : ServiceBase
{
	private readonly List<SceneObjectBase> objects = new();

	public delegate void SceneChanged(SceneObjectBase obj);

	public event SceneChanged? ObjectAdded;
	public event SceneChanged? ObjectRemoved;

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

	public void AddObject(SceneObjectBase obj)
	{
		lock (this.objects)
		{
			this.objects.Add(obj);
		}

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

	private void OnGameTick()
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