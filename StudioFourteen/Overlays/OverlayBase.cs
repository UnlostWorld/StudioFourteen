namespace StudioFourteen.Overlays;

using Serilog;
using System.Windows.Controls;
using TerraFX.Interop.Windows;

public abstract class OverlayBase(string group, string name)
{
	public readonly string Group = group;
	public readonly string Name = name;

	public ILogger Log => Logging.ForContext(this.GetType());
	public ServiceManager Services => ServiceManager.Instance;

	public bool IsVisible { get; protected set; } = true;
	public bool IsInitialized { get; private set; } = false;

	public bool IsHidden { get; set; } = false;

	public void Enable()
	{
		this.Log.Information($"ENABLE {this}");

		this.IsInitialized = false;
		this.Services.Overlays.AddOverlay(this);
	}

	public void Disable()
	{
		this.Services.Overlays.RemoveOverlay(this);
	}

	public virtual void Initialize(Canvas canvas)
	{
		this.IsInitialized = true;
	}

	public abstract void Update(Canvas canvas);

	public virtual void Shutdown(Canvas canvas)
	{
		this.IsInitialized = false;
	}
}
