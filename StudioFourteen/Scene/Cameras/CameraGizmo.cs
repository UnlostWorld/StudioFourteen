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

namespace StudioFourteen.Scene.Cameras;

using System.Numerics;
using StudioFourteen.Content;
using StudioFourteen.Rendering;
using StudioFourteen.Rendering.Draw;
using StudioFourteen.Rendering.Draw.Gizmos;
using StudioFourteen.Rendering.Materials;

public class CameraGizmo : SceneObjectGizmoBase<Camera>
{
	public static readonly IContent<Mesh> CameraMesh = new JsonContentReference<Mesh>("Meshes/Camera.jsonc");

	private readonly MeshRenderer<GizmoLineMaterial> cameraRenderer;

	public CameraGizmo(Camera camera)
	{
		this.SetTarget(camera);

		this.cameraRenderer = new(CameraMesh);
		this.cameraRenderer.Material.EndCaps = 0;
		this.Add(this.cameraRenderer);

		this.Enable();
	}

	public override string Name => "Camera";
	public override bool KeepScreenSize => false;

	protected override void OnDraw()
	{
		if (this.SceneObject == null)
			return;

		// Don't draw the gizmo for the active camera since its going to clip
		// the view.
		this.IsVisible = !this.SceneObject.IsActive;

		CameraState state = this.SceneObject.LastState;
		this.Transform = Transform.FromTRS(state.Position, state.Rotation, new Vector3(0.3f));

		base.OnDraw();
	}
}