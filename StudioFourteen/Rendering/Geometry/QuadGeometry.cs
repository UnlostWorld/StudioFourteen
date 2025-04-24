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

namespace StudioFourteen.Rendering.Geometry;

public class QuadGeometry : GeometryBase
{
	protected override void Load(out Vertex[] vertices, out ushort[] indices)
	{
		vertices =
		[
			new (new(-1, -1, 0, 1), new(1, 1, 0, 1), new(0, 1)),
			new (new(+1, -1, 0, 1), new(0, 1, 0, 1), new(1, 1)),
			new (new(+1, +1, 0, 1), new(0, 1, 1, 1), new(1, 0)),
			new (new(-1, +1, 0, 1), new(0, 0, 1, 1), new(0, 0)),
		];

		indices =
		[
			0, 2, 1,
			0, 3, 2,
		];
	}
}