namespace StudioFourteen.Overlays;

using StudioFourteen.Mvm;
using System.Collections.ObjectModel;

public partial class OverlayBar : View
{
	public OverlayBar()
	{
		this.Services.Overlays.OverlayAdded += this.OnOverlayAdded;
		this.Services.Overlays.OverlayRemoved += this.OnOverlayRemoved;
	}

	public ObservableCollection<OverlayBarSettings> Settings { get; init; } = new();

	private void OnOverlayAdded(OverlayBase overlay)
	{
		this.Dispatcher.Invoke(() =>
		{
			// TODO: Combine!
			this.Settings.Add(new(overlay));
		});
	}

	private void OnOverlayRemoved(OverlayBase overlay)
	{
		this.Dispatcher.Invoke(() =>
		{
			foreach (OverlayBarSettings setting in this.Settings)
			{
				if (setting.Overlay == overlay)
				{
					this.Settings.Remove(setting);
					break;
				}
			}
		});
	}
}

public class OverlayBarSettings(OverlayBase overlay)
{
	public readonly OverlayBase Overlay = overlay;

	public string Group => this.Overlay.Group;
	public string Name => this.Overlay.Name;

	// TODO: LOC
	public string DisplayName => this.Name;
	public string DisplayGroup => this.Group;

	// TODO: Save settings?
	public bool IsEnabled
	{
		get => !this.Overlay.IsHidden;
		set => this.Overlay.IsHidden = !value;
	}
}