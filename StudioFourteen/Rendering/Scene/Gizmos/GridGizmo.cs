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

namespace StudioFourteen.Rendering.Scene.Gizmos;

using FFXIVClientStructs.FFXIV.Client.Game.Character;
using SharpDX.Direct3D11;
using StudioFourteen.Rendering.Materials;
using StudioFourteen.Rendering.Scene;

public class GridGizmo : GizmoBase
{
	private readonly MeshRenderer<GridMaterial> gridRenderer = new(MeshContent.Plane);

	public GridGizmo()
	{
		this.Add(this.gridRenderer);

		// TODO:
		this.IsHitTestVisible = false;
	}

	public override string Name => "Grid";

	public float Opacity
	{
		get => this.GetPersistence<float>(defaultValue: 0.75f);
		set => this.SetPersistence(value);
	}

	public bool KeepAtTargetHeight
	{
		get => this.GetPersistence<bool>();
		set => this.SetPersistence(value);
	}

	public float Height
	{
		get => this.GetPersistence<float>();
		set => this.SetPersistence(value);
	}

	public unsafe override void Draw(Transform transform, Device device, DeviceContext deviceContext)
	{
		if (this.KeepAtTargetHeight)
		{
			Character* target = this.Services.Target.GetTarget();
			if (target != null && target->DrawObject != null)
			{
				this.gridRenderer.Material.Height = target->DrawObject->Position.Y;
			}
		}

		base.Draw(transform, device, deviceContext);
	}

	protected override void OnPersistenceChanged()
	{
		base.OnPersistenceChanged();

		this.gridRenderer.Material.Color.A = this.Opacity;

		if (!this.KeepAtTargetHeight)
		{
			this.gridRenderer.Material.Height = this.Height;
		}
	}
}