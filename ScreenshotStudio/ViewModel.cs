namespace ScreenshotStudio;

using Newtonsoft.Json;
using ScreenshotStudio.Services;
using Serilog;

public abstract class ViewModel : AutoNotify
{
	protected readonly ILogger Log;

	public ViewModel()
		: base()
	{
		this.Log = Logging.ForContext(this.GetType());
	}

	[JsonIgnore]
	public ServiceManager Services => ServiceManager.Instance;
}