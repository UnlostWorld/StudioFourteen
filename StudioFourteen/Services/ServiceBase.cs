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

using Dalamud.Plugin.Services;
using StudioFourteen.Mvm;
using StudioFourteen.Plugin;
using StudioFourteen.Settings;
using System.Threading.Tasks;

public abstract class ServiceBase : ViewModel
{
	public bool IsAlive { get; private set; }
	public bool IsAttached { get; private set; }

	protected SettingsService.Configuration Settings => this.Services.Settings.Current;

	public virtual Task Initialize()
	{
		this.IsAlive = true;
		return Task.CompletedTask;
	}

	public virtual Task Start()
	{
		return Task.CompletedTask;
	}

	public virtual Task Stop()
	{
		this.IsAlive = false;
		return Task.CompletedTask;
	}

	public virtual Task Shutdown()
	{
		return Task.CompletedTask;
	}

	public virtual Task Tick()
	{
		return Task.CompletedTask;
	}

	public virtual void Attach()
	{
		if (DalamudServices.Framework != null)
			DalamudServices.Framework.Update += this.OnFrameworkUpdate;

		this.IsAttached = true;
	}

	public virtual void Detach()
	{
		if (DalamudServices.Framework != null)
			DalamudServices.Framework.Update -= this.OnFrameworkUpdate;

		this.IsAttached = false;
	}

	protected virtual void OnFrameworkUpdate(IFramework framework)
	{
	}
}
