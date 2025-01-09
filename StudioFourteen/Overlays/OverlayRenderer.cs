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

using StudioFourteen.Overlays.Primitives;
using System.ComponentModel;

public partial class OverlayRenderer : PrimitiveRenderer
{
	public OverlayRenderer()
	{
		if (DesignerProperties.GetIsInDesignMode(this))
			return;

		this.Services.Overlays.LayerAdded += this.OnLayerAdded;
		this.Services.Overlays.LayerRemoved += this.OnLayerRemoved;

		foreach(OverlayLayerBase layer in this.Services.Overlays.GetOverlayLayers())
		{
			this.OnLayerAdded(layer);
		}
	}

	private void OnLayerAdded(OverlayLayerBase overlay)
	{
		this.AddPrimitive(overlay);
	}

	private void OnLayerRemoved(OverlayLayerBase overlay)
	{
		this.RemovePrimitive(overlay);
	}
}
