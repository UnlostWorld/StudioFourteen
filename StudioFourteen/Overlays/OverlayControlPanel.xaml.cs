// .                    @@             _____ _______ _    _ _____ _____ ____
//          @       @@@@@             / ____|__   __| |  | |  __ \_   _/ __ \
//         @@@  @@@@                 | (___    | |  | |  | | |  | || || |  | |
//         @@@@@@@@@  @    @          \___ \   | |  | |  | | |  | || || |  | |
//        @@@@       @@@@@@@          ____) |  | |  | |__| | |__| || || |__| |
//    @@@@@             @@@          |_____/   |_|   \____/|_____/_____\____/
//     @@@      @@@      @@        ___     _    _   _  __   _____  ___  ___  _  _
//      @@    @@@@@@@    @@       |  _|  / _ \ | | | || _ \|_   _|| __|| __|| \| |
//      @@    @@@@@@@    @   @    | __| | (_) || |_| ||   /  | |  | _| | _| | .` |
//    @@@@      @@@      @@@@     |_|    \___/  \___/ |_|_\  |_|  |___||___||_|\_|
//     @@@@             @@@        https://github.com/UnlostWorld/StudioFourteen
//       @@@@@      @@@@@
//        @@@@@@@@@@@@@@                This software is licensed under the
//            @@@@  @                  GNU AFFERO GENERAL PUBLIC LICENSE v3

namespace StudioFourteen.Overlays;

using StudioFourteen.Panels;
using System.ComponentModel;
using WpfUtils.Extensions;

public partial class OverlayControlPanel : Panel
{
	public OverlayControlPanel()
	{
		if (DesignerProperties.GetIsInDesignMode(this))
			return;

		this.Services.Overlays.LayerAdded += this.OnLayerAdded;
		this.Services.Overlays.LayerRemoved += this.OnLayerRemoved;

		this.Overlays.AddRange(this.Services.Overlays.GetOverlayLayers());
	}

	public FastObservableCollection<OverlayLayerBase> Overlays { get; init; } = new();

	private void OnLayerAdded(OverlayLayerBase overlay)
	{
		this.Dispatcher.Invoke(() =>
		{
			this.Overlays.Add(overlay);
		});
	}

	private void OnLayerRemoved(OverlayLayerBase overlay)
	{
		this.Dispatcher.Invoke(() =>
		{
			this.Overlays.Remove(overlay);
		});
	}
}