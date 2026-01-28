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

namespace StudioFourteen.Services.Rendering.Draw.Gizmos;

using SharpDX.Direct3D11;
using StudioFourteen.Scene;
using StudioFourteen.Services.Rendering.Materials;
using System.Numerics;

public class GridGizmo : GizmoBase
{
	private readonly MeshRenderer<GridMaterial> gridRenderer = new(MeshContent.Plane);

	public GridGizmo()
	{
		this.gridRenderer.WriteDepth = false;
		this.gridRenderer.CullMode = CullMode.None;
		this.gridRenderer.Material.UseCameraPosition = 1;
		this.Add(this.gridRenderer);

		// TODO:
		this.IsHitTestVisible = false;
	}

	public override bool KeepScreenSize => false;
	public float Opacity { get; set; } = 1.0f;
	public bool KeepAtTargetHeight { get; set; } = true;
	public float Height { get; set; } = 0;

	protected unsafe override void OnDraw()
	{
		if (this.KeepAtTargetHeight)
		{
			if (Studio.Scene.PrimarySelection is Character target)
			{
				this.Height = Vector3.Transform(Vector3.Zero, target.WorldTransform.ToMatrix()).Y;
			}
		}

		this.gridRenderer.Material.Height = this.Height;
		this.gridRenderer.Material.Color.A = this.Opacity;
		this.gridRenderer.Material.XColor = Axes.XColor;
		this.gridRenderer.Material.ZColor = Axes.ZColor;

		base.OnDraw();
	}
}