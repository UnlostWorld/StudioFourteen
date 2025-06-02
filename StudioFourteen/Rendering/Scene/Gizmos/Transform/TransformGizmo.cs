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

using System.Numerics;
using StudioFourteen.Rendering.Materials;
using StudioFourteen.Rendering.Scene.Handles;

public class TransformGizmo : GizmoBase
{
	private readonly QuaternionGizmo rotation;

	public TransformGizmo()
	{
		this.rotation = new(this);
		this.Add(this.rotation);
	}

	public override string Name => "Transform";

	public override bool IsBeingManipulated => this.rotation.IsBeingManipulated;
}

public class QuaternionGizmo : SceneGroup
{
	private readonly AxisHandle xHandle;
	private readonly AxisHandle yHandle;
	private readonly AxisHandle zHandle;
	private readonly OrbHandle orbHandle;

	public QuaternionGizmo(GizmoBase gizmo)
	{
		this.xHandle = new(gizmo);
		this.Add(this.xHandle);
		this.xHandle.Transform = Transform.FromRotation(0, 90, 0) * Transform.FromScale(0.5f);
		this.xHandle.Color = Axes.XColor;

		this.yHandle = new(gizmo);
		this.Add(this.yHandle);
		this.yHandle.Transform = Transform.FromRotation(0, 0, 0) * Transform.FromScale(0.5f);
		this.yHandle.Color = Axes.YColor;

		this.zHandle = new(gizmo);
		this.Add(this.zHandle);
		this.zHandle.Transform = Transform.FromRotation(0, 0, 90) * Transform.FromScale(0.5f);
		this.zHandle.Color = Axes.ZColor;

		this.orbHandle = new(gizmo);
		this.Add(this.orbHandle);
	}

	public bool IsBeingManipulated =>
		this.xHandle.IsHovered
		|| this.yHandle.IsHovered
		|| this.zHandle.IsHovered
		|| this.orbHandle.IsHovered;
}

public class AxisHandle : Handle
{
	private readonly MeshRenderer<GizmoLineMaterial> circleRenderer;
	private readonly GizmoBase gizmo;

	public AxisHandle(GizmoBase gizmo)
	{
		this.gizmo = gizmo;
		this.circleRenderer = new(MeshContent.WireCircle);
		this.Add(this.circleRenderer);
	}

	public Color Color { get; set; }

	protected override void OnDraw()
	{
		base.OnDraw();

		if (this.IsPressed)
		{
			this.circleRenderer.Material.Color = Color.White;
		}
		else
		{
			this.circleRenderer.Material.Color = this.Color;
		}

		if (this.IsHovered)
		{
			this.circleRenderer.Material.Thickness = 1.5f;
		}
		else
		{
			this.circleRenderer.Material.Thickness = 1.0f;
		}
	}

	protected override void OnDrag(Vector2 delta)
	{
		// TODO: Not this.
		////this.gizmo.Transform *= Transform.FromTranslation(delta.X / 100, delta.Y / 100, 0);

		base.OnDrag(delta);
	}
}

public class OrbHandle : Handle
{
	private readonly MeshRenderer<GizmoFlatMaterial> sphereRenderer;
	private readonly GizmoBase gizmo;

	public OrbHandle(GizmoBase gizmo)
	{
		this.gizmo = gizmo;
		this.sphereRenderer = new(MeshContent.Sphere);
		this.Add(this.sphereRenderer);
		this.sphereRenderer.Transform = Transform.FromScale(0.48f);
		this.sphereRenderer.HitTestBias = -0.5f;
	}

	protected override void OnDraw()
	{
		base.OnDraw();

		this.sphereRenderer.Material.Color = new(0.0f, 0.0f, 0.0f, 0.75f);
	}
}