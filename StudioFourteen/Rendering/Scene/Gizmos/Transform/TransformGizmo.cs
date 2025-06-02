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
		this.xHandle.Transform = Transform.FromRotation(0, 0, -90) * Transform.FromScale(0.5f);
		this.xHandle.Color = Axes.XColor;
		this.xHandle.AxisUnit = Vector3.UnitX;

		this.yHandle = new(gizmo);
		this.Add(this.yHandle);
		this.yHandle.Transform = Transform.FromRotation(0, 0, 0) * Transform.FromScale(0.5f);
		this.yHandle.Color = Axes.YColor;
		this.yHandle.AxisUnit = Vector3.UnitY;

		this.zHandle = new(gizmo);
		this.Add(this.zHandle);
		this.zHandle.Transform = Transform.FromRotation(0, 90, 0) * Transform.FromScale(0.5f);
		this.zHandle.Color = Axes.ZColor;
		this.zHandle.AxisUnit = Vector3.UnitZ;

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
	private readonly LineRenderer<GizmoLineMaterial> fromLineRenderer;
	private readonly LineRenderer<GizmoLineMaterial> toLineRenderer;
	private readonly GizmoBase gizmo;

	private Vector2 dragStartScreenNormal;
	private Vector4 dragStartVertPos;
	private Quaternion totalRotation;

	public AxisHandle(GizmoBase gizmo)
	{
		this.gizmo = gizmo;
		this.circleRenderer = new(MeshContent.WireCircle);
		this.circleRenderer.Material.EndCaps = 0;
		this.Add(this.circleRenderer);

		this.fromLineRenderer = new();
		this.fromLineRenderer.IsHitTestVisible = false;
		this.fromLineRenderer.IsVisible = false;
		this.fromLineRenderer.Material.Outline = 0;
		this.Add(this.fromLineRenderer);

		this.toLineRenderer = new();
		this.toLineRenderer.IsHitTestVisible = false;
		this.toLineRenderer.IsVisible = false;
		this.toLineRenderer.Material.Outline = 0;
		this.Add(this.toLineRenderer);
	}

	public Color Color { get; set; }
	public float Sensitivity { get; set; } = 1.0f;
	public Vector3 AxisUnit { get; set; }

	protected override void OnDraw()
	{
		this.fromLineRenderer.IsVisible = this.IsDragging;
		this.toLineRenderer.IsVisible = this.IsDragging;

		base.OnDraw();

		if (this.IsPressed)
		{
			this.circleRenderer.Material.Color = Color.White;
			this.fromLineRenderer.Material.Color = Color.White;
			this.toLineRenderer.Material.Color = Color.White;
		}
		else
		{
			this.circleRenderer.Material.Color = this.Color;
			this.fromLineRenderer.Material.Color = this.Color;
			this.toLineRenderer.Material.Color = this.Color;
		}

		if (this.IsHovered)
		{
			this.circleRenderer.Material.Thickness = 1.5f;
		}
		else
		{
			this.circleRenderer.Material.Thickness = 1.0f;
		}

		if (this.IsDragging)
		{
			Vector3 from = this.dragStartVertPos.AsVector3();
			this.fromLineRenderer.From = from * 0.01f;
			this.fromLineRenderer.To = from * 0.95f;

			Vector3 to = Vector3.Transform(this.dragStartVertPos.AsVector3(), Quaternion.Inverse(this.totalRotation));
			this.toLineRenderer.From = to * 0.01f;
			this.toLineRenderer.To = to * 0.95f;
		}
	}

	protected override void OnStartDrag(HitTestResult hitTest)
	{
		this.totalRotation = Quaternion.Identity;
		this.dragStartScreenNormal = hitTest.ScreenNormal;

		if (hitTest.MeshVertex != null)
		{
			this.dragStartVertPos = hitTest.MeshVertex.Value.Position;
		}
	}

	protected override void OnDrag(Vector2 delta)
	{
		// TODO: Not this.
		////this.gizmo.Transform *= Transform.FromTranslation(delta.X / 100, delta.Y / 100, 0);

		float mag = delta.Length();
		delta = Vector2.Normalize(delta);

		float dot = Vector2.Dot(delta, this.dragStartScreenNormal);
		float dragDelta = (float)(mag * dot);
		float angleChange = dragDelta / 50;
		angleChange *= this.Sensitivity;

		if (this.Services.Input.FastChange)
			angleChange *= 10;

		if (this.Services.Input.SlowChange)
			angleChange /= 10;

		if (this.Services.Tablet.PenPressure > 0)
			angleChange *= (float)this.Services.Tablet.PenPressure;

		Quaternion rot = Quaternion.CreateFromAxisAngle(this.AxisUnit, angleChange);
		this.gizmo.Transform = Transform.FromRotation(rot) * this.gizmo.Transform;
		base.OnDrag(delta);

		this.totalRotation *= Quaternion.CreateFromAxisAngle(Vector3.UnitY, angleChange);
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