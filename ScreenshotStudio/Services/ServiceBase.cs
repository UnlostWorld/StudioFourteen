namespace ScreenshotStudio.Services;

using Dalamud.Plugin.Services;
using ScreenshotStudio.Plugin;
using Serilog;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

public abstract class ServiceBase : INotifyPropertyChanged
{
	public ServiceBase()
	{
		this.Log = Logging.ForContext(this.GetType());
	}

	public event PropertyChangedEventHandler? PropertyChanged;

	public bool IsAlive { get; private set; }

	protected ILogger Log { get; private set; }
	protected ServiceManager Services => ServiceManager.Instance;

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

	protected virtual void RaisePropertyChanged([CallerMemberName]string propertyName = "")
	{
		this.PropertyChanged?.Invoke(this, new(propertyName));
	}

	protected virtual void OnFrameworkUpdate(IFramework framework)
	{
	}
}
