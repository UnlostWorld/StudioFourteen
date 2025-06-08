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

namespace StudioFourteen.Rendering.Scene.Gizmos;

using System.Diagnostics;
using StudioFourteen.Rendering.Materials;
using StudioFourteen.Rendering.Scene.Gizmos.Transforms;

public class SelectionGizmo : TransformGizmoBase
{
	private readonly MeshRenderer<GizmoFlatMaterial> circleRenderer;
	private readonly Stopwatch flashTimer = new();

	public SelectionGizmo()
	{
		this.circleRenderer = new(MeshContent.WireCircle);
		this.circleRenderer.WriteDepth = false;
		this.Add(this.circleRenderer);
	}

	public override string Name => "Selection";
	public override object? Icon => Resources.Find("ICON_Selection");

	public override void Enable()
	{
		base.Enable();
		this.flashTimer.Restart();
	}

	protected override void OnDraw()
	{
		base.OnDraw();

		float p = this.flashTimer.ElapsedMilliseconds / 1000.0f;
		p = 1 - float.Clamp(p, 0, 1);
		this.circleRenderer.Material.Color.A = p;

		if (p <= 0)
		{
			this.flashTimer.Stop();
		}
	}
}