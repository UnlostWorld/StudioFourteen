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

namespace StudioFourteen.Rendering.Draw.Gizmos.Transforms;

using System.Numerics;
using StudioFourteen.Rendering.Materials;
using StudioFourteen.Rendering.Draw.Handles;
using StudioFourteen.Structs.Extensions;

public class RotationGizmo : TransformGizmoBase
{
	private readonly AxisHandle xHandle;
	private readonly AxisHandle yHandle;
	private readonly AxisHandle zHandle;
	private readonly OrbHandle orbHandle;

	public RotationGizmo()
	{
		this.orbHandle = new(this);
		this.Add(this.orbHandle);

		this.xHandle = new(this);
		this.Add(this.xHandle);
		this.xHandle.Transform = Transform.FromRotation(0, 0, -90) * Transform.FromScale(0.5f);
		this.xHandle.Color = Axes.XColor;
		this.xHandle.AxisUnit = Vector3.UnitX;

		this.yHandle = new(this);
		this.Add(this.yHandle);
		this.yHandle.Transform = Transform.FromRotation(0, 0, 0) * Transform.FromScale(0.5f);
		this.yHandle.Color = Axes.YColor;
		this.yHandle.AxisUnit = Vector3.UnitY;

		this.zHandle = new(this);
		this.Add(this.zHandle);
		this.zHandle.Transform = Transform.FromRotation(0, 90, 0) * Transform.FromScale(0.5f);
		this.zHandle.Color = Axes.ZColor;
		this.zHandle.AxisUnit = Vector3.UnitZ;
	}

	public override string Name => Resources.Find("LOC_Rotate", "Rotate");
	public override object? Icon => Resources.Find("ICON_Transform_Rotate");

	public override bool IsBeingManipulated =>
		this.xHandle.IsHovered
		|| this.yHandle.IsHovered
		|| this.zHandle.IsHovered
		|| this.orbHandle.IsHovered;

	public class AxisHandle : Handle
	{
		private readonly MeshRenderer<GizmoLineMaterial> circleRenderer;
		private readonly LineRenderer<GizmoLineMaterial> fromLineRenderer;
		private readonly LineRenderer<GizmoLineMaterial> toLineRenderer;
		private readonly RotationGizmo gizmo;

		private Vector2 dragStartScreenNormal;
		private Vector4 dragStartVertPos;
		private Quaternion totalRotation;

		public AxisHandle(RotationGizmo gizmo)
		{
			this.gizmo = gizmo;
			this.circleRenderer = new(MeshContent.WireCircle);
			this.circleRenderer.Material.EndCaps = 0;
			this.circleRenderer.Material.OutlineColor = Axes.OutlineColor;
			this.circleRenderer.Material.FadeOutDepth = 0.075f;
			this.Add(this.circleRenderer);

			this.fromLineRenderer = new();
			this.fromLineRenderer.IsHitTestVisible = false;
			this.fromLineRenderer.IsVisible = false;
			this.Add(this.fromLineRenderer);

			this.toLineRenderer = new();
			this.toLineRenderer.IsHitTestVisible = false;
			this.toLineRenderer.IsVisible = false;
			this.Add(this.toLineRenderer);
		}

		public Color Color { get; set; }
		public float Sensitivity { get; set; } = 1.0f;
		public Vector3 AxisUnit { get; set; }

		public override bool GetToolTip(ref string content, ref Vector3 worldPosition)
		{
			if (!this.IsDragging)
				return false;

			Vector3 euler = this.totalRotation.ToEuler();

			content = $"{euler.X.ToString("F1")}°";
			worldPosition = this.gizmo.WorldPosition;
			return true;
		}

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
				this.circleRenderer.Material.FadeOutDepth = 0;
			}
			else
			{
				this.circleRenderer.Material.Color = this.Color;
				this.fromLineRenderer.Material.Color = this.Color;
				this.toLineRenderer.Material.Color = this.Color;
				this.circleRenderer.Material.FadeOutDepth = 0.075f;
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
			float mag = delta.Length();
			delta = Vector2.Normalize(delta);

			float dot = Vector2.Dot(delta, -this.dragStartScreenNormal);
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
			this.gizmo.TargetTransform = Transform.FromRotation(rot) * this.gizmo.TargetTransform;
			base.OnDrag(delta);

			this.totalRotation *= Quaternion.CreateFromAxisAngle(Vector3.UnitY, angleChange);
		}
	}

	public class OrbHandle : Handle
	{
		private readonly MeshRenderer<GizmoFlatMaterial> sphereRenderer;
		private readonly RotationGizmo gizmo;

		public OrbHandle(RotationGizmo gizmo)
		{
			this.gizmo = gizmo;

			this.sphereRenderer = new(MeshContent.Sphere);
			this.sphereRenderer.Transform = Transform.FromScale(0.48f);
			this.sphereRenderer.HitTestBias = -0.5f;
			this.sphereRenderer.Material.Color = Axes.OutlineColor;
			this.sphereRenderer.Material.Color.A = 0.5f;
			this.sphereRenderer.WriteDepth = false;
			this.Add(this.sphereRenderer);
		}
	}
}
