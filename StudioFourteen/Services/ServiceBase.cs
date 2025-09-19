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

using Serilog;
using StudioFourteen.Settings;
using StudioFourteen.Xaml;
using System.Threading.Tasks;

[NotifyPropertyChanged]
public abstract partial class ServiceBase
{
	protected readonly ILogger Log;

	public ServiceBase()
	{
		this.Log = Logging.ForContext(this.GetType());
	}

	public virtual string Name => this.GetType().Name;
	public virtual object? Icon => XamlResources.Find("ICON_Selection_Service");
	public bool IsReady => this.IsAlive;

	public bool IsAlive { get; private set; }
	public bool IsAttached { get; private set; }

	protected ServiceManager Services => ServiceManager.Instance;

	protected Configuration Settings => this.Services.Settings.Current;

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

	public virtual void Attach()
	{
		this.IsAttached = true;
	}

	public virtual void Detach()
	{
		this.IsAttached = false;
	}

	public virtual void Dispose()
	{
		if (this.IsAttached)
			this.Detach();
	}
}