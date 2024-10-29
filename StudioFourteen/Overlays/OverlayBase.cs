namespace StudioFourteen.Overlays;

using Serilog;
using System.Windows.Controls;

public abstract class OverlayBase
{
	public ILogger Log => Logging.ForContext(this.GetType());
	public ServiceManager Services => ServiceManager.Instance;

	public bool IsVisible { get; protected set; } = true;

	public abstract void Update(Canvas canvas);
}
