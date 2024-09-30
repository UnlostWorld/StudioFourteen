namespace StudioFourteen.Mvm;

using Newtonsoft.Json;
using StudioFourteen.Mvm;
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