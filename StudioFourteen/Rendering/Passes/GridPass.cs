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

namespace StudioFourteen.Rendering.Passes;

using System.Numerics;
using SharpDX.Direct3D11;
using StudioFourteen.Rendering.Materials;

public class GridPass : GeometryPass
{
	private readonly DrawObject plane;
	private readonly EmbeddedMaterial gridMaterial = new("Grid.hlsl", false);

	public GridPass()
	{
		this.plane = new(this.gridMaterial, Geometry.Plane);
		this.Add(this.plane);
	}

	public override void Render(RenderingService service, Device device, DeviceContext deviceContext)
	{
		this.plane.Transform = Transform.FromTranslation(
			-603,
			30,
			-839);

		base.Render(service, device, deviceContext);
	}
}