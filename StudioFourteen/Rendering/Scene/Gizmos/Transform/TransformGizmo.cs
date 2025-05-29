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

namespace StudioFourteen.Rendering.Scene.Gizmos.Transforms;

using StudioFourteen.Rendering.Materials;
using StudioFourteen.Rendering.Scene.Handles;

public class TransformGizmo : GizmoBase
{
	private readonly QuaternionGizmo rotation = new();

	public TransformGizmo()
	{
		this.Add(this.rotation);
	}

	public override string Name => "Transform";
}

public class QuaternionGizmo : SceneGroup
{
	private readonly AxisHandle xHandle = new();
	private readonly AxisHandle yHandle = new();
	private readonly AxisHandle zHandle = new();
	private readonly OrbHandle orbHandle = new();

	public QuaternionGizmo()
	{
		this.Add(this.xHandle);
		this.xHandle.Transform = Transform.FromRotation(0, 90, 0);
		this.xHandle.Color = Axes.XColor;

		this.Add(this.yHandle);
		this.yHandle.Transform = Transform.FromRotation(0, 0, 0);
		this.yHandle.Color = Axes.YColor;

		this.Add(this.zHandle);
		this.zHandle.Transform = Transform.FromRotation(0, 0, 90);
		this.zHandle.Color = Axes.ZColor;

		this.Add(this.orbHandle);
	}
}

public class AxisHandle : Handle
{
	private readonly MeshRenderer<GizmoLineMaterial> circleRenderer;

	public AxisHandle()
	{
		this.circleRenderer = new(MeshContent.WireCircle);
		this.Add(this.circleRenderer);
	}

	public Color Color { get; set; }

	protected override void OnDraw()
	{
		base.OnDraw();

		this.circleRenderer.Material.Color = this.Color;

		if (this.IsHovered)
		{
			this.circleRenderer.Material.Thickness = 1.5f;
		}
		else
		{
			this.circleRenderer.Material.Thickness = 1.0f;
		}
	}
}

public class OrbHandle : Handle
{
	private readonly MeshRenderer<PositionColorMaterial> sphereRenderer;

	public OrbHandle()
	{
		this.sphereRenderer = new(MeshContent.Sphere);
		this.Add(this.sphereRenderer);
	}
}