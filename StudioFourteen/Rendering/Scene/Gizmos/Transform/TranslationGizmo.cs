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
using SharpDX.Direct3D11;
using StudioFourteen.Rendering.Materials;
using StudioFourteen.Rendering.Scene.Handles;
using StudioFourteen.Structs.Extensions;

public class TranslationGizmo : SceneGroup
{
	private readonly AxisHandle xHandle;
	private readonly AxisHandle yHandle;
	private readonly AxisHandle zHandle;

	private readonly PlaneHandle xzPlaneHandle;
	private readonly PlaneHandle xyPlaneHandle;
	private readonly PlaneHandle zyPlaneHandle;

	public TranslationGizmo(GizmoBase gizmo)
	{
		this.LocalTransform = Transform.FromScale(0.5f);

		this.xHandle = new(gizmo);
		this.Add(this.xHandle);
		this.xHandle.Transform = Transform.FromRotation(0, 0, 0);
		this.xHandle.Color = Axes.XColor;
		this.xHandle.AxisUnit = Vector3.UnitX;

		this.yHandle = new(gizmo);
		this.Add(this.yHandle);
		this.yHandle.Transform = Transform.FromRotation(0, 0, 90);
		this.yHandle.Color = Axes.YColor;
		this.yHandle.AxisUnit = Vector3.UnitY;

		this.zHandle = new(gizmo);
		this.Add(this.zHandle);
		this.zHandle.Transform = Transform.FromRotation(-90, 0, 0);
		this.zHandle.Color = Axes.ZColor;
		this.zHandle.AxisUnit = Vector3.UnitZ;

		this.xzPlaneHandle = new(gizmo);
		this.Add(this.xzPlaneHandle);

		this.xzPlaneHandle.Color = Axes.YColor;
		this.xzPlaneHandle.AxisUnit = Vector3.UnitY;

		this.xyPlaneHandle = new(gizmo);
		this.Add(this.xyPlaneHandle);
		this.xyPlaneHandle.Color = Axes.ZColor;
		this.xyPlaneHandle.AxisUnit = Vector3.UnitZ;

		this.zyPlaneHandle = new(gizmo);
		this.Add(this.zyPlaneHandle);
		this.zyPlaneHandle.Color = Axes.XColor;
		this.zyPlaneHandle.AxisUnit = Vector3.UnitX;
	}

	public bool IsBeingManipulated =>
		this.xHandle.IsHovered
		|| this.yHandle.IsHovered
		|| this.zHandle.IsHovered;

	public override void Draw(Transform transform, Device device, DeviceContext deviceContext)
	{
		base.Draw(transform, device, deviceContext);

		Transform thisTransform = this.Transform * transform;

		Vector3 lookVector = this.WorldPosition - this.Services.Camera.CurrentPosition;
		lookVector = Vector3.Normalize(lookVector);

		float x = Vector3.Dot(Vector3.Transform(Vector3.UnitX, thisTransform.Rotation), lookVector);
		float y = Vector3.Dot(Vector3.Transform(Vector3.UnitY, thisTransform.Rotation), lookVector);
		float z = Vector3.Dot(Vector3.Transform(Vector3.UnitZ, thisTransform.Rotation), lookVector);

		Vector3 scale = new(
			x > 0 ? -1 : 1,
			y > 0 ? -1 : 1,
			z > 0 ? -1 : 1);

		this.zyPlaneHandle.Transform = Transform.FromRotation(90, 0, 90) * Transform.FromScale(scale);
		this.xyPlaneHandle.Transform = Transform.FromRotation(180, 270, 90) * Transform.FromScale(scale);
		this.xzPlaneHandle.Transform = Transform.FromRotation(0, 0, 0) * Transform.FromScale(scale);
	}

	public class AxisHandle : Handle
	{
		private readonly MeshRenderer<GizmoFlatMaterial> coneRenderer;
		private readonly MeshRenderer<GizmoFlatOutlineMaterial> coneOutlineRenderer;
		private readonly LineRenderer<GizmoLineMaterial> lineRenderer;
		private readonly GizmoBase gizmo;

		private Vector2 dragStartScreenNormal;
		private Vector3 totalTranslation;

		public AxisHandle(GizmoBase gizmo)
		{
			this.gizmo = gizmo;

			this.coneOutlineRenderer = new(MeshContent.Cone);
			this.coneOutlineRenderer.Transform = Transform.FromTRS(
				new(1, 0, 0),
				Quaternion.CreateFromYawPitchRoll(0, 0, -90 * QuaternionExtensions.Deg2Rad),
				new(0.25f, 0.25f, 0.25f));
			this.coneOutlineRenderer.Material.OutlineColor = Axes.OutlineColor;
			this.Add(this.coneOutlineRenderer);

			this.coneRenderer = new(MeshContent.Cone);
			this.coneRenderer.Transform = Transform.FromTRS(
				new(1, 0, 0),
				Quaternion.CreateFromYawPitchRoll(0, 0, -90 * QuaternionExtensions.Deg2Rad),
				new(0.25f, 0.25f, 0.25f));
			this.Add(this.coneRenderer);

			this.lineRenderer = new();
			this.lineRenderer.From = Vector3.UnitX * 0.05f;
			this.lineRenderer.To = Vector3.UnitX * 0.95f;
			this.lineRenderer.Material.OutlineColor = Axes.OutlineColor;
			this.Add(this.lineRenderer);
		}

		public Color Color { get; set; }
		public float Sensitivity { get; set; } = 1.0f;
		public Vector3 AxisUnit { get; set; }

		public override bool GetToolTip(ref string content, ref Vector3 worldPosition)
		{
			if (!this.IsDragging)
				return false;

			content = $"{this.totalTranslation.Length().ToString("F1")}m";
			worldPosition = this.gizmo.WorldPosition;
			return true;
		}

		protected override void OnDraw()
		{
			base.OnDraw();

			if (this.IsPressed)
			{
				this.lineRenderer.Material.Color = Color.White;
				this.coneRenderer.Material.Color = Color.White;
			}
			else
			{
				this.lineRenderer.Material.Color = this.Color;
				this.coneRenderer.Material.Color = this.Color;
			}

			if (this.IsHovered)
			{
				this.lineRenderer.Material.Thickness = 1.5f;
			}
			else
			{
				this.lineRenderer.Material.Thickness = 1.0f;
			}
		}

		protected override void OnStartDrag(HitTestResult hitTest)
		{
			this.totalTranslation = Vector3.Zero;
			this.dragStartScreenNormal = hitTest.ScreenNormal;
		}

		protected override void OnDrag(Vector2 delta)
		{
			float mag = delta.Length();
			delta = Vector2.Normalize(delta);

			float dot = Vector2.Dot(delta, this.dragStartScreenNormal);
			float dragDelta = (float)(mag * dot);
			float change = dragDelta / 50;
			change *= this.Sensitivity;

			if (this.Services.Input.FastChange)
				change *= 10;

			if (this.Services.Input.SlowChange)
				change /= 10;

			if (this.Services.Tablet.PenPressure > 0)
				change *= (float)this.Services.Tablet.PenPressure;

			Vector3 move = this.AxisUnit * change;
			this.gizmo.Transform = Transform.FromTranslation(-move) * this.gizmo.Transform;
			base.OnDrag(delta);

			this.totalTranslation += move;
		}
	}

	public class PlaneHandle : Handle
	{
		private readonly MeshRenderer<GizmoFlatMaterial> planeRenderer;
		private readonly GizmoBase gizmo;

		private Vector2 dragStartScreenNormal;
		private Vector3 totalTranslation;

		public PlaneHandle(GizmoBase gizmo)
		{
			this.gizmo = gizmo;

			this.planeRenderer = new(MeshContent.Plane);
			this.planeRenderer.Transform = Transform.FromTRS(
				new(0.25f, 0, 0.25f),
				Quaternion.Identity,
				new(0.25f, 0.25f, 0.25f));
			this.planeRenderer.CullMode = CullMode.None;
			this.planeRenderer.HitTestBias = 10.0f;
			this.Add(this.planeRenderer);
		}

		public Color Color { get; set; }
		public float Sensitivity { get; set; } = 1.0f;
		public Vector3 AxisUnit { get; set; }

		public override bool GetToolTip(ref string content, ref Vector3 worldPosition)
		{
			if (!this.IsDragging)
				return false;

			content = $"{this.totalTranslation.Length().ToString("F1")}m";
			worldPosition = this.gizmo.WorldPosition;
			return true;
		}

		protected override void OnDraw()
		{
			base.OnDraw();

			if (this.IsPressed)
			{
				this.planeRenderer.Material.Color = Color.White;
			}
			else
			{
				this.planeRenderer.Material.Color = this.Color;
				this.planeRenderer.Material.Color.A = 0.5f;
			}

			if (this.IsHovered)
			{
			}
			else
			{
			}
		}

		protected override void OnStartDrag(HitTestResult hitTest)
		{
			this.totalTranslation = Vector3.Zero;
			this.dragStartScreenNormal = hitTest.ScreenNormal;
		}

		protected override void OnDrag(Vector2 delta)
		{
			float mag = delta.Length();
			delta = Vector2.Normalize(delta);

			float dot = Vector2.Dot(delta, this.dragStartScreenNormal);
			float dragDelta = (float)(mag * dot);
			float change = dragDelta / 50;
			change *= this.Sensitivity;

			if (this.Services.Input.FastChange)
				change *= 10;

			if (this.Services.Input.SlowChange)
				change /= 10;

			if (this.Services.Tablet.PenPressure > 0)
				change *= (float)this.Services.Tablet.PenPressure;

			Vector3 move = this.AxisUnit * change;
			this.gizmo.Transform = Transform.FromTranslation(-move) * this.gizmo.Transform;
			base.OnDrag(delta);

			this.totalTranslation += move;
		}
	}
}