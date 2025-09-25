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

namespace StudioFourteen.Rendering.Draw.Gizmos;

using System.Diagnostics;
using StudioFourteen.Rendering.Draw.Gizmos.Transforms;
using StudioFourteen.Rendering.Materials;
using StudioFourteen.Rendering.Passes;
using StudioFourteen.Xaml;
using StudioFourteen.Animation;

public class SelectionGizmo : TransformGizmoBase
{
	private const float DurationMs = 500;
	private readonly MeshRenderer<GizmoFlatMaterial> circleRenderer;
	private readonly Stopwatch flashTimer = new();
	private readonly EasingFunctionBase easing = new SineEase();

	public SelectionGizmo()
	{
		this.circleRenderer = new(MeshContent.WireCircle);
		this.circleRenderer.WriteDepth = false;
		this.Add(this.circleRenderer);
	}

	public override string Name => "Selection";
	public override object? Icon => XamlResources.Find("ICON_Selection");
	public override bool ShowInControlPanel => false;
	public override bool KeepScreenSize => false;

	public override void Enable(ForwardPass? pass = null)
	{
		base.Enable(pass);
		this.flashTimer.Restart();
	}

	protected override void OnDraw()
	{
		base.OnDraw();

		float p = this.flashTimer.ElapsedMilliseconds / DurationMs;
		p = float.Clamp(p, 0, 1);
		p = this.easing.Ease(p, EasingFunctionBase.EasingModes.EaseOut);
		this.circleRenderer.Material.Color.A = 1 - p;
		this.circleRenderer.Transform = Transform.FromScale(p * 0.5f);

		if (p <= 0)
		{
			this.flashTimer.Stop();
		}
	}
}