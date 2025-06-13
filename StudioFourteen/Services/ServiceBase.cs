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

using FontAwesome.Sharp;
using StudioFourteen.History;
using StudioFourteen.Mvm;
using StudioFourteen.Settings;
using System;
using System.Threading.Tasks;

public abstract class ServiceBase : ViewModel, IHistoryTarget
{
	public virtual string Name => this.GetType().Name;
	public virtual object? Icon => Resources.Find("ICON_Selection_Service");
	public bool IsReady => this.IsAlive;

	public bool IsAlive { get; private set; }
	public bool IsAttached { get; private set; }

	protected SettingsService.Configuration Settings => this.Services.Settings.Current;

	public Operation CreateHistoryOperation() => new ServiceOperation(this.GetType());

	public virtual void FinalizeHistoryOperation(ref Operation operation)
	{
	}

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

public class ServiceOperation(Type type)
	: Operation
{
	public Type ServiceType { get; init; } = type;

	public override IHistoryTarget GetTarget()
	{
		return ServiceManager.Instance.GetService(this.ServiceType);
	}

	public override bool IsTarget(IHistoryTarget target)
	{
		return ServiceManager.Instance.Selection.Current == target;
	}
}
