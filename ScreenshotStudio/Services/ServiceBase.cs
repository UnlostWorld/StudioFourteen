namespace ScreenshotStudio.Services;

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
	public ILogger Log { get; private set; }

	public ServiceManager Services => ServiceManager.Instance;

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

	protected virtual void RaisePropertyChanged([CallerMemberName]string propertyName = "")
	{
		this.PropertyChanged?.Invoke(this, new(propertyName));
	}
}
