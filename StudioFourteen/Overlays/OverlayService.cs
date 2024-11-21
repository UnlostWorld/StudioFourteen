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

using StudioFourteen.Services;
using System.Collections.Generic;

public interface IOverlayOwner
{
	string OverlayName { get; }
}

public class OverlayService
	: ServiceBase
{
	private readonly List<OverlayBase> overlays = new();

	public delegate void OverlayEvent(OverlayBase overlay);

	public event OverlayEvent? OverlayAdded;
	public event OverlayEvent? OverlayRemoved;

	public void AddOverlay(OverlayBase overlay)
	{
		this.OverlayAdded?.Invoke(overlay);
	}

	public void RemoveOverlay(OverlayBase overlay)
	{
		this.OverlayRemoved?.Invoke(overlay);
	}

	public List<OverlayBase> GetOverlays()
	{
		return this.overlays;
	}
}
