namespace StudioFourteen.Services;

using Dalamud.Plugin.Services;
using StudioFourteen.Mvm;
using StudioFourteen.Plugin;
using StudioFourteen.Settings;
using System.Threading.Tasks;

public abstract class ServiceBase : ViewModel
{
	public bool IsAlive { get; private set; }

	protected SettingsService.Configuration Settings => this.Services.Settings.Current;

	public virtual Task Initialize()
	{
		this.IsAlive = true;
		return Task.CompletedTask;
	}

	public virtual Task Start()
	{
		if (DalamudServices.Framework != null)
			DalamudServices.Framework.Update += this.OnFrameworkUpdate;

		return Task.CompletedTask;
	}

	public virtual Task Stop()
	{
		if (DalamudServices.Framework != null)
			DalamudServices.Framework.Update -= this.OnFrameworkUpdate;

		this.IsAlive = false;
		return Task.CompletedTask;
	}

	public virtual Task Shutdown()
	{
		if (DalamudServices.Framework != null)
			DalamudServices.Framework.Update -= this.OnFrameworkUpdate;

		return Task.CompletedTask;
	}

	public virtual Task Tick()
	{
		return Task.CompletedTask;
	}

	protected virtual void OnFrameworkUpdate(IFramework framework)
	{
	}
}
