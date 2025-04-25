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

namespace StudioFourteen.Rendering.Gizmos;

using StudioFourteen.Selection;
using StudioFourteen.Services;

public class SelectionGizmo : GizmoBase
{
	private readonly DrawObject cube = new(EmbeddedMaterial.Line, EmbeddedGeometry.WireCube);

	public SelectionGizmo()
	{
		this.Add(this.cube);
	}

	public override void Enable()
	{
		this.Services.Tick.Add(TickService.Channels.GameTick, this.OnGameTick);
		base.Enable();
	}

	public override void Disable()
	{
		this.Services.Tick.Remove(TickService.Channels.GameTick, this.OnGameTick);
		base.Disable();
	}

	private unsafe void OnGameTick()
	{
		SelectionBase? currentSelection = this.Services.Selection.Current;
		if (currentSelection == null)
			return;

		if (currentSelection is TransformSelectionBase transformSelection)
		{
			this.Transform = transformSelection.WorldTransform;
		}
	}
}