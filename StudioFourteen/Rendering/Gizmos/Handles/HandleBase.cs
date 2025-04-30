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

namespace StudioFourteen.Rendering.Gizmos.Handles;

using FFXIVClientStructs.FFXIV.Client.Game.Object;
using SharpDX.Direct3D11;
using StudioFourteen.Rendering.Scene;
using StudioFourteen.Selection;

public class HandleBase : GizmoBase
{
	// TODO: a handle service for doing the hit test dispatch, instead of cramming it
	// into the render pass?

	/*
	// Test
		Vector2? mouse = this.Services.Input.Mouse?.GetPosition();
		if (mouse != null)
		{
			HitTestResult result = new();
			this.Geometry.HitTest(mouse.Value, ref result);

			this.Log.Information($">> {result.DrawObject} {result.Distance}");
		}
		*/
}