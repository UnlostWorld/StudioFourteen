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

namespace StudioFourteen.Controllers;

using System;
using Serilog;
using StudioFourteen.Scene;

public abstract class SceneObjectControllerBase : IDisposable
{
	protected readonly ILogger Log;

	public SceneObjectControllerBase(SceneObjectBase obj)
	{
		this.Object = obj;
		this.Log = Logging.ForContext(this.GetType());
	}

	protected SceneObjectBase? Object { get; private set; }

	protected ServiceManager Services => ServiceManager.Instance;

	public virtual void Activate()
	{
	}

	public virtual void Deactivate()
	{
	}

	public virtual void Dispose()
	{
	}
}

public abstract class SceneObjectControllerBase<T> : SceneObjectControllerBase
	where T : SceneObjectBase
{
	protected SceneObjectControllerBase(T obj)
		: base(obj)
	{
	}

	protected new T? Object => base.Object as T;
}