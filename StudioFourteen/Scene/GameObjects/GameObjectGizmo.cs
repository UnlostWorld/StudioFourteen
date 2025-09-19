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

namespace StudioFourteen.Scene.GameObjects;

using StudioFourteen.Rendering;
using StudioFourteen.Rendering.Draw;
using StudioFourteen.Rendering.Draw.Gizmos;
using StudioFourteen.Rendering.Draw.Handles;
using StudioFourteen.Rendering.Materials;
using StudioFourteen.Scene.GameObjects.Characters;

public class GameObjectGizmo : SceneObjectGizmoBase<Character>
{
	private readonly GameObjectHandle handle;

	public GameObjectGizmo(GameObject gameObject)
	{
		this.handle = new GameObjectHandle(gameObject);
		this.Add(this.handle);

		this.Enable(gameObject);
	}

	public override string Name => "Character";
	public override bool KeepScreenSize => false;

	protected override void OnDraw()
	{
		this.handle.IsVisible = this.SceneObject?.IsSelected == false;
		base.OnDraw();
	}
}

public class GameObjectHandle : SelectionHandle
{
	private readonly MeshRenderer<GizmoLineMaterial> circleRenderer;

	public GameObjectHandle(SceneObjectBase selection)
		: base(selection)
	{
		this.circleRenderer = new(MeshContent.WireCircle);
		this.circleRenderer.Transform = Transform.FromScale(0.25f);
		this.circleRenderer.Material.EndCaps = 0;
		this.circleRenderer.Material.OutlineColor = Color.Transparent;
		this.circleRenderer.Material.MinAlpha = 0.1f;
		this.Add(this.circleRenderer);
	}

	protected override void OnDraw()
	{
		if (this.Selection is GameObject go)
			this.Transform = go.WorldTransform;

		this.circleRenderer.Material.Thickness = this.IsHovered ? 1.5f : 1.0f;

		base.OnDraw();
	}
}