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

namespace StudioFourteen.Rendering.Gizmos;

using StudioFourteen.Rendering.Materials;
using StudioFourteen.Rendering.Scene;

public class GridGizmo : GizmoBase
{
	private readonly MeshRenderer gridRenderer = new(Meshes.Plane, new GridMaterial());

	public GridGizmo()
	{
		this.Add(this.gridRenderer);
	}

	public override string Name => "Grid";

	public float Opacity
	{
		get => this.GetPersistence<float>(defaultValue: 0.75f);
		set => this.SetPersistence(value);
	}

	public float GridSize
	{
		get => this.GetPersistence<float>(defaultValue: 1.0f);
		set => this.SetPersistence(value);
	}

	public float LineThickness
	{
		get => this.GetPersistence<float>(defaultValue: 0.2f);
		set => this.SetPersistence(value);
	}

	public float Height
	{
		get => this.GetPersistence<float>();
		set => this.SetPersistence(value);
	}

	protected override void OnPersistenceChanged()
	{
		base.OnPersistenceChanged();

		ref GridMaterial.GridInstanceData data = ref this.gridRenderer.GetMaterialInstance<GridMaterial.GridInstanceData>();
		data.Color.A = this.Opacity;
		data.GridSize = this.GridSize;
		data.LineThickness = this.LineThickness;
		data.Height = this.Height;
	}
}