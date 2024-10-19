namespace StudioFourteen.Mvm;

using Newtonsoft.Json;
using StudioFourteen.Mvm;
using Serilog;
using System.ComponentModel;
using System.Runtime.CompilerServices;

public abstract class AutoViewModel : AutoNotify
{
	protected readonly ILogger Log;

	public AutoViewModel()
		: base()
	{
		this.Log = Logging.ForContext(this.GetType());
	}

	[JsonIgnore]
	public ServiceManager Services => ServiceManager.Instance;
}

public abstract class ViewModel : INotifyPropertyChanged
{
	protected readonly ILogger Log;

	public ViewModel()
		: base()
	{
		this.Log = Logging.ForContext(this.GetType());
	}

	public event PropertyChangedEventHandler? PropertyChanged;

	[JsonIgnore]
	public ServiceManager Services => ServiceManager.Instance;

	protected virtual void RaisePropertyChanged([CallerMemberName] string propertyName = "")
	{
		this.PropertyChanged?.Invoke(this, new(propertyName));
	}
}