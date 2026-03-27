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

namespace StudioFourteen.Services.Rendering;

using StudioFourteen;
using StudioFourteen.Services.Rendering.Effects;
using StudioFourteen.Services.Rendering.Passes;

public class RenderingService : IService
{
	public readonly GameOverlayRenderer OverlayRenderer = new();

	public RenderingService()
	{
		this.OverlayRenderer.Attach();

		////ScreenEffectPass<DisplayUiMaskEffect> pass = new();
		////this.OverlayRenderer.AddPass(OverlayLayers.AfterEffects, pass);

		////ScreenEffectPass<DisplayDepthEffect> pass = new();
		////this.OverlayRenderer.AddPass(OverlayLayers.AfterEffects, pass);

		////ScreenEffectPass<DisplayStencilEffect> pass = new();
		////this.OverlayRenderer.AddPass(OverlayLayers.AfterEffects, pass);
	}

	public void Dispose()
	{
		this.OverlayRenderer.Detach();
		this.OverlayRenderer.Dispose();
	}
}