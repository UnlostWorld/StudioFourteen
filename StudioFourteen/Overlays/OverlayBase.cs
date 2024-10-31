namespace StudioFourteen.Overlays;

using Serilog;
using StudioFourteen.Settings;
using System.Windows.Controls;

public abstract class OverlayBase(string group, string name)
{
	public readonly string Group = group;
	public readonly string Name = name;

	protected readonly Persistence persistence = new($"Overlay_{group}_{name}");

	public ILogger Log => Logging.ForContext(this.GetType());
	public ServiceManager Services => ServiceManager.Instance;

	public bool IsVisible { get; protected set; } = true;
	public bool IsInitialized { get; private set; } = false;

	public string DisplayGroup => Resources.Find($"LOC_OverlayGroup_{this.Group}", this.Group);
	public string DisplayName => Resources.Find($"LOC_Overlay_{this.Name}", this.Name);

	public bool IsHidden
	{
		get => this.persistence.GetPersistence<bool>();
		set => this.persistence.SetPersistence(value);
	}

	public void Enable()
	{
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
