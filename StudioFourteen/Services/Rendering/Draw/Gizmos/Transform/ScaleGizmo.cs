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

namespace StudioFourteen.Services.Rendering.Draw.Gizmos.Transforms;

using System.Numerics;
using StudioFourteen.Services.Numerics;
using StudioFourteen.Services.Rendering.Draw.Handles;
using StudioFourteen.Services.Rendering.Materials;
using StudioFourteen.Services.Rendering.Passes;

public class ScaleGizmo : TransformGizmoBase
{
	private readonly AxisHandle xHandle;
	private readonly AxisHandle yHandle;
	private readonly AxisHandle zHandle;
	private readonly UniformHandle uniformHandle;

	public ScaleGizmo()
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

		this.uniformHandle = new(this);
		this.Add(this.uniformHandle);
	}

	public float DepthOffset => 0.5f;

	public override bool KeepScreenSize => true;

	public override bool IsBeingManipulated =>
		this.xHandle.IsHovered
		|| this.yHandle.IsHovered
		|| this.zHandle.IsHovered
		|| this.uniformHandle.IsHovered;

	public bool IsDragging =>
		this.xHandle.IsDragging
		|| this.yHandle.IsDragging
		|| this.zHandle.IsDragging
		|| this.uniformHandle.IsDragging;

	public override void Enable(ForwardPass? pass = null)
	{
		this.xHandle.Alpha = 0;
		this.yHandle.Alpha = 0;
		this.zHandle.Alpha = 0;
		this.uniformHandle.Alpha = 0;
		base.Enable(pass);
	}

	public class AxisHandle : Handle
	{
		private readonly MeshRenderer<GizmoFlatMaterial> cubeRenderer;
		private readonly MeshRenderer<GizmoFlatOutlineMaterial> cubeOutlineRenderer;
		private readonly LineRenderer<GizmoLineMaterial> lineRenderer;
		private readonly ScaleGizmo gizmo;

		public AxisHandle(ScaleGizmo gizmo)
		{
			this.gizmo = gizmo;

			this.cubeOutlineRenderer = new(MeshContent.Cube);
			this.cubeOutlineRenderer.Material.DepthOffset = this.gizmo.DepthOffset;
			this.cubeOutlineRenderer.Transform = Transform.FromTRS(
				new(0.5f, 0, 0),
				Quaternion.Identity,
				new(0.1f, 0.1f, 0.1f));
			this.cubeOutlineRenderer.Material.OutlineColor = Axes.OutlineColor;
			this.Add(this.cubeOutlineRenderer);

			this.cubeRenderer = new(MeshContent.Cube);
			this.cubeRenderer.Material.DepthOffset = this.gizmo.DepthOffset;
			this.cubeRenderer.HitTestBias = 10;
			this.cubeRenderer.Transform = Transform.FromTRS(
				new(0.5f, 0, 0),
				Quaternion.Identity,
				new(0.1f, 0.1f, 0.1f));
			this.Add(this.cubeRenderer);

			this.lineRenderer = new();
			this.lineRenderer.From = Vector3.UnitX * 0.1f;
			this.lineRenderer.To = Vector3.UnitX * 0.45f;
			this.lineRenderer.Material.OutlineColor = Axes.OutlineColor;
			this.lineRenderer.Material.DepthOffset = this.gizmo.DepthOffset;
			this.lineRenderer.IsHitTestVisible = false;
			this.Add(this.lineRenderer);
		}

		public Color Color { get; set; }
		public float Sensitivity { get; set; } = 1.0f;
		public Vector3 AxisUnit { get; set; }
		public float Alpha { get; set; } = 0;

		public override bool GetToolTip(ref string content, ref Vector3 worldPosition)
		{
			if (!this.IsDragging)
				return false;

			float scale = 1.0f;
			if (this.AxisUnit.X > 0)
			{
				scale = this.gizmo.TargetTransform.Scale.X;
			}
			else if (this.AxisUnit.Y > 0)
			{
				scale = this.gizmo.TargetTransform.Scale.Y;
			}
			else if (this.AxisUnit.Z > 0)
			{
				scale = this.gizmo.TargetTransform.Scale.Z;
			}

			scale *= 100;

			worldPosition = this.gizmo.WorldPosition;
			content = $"{scale.ToString("F1")}%";
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
				this.cubeRenderer.Material.Color = Color.White;
				desiredAlpha = 0.5f;
			}
			else
			{
				this.lineRenderer.Material.Color = this.Color;
				this.cubeRenderer.Material.Color = this.Color;
			}

			if (this.IsHovered)
			{
				this.lineRenderer.Material.Thickness = 1.5f;
				this.cubeOutlineRenderer.Transform = Transform.FromTRS(
					new(0.5f, 0, 0),
					Quaternion.Identity,
					new(0.12f, 0.12f, 0.12f));

				this.cubeRenderer.Transform = Transform.FromTRS(
					new(0.5f, 0, 0),
					Quaternion.Identity,
					new(0.12f, 0.12f, 0.12f));
			}
			else
			{
				this.lineRenderer.Material.Thickness = 1.0f;
				this.cubeOutlineRenderer.Transform = Transform.FromTRS(
					new(0.5f, 0, 0),
					Quaternion.Identity,
					new(0.1f, 0.1f, 0.1f));

				this.cubeRenderer.Transform = Transform.FromTRS(
					new(0.5f, 0, 0),
					Quaternion.Identity,
					new(0.1f, 0.1f, 0.1f));
			}

			this.Alpha = float.Lerp(this.Alpha, desiredAlpha, 0.25f);
			this.lineRenderer.Material.Color.A = this.Alpha;
			this.cubeRenderer.Material.Color.A = this.Alpha;
			this.cubeOutlineRenderer.Material.OutlineColor.A = this.Alpha;
		}

		protected override void OnStartDrag(HitTestResult hitTest)
		{
			base.OnStartDrag(hitTest);
		}

		protected override void OnDrag(Vector2 delta)
		{
			float mag = delta.Length();
			delta = Vector2.Normalize(delta);

			float dot = Vector2.Dot(delta, this.GetScreenVector(Vector3.UnitX));
			float dragDelta = (float)(mag * dot);
			float change = dragDelta / 500;
			change *= this.Sensitivity;

			/*if (InputService.FastChange)
				change *= 10;

			if (InputService.SlowChange)
				change /= 10;

			if (TabletService.PenPressure > 0)
				change *= (float)TabletService.PenPressure;*/

			Vector3 move = this.AxisUnit * change;

			if (float.IsNaN(move.X)
				|| float.IsNaN(move.Y)
				|| float.IsNaN(move.Z))
				return;

			this.gizmo.TargetTransform = Transform.FromScale(Vector3.One - move) * this.gizmo.TargetTransform;
			base.OnDrag(delta);
		}
	}

	public class UniformHandle : Handle
	{
		private readonly MeshRenderer<GizmoFlatMaterial> cubeRenderer;
		private readonly MeshRenderer<GizmoFlatOutlineMaterial> cubeOutlineRenderer;
		private readonly ScaleGizmo gizmo;

		public UniformHandle(ScaleGizmo gizmo)
		{
			this.gizmo = gizmo;

			this.cubeOutlineRenderer = new(MeshContent.Cube);
			this.cubeOutlineRenderer.Transform = Transform.FromScale(0.18f);
			this.cubeOutlineRenderer.Material.OutlineColor = Axes.OutlineColor;
			this.cubeOutlineRenderer.Material.DepthOffset = this.gizmo.DepthOffset;
			this.Add(this.cubeOutlineRenderer);

			this.cubeRenderer = new(MeshContent.Cube);
			this.cubeRenderer.Transform = Transform.FromScale(0.2f);
			this.cubeRenderer.Material.DepthOffset = this.gizmo.DepthOffset;
			this.cubeRenderer.HitTestBias = 10;
			this.Add(this.cubeRenderer);
		}

		public float Sensitivity { get; set; } = 1.0f;
		public float Alpha { get; set; } = 0;

		public override bool GetToolTip(ref string content, ref Vector3 worldPosition)
		{
			if (!this.IsDragging)
				return false;

			float scale = this.gizmo.TargetTransform.Scale.X;
			scale *= 100;

			worldPosition = this.gizmo.WorldPosition;
			content = $"{scale.ToString("F1")}%";
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
				this.cubeRenderer.Material.Color = Color.White;
				desiredAlpha = 0.5f;
			}
			else
			{
				this.cubeRenderer.Material.Color = new(0.25f, 0.25f, 0.25f, 1.0f);
			}

			if (this.IsHovered)
			{
				this.cubeRenderer.Transform = Transform.FromScale(0.22f);
				this.cubeOutlineRenderer.Transform = Transform.FromScale(0.20f);
			}
			else
			{
				this.cubeRenderer.Transform = Transform.FromScale(0.2f);
				this.cubeOutlineRenderer.Transform = Transform.FromScale(0.18f);
			}

			this.Alpha = float.Lerp(this.Alpha, desiredAlpha, 0.25f);
			this.cubeRenderer.Material.Color.A = this.Alpha;
			this.cubeOutlineRenderer.Material.OutlineColor.A = this.Alpha;
		}

		protected override void OnStartDrag(HitTestResult hitTest)
		{
			base.OnStartDrag(hitTest);
		}

		protected override void OnDrag(Vector2 delta)
		{
			float mag = delta.X + delta.Y;

			float change = mag / 500;
			change *= this.Sensitivity;

			/*if (InputService.FastChange)
				change *= 10;

			if (InputService.SlowChange)
				change /= 10;

			if (TabletService.PenPressure > 0)
				change *= (float)TabletService.PenPressure;*/

			Vector3 move = Vector3.One * change;

			if (float.IsNaN(move.X)
				|| float.IsNaN(move.Y)
				|| float.IsNaN(move.Z))
				return;

			this.gizmo.TargetTransform = Transform.FromScale(Vector3.One - move) * this.gizmo.TargetTransform;
			base.OnDrag(delta);
		}

		protected override void OnEndDrag()
		{
			////Vector2 pos = this.GetScreenPosition(Vector3.Zero);
			////WindowService.SetCursorPosition(pos);

			base.OnEndDrag();
		}
	}
}