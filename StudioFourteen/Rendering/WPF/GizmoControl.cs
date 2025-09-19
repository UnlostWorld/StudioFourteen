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

namespace StudioFourteen.Rendering.WPF;

using System.Collections.Generic;
using System.Numerics;
using DependencyPropertyGenerator;
using SharpDX.Direct3D11;
using StudioFourteen.Rendering.Draw;
using StudioFourteen.Rendering.Draw.Gizmos;
using StudioFourteen.Rendering.Draw.Gizmos.Transforms;
using StudioFourteen.Rendering.Materials;
using StudioFourteen.Scene;
using StudioFourteen.Extensions;

[DependencyProperty<SceneObjectBase>("Target")]
public partial class GizmoControl : RendererElement
{
	private readonly GizmoOrbitCamera camera = new();
	private readonly TransformGizmo transform = new();
	private readonly Grid grid = new();

	public List<GizmoBase> Gizmos => this.transform.Gizmos;
	public GizmoBase? Current
	{
		get => this.transform.Current;
		set => this.transform.Current = value;
	}

	protected override RendererCamera Camera => this.camera;

	protected override void Initialize()
	{
		if (this.Renderer == null)
			return;

		this.Renderer.Forward.Add(this.grid);

		this.OnTargetChanged(this.Target);
	}

	partial void OnTargetChanged(SceneObjectBase? newValue)
	{
		this.camera.Target = newValue as TransformSceneObjectBase;
		this.grid.Target = newValue as TransformSceneObjectBase;

		if (this.Renderer == null)
			return;

		if (newValue != null)
		{
			this.transform.Enable(newValue, this.Renderer.Forward);
		}
		else
		{
			this.transform.Disable();
		}
	}

	public class Grid : DrawGroup
	{
		private readonly MeshRenderer<GridMaterial> gridRenderer = new(MeshContent.Plane);

		public Grid()
		{
			this.gridRenderer.WriteDepth = false;
			this.gridRenderer.CullMode = CullMode.None;
			this.Add(this.gridRenderer);
			this.IsHitTestVisible = false;
		}

		public TransformSceneObjectBase? Target { get; set; }

		protected unsafe override void OnDraw()
		{
			this.gridRenderer.Material.Height = 0;
			this.gridRenderer.Material.Color.A = 0.5f;
			this.gridRenderer.Material.LineThickness = 0.05f;
			this.gridRenderer.Material.XColor = Axes.XColor;
			this.gridRenderer.Material.ZColor = Axes.ZColor;

			if (this.Target != null)
			{
				Vector3 targetPos = Vector3.Transform(Vector3.Zero, this.Target.WorldTransform.ToMatrix());
				this.gridRenderer.Material.Height = targetPos.Y;
			}

			base.OnDraw();
		}
	}

	public class GizmoOrbitCamera : RendererCamera
	{
		public TransformSceneObjectBase? Target { get; set; }

		public override Matrix4x4 GetProjectionMatrix(Renderer renderer)
		{
			Matrix4x4 projection = Matrix4x4.CreatePerspectiveFieldOfView(0.52f, 1.0f, 0.1f, 10.0f);
			projection.M33 = 0;
			projection.M43 = 0.1f;
			return projection;
		}

		public override Matrix4x4 GetViewMatrix(Renderer renderer)
		{
			Vector3 forward = ServiceManager.Instance.Camera.CurrentForward;
			Vector3 cameraPosition = this.GetCameraPosition(renderer);
			Matrix4x4 view = Matrix4x4.CreateLookTo(cameraPosition, forward, Vector3.UnitY);
			view.M44 = 1;
			return view;
		}

		public override Vector3 GetCameraPosition(Renderer renderer)
		{
			Vector3 forward = ServiceManager.Instance.Camera.CurrentForward;
			return this.GetTargetPosition() - (forward * 3);
		}

		protected Vector3 GetTargetPosition()
		{
			if (this.Target == null)
				return Vector3.Zero;

			return Vector3.Transform(Vector3.Zero, this.Target.WorldTransform.ToMatrix());
		}
	}
}