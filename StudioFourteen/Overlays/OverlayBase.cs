namespace StudioFourteen.Overlays;

using Serilog;
using System.Windows.Controls;

public abstract class OverlayBase
{
	public ILogger Log => Logging.ForContext(this.GetType());
	public ServiceManager Services => ServiceManager.Instance;

	public bool IsVisible { get; protected set; } = true;
	public bool IsInitialized { get; private set; } = false;
	public bool IsShuttingDown { get; private set; } = false;

	public void Enable()
	{
		this.IsInitialized = false;
		this.IsShuttingDown = false;
		this.Services.Overlays.Overlays.Add(this);
	}

	public void Disable()
	{
		this.IsShuttingDown = true;
	}

	public virtual void Initialize(Canvas canvas)
	{
		this.IsInitialized = true;
	}

	public abstract void Update(Canvas canvas);

	public virtual void Shutdown(Canvas canvas)
	{
		this.Services.Overlays.Overlays.Remove(this);
	}
}
