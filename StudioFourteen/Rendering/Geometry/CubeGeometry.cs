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

public class CubeGeometry : GeometryBase
{
	protected override void Load(out Vertex[] vertices, out ushort[] indices)
	{
		vertices =
		[

			// Bottom
			new(new(-0.5f, -0.5f, -0.5f, 1), new(1, 0, 0, 1), new(0, 0)),
			new(new(-0.5f, -0.5f, +0.5f, 1), new(1, 1, 0, 1), new(1, 0)),
			new(new(+0.5f, -0.5f, +0.5f, 1), new(0, 1, 0, 1), new(1, 1)),
			new(new(+0.5f, -0.5f, -0.5f, 1), new(0, 1, 1, 1), new(0, 1)),

			// Top
			new(new(-0.5f, +0.5f, -0.5f, 1), new(0, 0, 1, 1), new(0, 0)),
			new(new(-0.5f, +0.5f, +0.5f, 1), new(1, 0, 1, 1), new(1, 0)),
			new(new(+0.5f, +0.5f, +0.5f, 1), new(1, 0, 0, 1), new(1, 1)),
			new(new(+0.5f, +0.5f, -0.5f, 1), new(1, 1, 0, 1), new(0, 1)),

		];

		indices =
		[

			// Bottom
			0, 1, 2,
			2, 3, 0,

			// Top
			6, 5, 4,
			4, 7, 6,

			// Front
			3, 2, 6,
			6, 7, 3,

			// Back
			5, 1, 0,
			0, 4, 5,

			// Side L
			1, 5, 6,
			6, 2, 1,

			// Side R
			7, 4, 0,
			0, 3, 7,
		];
	}
}