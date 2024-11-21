// .                      @@             _____ _______ _    _ _____ _____ ____
//            @       @@@@@             / ____|__   __| |  | |  __ \_   _/ __ \
//           @@@  @@@@                 | (___    | |  | |  | | |  | || || |  | |
//           @@@@@@@@@  @    @          \___ \   | |  | |  | | |  | || || |  | |
//          @@@@       @@@@@@@          ____) |  | |  | |__| | |__| || || |__| |
//      @@@@@             @@@          |_____/   |_|   \____/|_____/_____\____/
//       @@@      @@@      @@        ___     _    _   _  __   _____  ___  ___  _  _
//        @@    @@@@@@@    @@       |  _|  / _ \ | | | || _ \|_   _|| __|| __|| \| |
//        @@    @@@@@@@    @   @    | __| | (_) || |_| ||   /  | |  | _| | _| | .` |
//      @@@@      @@@      @@@@     |_|    \___/  \___/ |_|_\  |_|  |___||___||_|\_|
//       @@@@             @@@
//         @@@@@      @@@@@               This software is licensed under the
//          @@@@@@@@@@@@@@                 GNU AFFERO GENERAL PUBLIC LICENSE
//              @@@@  @                       Version 3, 19 November 2007

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