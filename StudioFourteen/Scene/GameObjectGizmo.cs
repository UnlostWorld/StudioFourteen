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

namespace StudioFourteen.Scene;

using SharpDX.Direct3D11;
using StudioFourteen.Services.Rendering;
using StudioFourteen.Services.Rendering.Draw;
using StudioFourteen.Services.Rendering.Draw.Gizmos;
using StudioFourteen.Services.Rendering.Materials;
using StudioFourteen.Services.Numerics;
using System;
using System.Numerics;

public class GameObjectGizmo : SceneObjectGizmoBase
{
	private readonly MeshRenderer<LineMaterial> boundsRenderer = new(MeshContent.WireCube);

	public GameObjectGizmo()
	{
		this.boundsRenderer.WriteDepth = false;
		this.boundsRenderer.CullMode = CullMode.None;
		this.boundsRenderer.IsVisible = false;
		this.boundsRenderer.IgnoreParentTransform = true;
		this.boundsRenderer.Material.DepthOffset = 0;
		this.Add(this.boundsRenderer);
	}

	public override bool KeepScreenSize => false;

	public GameObject GameObject
	{
		get
		{
			if (this.sceneObject is GameObject go)
				return go;

			throw new Exception($"Invalid GameObject.Gizmo target: {this.sceneObject}");
		}
	}

	protected override void OnDraw()
	{
		this.Transform = this.GameObject.WorldTransform;

		if (this.boundsRenderer.IsVisible)
		{
			Bounds bounds = this.GameObject.Bounds;

			this.boundsRenderer.Transform = StudioFourteen.Services.Numerics.Transform.FromTRS(
				bounds.Center,
				Quaternion.Identity,
				bounds.Extents);
		}

		base.OnDraw();
	}

	protected override void OnHovered(bool isHovered)
	{
		base.OnHovered(isHovered);
		this.boundsRenderer.IsVisible = isHovered;
	}
}