namespace StudioFourteen.Overlays;

using DependencyPropertyGenerator;
using StudioFourteen.Mvm;
using System.Collections.ObjectModel;
using System.ComponentModel;

public partial class OverlayBar : View
{
	public OverlayBar()
	{
		if (DesignerProperties.GetIsInDesignMode(this))
			return;

		this.Services.Overlays.OverlayAdded += this.OnOverlayAdded;
		this.Services.Overlays.OverlayRemoved += this.OnOverlayRemoved;
	}

	public ObservableCollection<OverlayBase> Overlays { get; init; } = new();

	private void OnOverlayAdded(OverlayBase overlay)
	{
		this.Dispatcher.Invoke(() =>
		{
			this.Overlays.Add(overlay);
		});
	}

	private void OnOverlayRemoved(OverlayBase overlay)
	{
		this.Dispatcher.Invoke(() =>
		{
			this.Overlays.Remove(overlay);
		});
	}
}