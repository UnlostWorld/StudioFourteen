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
using SharpDX.Direct3D11;
using StudioFourteen.Rendering.Materials;
using StudioFourteen.Rendering.Draw.Handles;
using StudioFourteen.Structs.Extensions;

public class TranslationGizmo : TransformGizmoBase
{
	private readonly AxisHandle xHandle;
	private readonly AxisHandle yHandle;
	private readonly AxisHandle zHandle;

	private readonly PlaneHandle xzPlaneHandle;
	private readonly PlaneHandle xyPlaneHandle;
	private readonly PlaneHandle zyPlaneHandle;

	private readonly LineRenderer<GizmoLineMaterial> changeLineRenderer;

	public TranslationGizmo()
	{
		this.xHandle = new(this);
		this.Add(this.xHandle);
		this.xHandle.Transform = Transform.FromRotation(0, 0, 0);
		this.xHandle.Color = Axes.XColor;
		this.xHandle.AxisUnit = Vector3.UnitX;

		this.yHandle = new(this);
		this.Add(this.yHandle);
		this.yHandle.Transform = Transform.FromRotation(0, 0, 90);
		this.yHandle.Color = Axes.YColor;
		this.yHandle.AxisUnit = Vector3.UnitY;

		this.zHandle = new(this);
		this.Add(this.zHandle);
		this.zHandle.Transform = Transform.FromRotation(-90, 0, 0);
		this.zHandle.Color = Axes.ZColor;
		this.zHandle.AxisUnit = Vector3.UnitZ;

		this.xzPlaneHandle = new(this);
		this.Add(this.xzPlaneHandle);
		this.xzPlaneHandle.Color = Axes.YColor;
		this.xzPlaneHandle.Axis1Unit = Vector3.UnitX;
		this.xzPlaneHandle.Axis2Unit = Vector3.UnitZ;

		this.xyPlaneHandle = new(this);
		this.Add(this.xyPlaneHandle);
		this.xyPlaneHandle.Color = Axes.ZColor;
		this.xyPlaneHandle.Axis1Unit = Vector3.UnitY;
		this.xyPlaneHandle.Axis2Unit = Vector3.UnitX;

		this.zyPlaneHandle = new(this);
		this.Add(this.zyPlaneHandle);
		this.zyPlaneHandle.Color = Axes.XColor;
		this.zyPlaneHandle.Axis1Unit = Vector3.UnitY;
		this.zyPlaneHandle.Axis2Unit = Vector3.UnitZ;

		this.changeLineRenderer = new();
		////this.Services.Rendering.Forward.Add(this.changeLineRenderer);
		this.changeLineRenderer.To = Vector3.Zero;
		this.changeLineRenderer.IsVisible = false;
		this.changeLineRenderer.IsHitTestVisible = false;
	}

	public override string Name => Resources.Find("LOC_Translate", "Translate");
	public override object? Icon => Resources.Find("ICON_Transform_Translate");
	public override bool ShowInControlPanel => false;
	public override bool KeepScreenSize => true;

	public float DepthOffset => 0.5f;

	public override bool IsBeingManipulated =>
		this.xHandle.IsHovered
		|| this.yHandle.IsHovered
		|| this.zHandle.IsHovered
		|| this.xzPlaneHandle.IsHovered
		|| this.xyPlaneHandle.IsHovered
		|| this.zyPlaneHandle.IsHovered;

	public bool IsDragging =>
		this.xHandle.IsDragging
		|| this.yHandle.IsDragging
		|| this.zHandle.IsDragging
		|| this.xzPlaneHandle.IsDragging
		|| this.xyPlaneHandle.IsDragging
		|| this.zyPlaneHandle.IsDragging;

	public Vector3 StartDragPosition { get; set; }

	public void GetToolTip(ref string content, ref Vector3 worldPosition)
	{
		Vector3 change = this.WorldPosition - this.StartDragPosition;
		content = $"{change.Length().ToString("F1")}m";
		worldPosition = this.StartDragPosition + (change / 2);
	}

	public void StartManipulation()
	{
		this.StartDragPosition = this.WorldPosition;
	}

	public void EndManipulation()
	{
		this.StartDragPosition = Vector3.Zero;
	}

	protected override void OnDraw()
	{
		base.OnDraw();

		this.changeLineRenderer.IsVisible = this.IsBeingManipulated && this.StartDragPosition != Vector3.Zero;
		this.changeLineRenderer.From = this.StartDragPosition;
		this.changeLineRenderer.To = this.WorldPosition;

		if (this.IsBeingManipulated)
			return;

		Vector3 lookVector = this.WorldPosition - this.CameraPosition;
		lookVector = Vector3.Normalize(lookVector);

		float x = Vector3.Dot(Vector3.Transform(Vector3.UnitX, this.WorldRotation), lookVector);
		float y = Vector3.Dot(Vector3.Transform(Vector3.UnitY, this.WorldRotation), lookVector);
		float z = Vector3.Dot(Vector3.Transform(Vector3.UnitZ, this.WorldRotation), lookVector);

		Vector3 scale = new(
			x > 0 ? -1 : 1,
			y > 0 ? -1 : 1,
			z > 0 ? -1 : 1);

		this.zyPlaneHandle.Axis1Flipped = y < 0;
		this.zyPlaneHandle.Axis2Flipped = z < 0;
		this.zyPlaneHandle.Transform = Transform.FromRotation(0, 0, 90) * Transform.FromScale(scale);

		this.xyPlaneHandle.Axis1Flipped = y < 0;
		this.xyPlaneHandle.Axis2Flipped = x < 0;
		this.xyPlaneHandle.Transform = Transform.FromRotation(90, 0, 90) * Transform.FromScale(scale);

		this.xzPlaneHandle.Axis1Flipped = x < 0;
		this.xzPlaneHandle.Axis2Flipped = z < 0;
		this.xzPlaneHandle.Transform = Transform.FromRotation(0, 0, 0) * Transform.FromScale(scale);
	}

	public class AxisHandle : Handle
	{
		private readonly MeshRenderer<GizmoFlatMaterial> coneRenderer;
		private readonly MeshRenderer<GizmoFlatOutlineMaterial> coneOutlineRenderer;
		private readonly LineRenderer<GizmoLineMaterial> lineRenderer;
		private readonly TranslationGizmo gizmo;
		private float alpha = 0;

		public AxisHandle(TranslationGizmo gizmo)
		{
			this.gizmo = gizmo;

			this.coneOutlineRenderer = new(MeshContent.Cone);
			this.coneOutlineRenderer.Transform = Transform.FromTRS(
				new(0.5f, 0, 0),
				Quaternion.CreateFromYawPitchRoll(0, 0, -90 * QuaternionExtensions.Deg2Rad),
				new(0.15f, 0.15f, 0.15f));
			this.coneOutlineRenderer.Material.OutlineColor = Axes.OutlineColor;
			this.Add(this.coneOutlineRenderer);

			this.coneRenderer = new(MeshContent.Cone);
			this.coneRenderer.Transform = Transform.FromTRS(
				new(0.5f, 0, 0),
				Quaternion.CreateFromYawPitchRoll(0, 0, -90 * QuaternionExtensions.Deg2Rad),
				new(0.15f, 0.15f, 0.15f));
			this.Add(this.coneRenderer);

			this.coneRenderer.Material.DepthOffset = this.gizmo.DepthOffset;
			this.coneRenderer.HitTestBias = 10;

			this.lineRenderer = new();
			this.lineRenderer.From = Vector3.UnitX * 0.05f;
			this.lineRenderer.To = Vector3.UnitX * 0.45f;
			this.lineRenderer.Material.OutlineColor = Axes.OutlineColor;
			this.lineRenderer.IsHitTestVisible = false;
			this.lineRenderer.Material.DepthOffset = this.gizmo.DepthOffset;
			this.lineRenderer.HitTestBias = 10;
			this.Add(this.lineRenderer);
		}

		public Color Color { get; set; }
		public float Sensitivity { get; set; } = 1.0f;
		public Vector3 AxisUnit { get; set; }

		public override bool GetToolTip(ref string content, ref Vector3 worldPosition)
		{
			if (!this.IsDragging)
				return false;

			this.gizmo.GetToolTip(ref content, ref worldPosition);
			return true;
		}

		protected override void OnDraw()
		{
			base.OnDraw();

			float desiredAlpha = 1.0f;
			if (this.gizmo.IsDragging)
				desiredAlpha = 0;

			if (this.IsPressed)
			{
				this.lineRenderer.Material.Color = Color.White;
				this.coneRenderer.Material.Color = Color.White;
				desiredAlpha = 0.5f;
			}
			else
			{
				this.lineRenderer.Material.Color = this.Color;
				this.coneRenderer.Material.Color = this.Color;
			}

			if (this.IsPressed)
			{
				this.lineRenderer.Material.Thickness = 1.0f;
			}
			else if (this.IsHovered)
			{
				this.lineRenderer.Material.Thickness = 1.5f;
			}
			else
			{
				this.lineRenderer.Material.Thickness = 1.0f;
			}

			this.alpha = float.Lerp(this.alpha, desiredAlpha, 0.25f);
			this.lineRenderer.Material.Color.A = this.alpha;
			this.coneRenderer.Material.Color.A = this.alpha;
			this.coneOutlineRenderer.Material.OutlineColor.A = this.alpha;
		}

		protected override void OnStartDrag(HitTestResult hitTest)
		{
			this.gizmo.StartManipulation();
		}

		protected override void OnDrag(Vector2 delta)
		{
			float mag = delta.Length();
			delta = Vector2.Normalize(delta);

			float dot = Vector2.Dot(delta, this.GetScreenVector(Vector3.UnitX));
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
			this.gizmo.TargetTransform = Transform.FromTranslation(-move) * this.gizmo.TargetTransform;
			base.OnDrag(delta);
		}

		protected override void OnEndDrag()
		{
			this.gizmo.EndManipulation();
			base.OnEndDrag();
		}
	}

	public class PlaneHandle : Handle
	{
		private readonly MeshRenderer<GizmoFlatMaterial> planeRenderer;
		private readonly TranslationGizmo gizmo;
		private float alpha;

		public PlaneHandle(TranslationGizmo gizmo)
		{
			this.gizmo = gizmo;

			this.planeRenderer = new(MeshContent.Plane);
			this.planeRenderer.Transform = Transform.FromTRS(
				new(0.1f, 0, 0.1f),
				Quaternion.Identity,
				new(0.1f, 0.1f, 0.1f));
			this.planeRenderer.CullMode = CullMode.None;
			this.planeRenderer.Material.DepthOffset = this.gizmo.DepthOffset;
			this.planeRenderer.HitTestBias = 10;
			this.Add(this.planeRenderer);
		}

		public Color Color { get; set; }
		public float Sensitivity { get; set; } = 1.0f;
		public Vector3 Axis1Unit { get; set; }
		public Vector3 Axis2Unit { get; set; }
		public bool Axis1Flipped { get; set; } = false;
		public bool Axis2Flipped { get; set; } = false;

		public override bool GetToolTip(ref string content, ref Vector3 worldPosition)
		{
			if (!this.IsDragging)
				return false;

			this.gizmo.GetToolTip(ref content, ref worldPosition);
			return true;
		}

		protected override void OnDraw()
		{
			base.OnDraw();

			float desiredAlpha = 0.5f;
			if (this.gizmo.IsDragging)
				desiredAlpha = 0;

			if (this.IsPressed)
			{
				this.planeRenderer.Material.Color = Color.White;
				desiredAlpha = 0.5f;
			}
			else if (this.IsHovered)
			{
				this.planeRenderer.Material.Color = this.Color;
				desiredAlpha = 0.75f;
			}
			else
			{
				this.planeRenderer.Material.Color = this.Color;
			}

			this.alpha = float.Lerp(this.alpha, desiredAlpha, 0.25f);
			this.planeRenderer.Material.Color.A = this.alpha;
		}

		protected override void OnStartDrag(HitTestResult hitTest)
		{
			this.gizmo.StartManipulation();
		}

		protected override void OnDrag(Vector2 delta)
		{
			float mag = delta.Length();
			delta = Vector2.Normalize(delta);

			float multiplier = this.Sensitivity;

			if (this.Services.Input.FastChange)
				multiplier *= 10;

			if (this.Services.Input.SlowChange)
				multiplier /= 10;

			if (this.Services.Tablet.PenPressure > 0)
				multiplier *= (float)this.Services.Tablet.PenPressure;

			Vector2 a = this.GetScreenVector(this.Axis1Flipped ? Vector3.UnitX : -Vector3.UnitX);
			Vector2 b = this.GetScreenVector(this.Axis2Flipped ? Vector3.UnitZ : -Vector3.UnitZ);

			float dot = Vector2.Dot(delta, a);
			float dragDelta = (float)(mag * dot);
			float change = dragDelta / 50;

			Vector3 move = this.Axis1Unit * change * multiplier;

			float dot2 = Vector2.Dot(delta, b);
			float drag2Delta = (float)(mag * dot2);
			float change2 = drag2Delta / 50;

			move += this.Axis2Unit * change2 * multiplier;

			this.gizmo.TargetTransform = Transform.FromTranslation(-move) * this.gizmo.TargetTransform;
			base.OnDrag(delta);
		}

		protected override void OnEndDrag()
		{
			Vector2 pos = this.GetScreenPosition(new(0.25f, 0, 0.25f));
			this.Services.Windows.SetCursorPosition(pos);

			this.gizmo.EndManipulation();

			base.OnEndDrag();
		}
	}
}