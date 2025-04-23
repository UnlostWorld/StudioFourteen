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

public class FlatCubeGeometry : GeometryBase
{
	protected override void Load(out Vertex[] vertices, out ushort[] indices)
	{
		vertices =
		[

			// Bottom
			new(new(-0.5f, -0.5f, -0.5f, 1), new(1, 0, 0, 1), new(0, 0)),
			new(new(-0.5f, -0.5f, +0.5f, 1), new(1, 0, 0, 1), new(1, 0)),
			new(new(+0.5f, -0.5f, +0.5f, 1), new(1, 0, 0, 1), new(1, 1)),
			new(new(+0.5f, -0.5f, -0.5f, 1), new(1, 0, 0, 1), new(0, 1)),

			// Top
			new(new(-0.5f, +0.5f, -0.5f, 1), new(1, 1, 0, 1), new(0, 0)),
			new(new(-0.5f, +0.5f, +0.5f, 1), new(1, 1, 0, 1), new(1, 0)),
			new(new(+0.5f, +0.5f, +0.5f, 1), new(1, 1, 0, 1), new(1, 1)),
			new(new(+0.5f, +0.5f, -0.5f, 1), new(1, 1, 0, 1), new(0, 1)),

			// Front
			new(new(+0.5f, -0.5f, -0.5f, 1), new(0, 1, 0, 1), new(0, 0)),
			new(new(+0.5f, -0.5f, +0.5f, 1), new(0, 1, 0, 1), new(1, 0)),
			new(new(+0.5f, +0.5f, +0.5f, 1), new(0, 1, 0, 1), new(1, 1)),
			new(new(+0.5f, +0.5f, -0.5f, 1), new(0, 1, 0, 1), new(0, 1)),

			// Back
			new(new(-0.5f, -0.5f, -0.5f, 1), new(0, 1, 1, 1), new(0, 0)),
			new(new(-0.5f, -0.5f, +0.5f, 1), new(0, 1, 1, 1), new(1, 0)),
			new(new(-0.5f, +0.5f, +0.5f, 1), new(0, 1, 1, 1), new(1, 1)),
			new(new(-0.5f, +0.5f, -0.5f, 1), new(0, 1, 1, 1), new(0, 1)),

			// Side L
			new(new(-0.5f, -0.5f, +0.5f, 1), new(0, 0, 1, 1), new(0, 0)),
			new(new(-0.5f, +0.5f, +0.5f, 1), new(0, 0, 1, 1), new(1, 0)),
			new(new(+0.5f, +0.5f, +0.5f, 1), new(0, 0, 1, 1), new(1, 1)),
			new(new(+0.5f, -0.5f, +0.5f, 1), new(0, 0, 1, 1), new(0, 1)),

			// Side R
			new(new(-0.5f, -0.5f, -0.5f, 1), new(0, 0, 1, 1), new(0, 0)),
			new(new(-0.5f, +0.5f, -0.5f, 1), new(0, 0, 1, 1), new(1, 0)),
			new(new(+0.5f, +0.5f, -0.5f, 1), new(0, 0, 1, 1), new(1, 1)),
			new(new(+0.5f, -0.5f, -0.5f, 1), new(0, 0, 1, 1), new(0, 1)),
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
			8, 9, 10,
			10, 11, 8,

			// Back
			14, 13, 12,
			12, 15, 14,

			// Side L
			16, 17, 18,
			18, 19, 16,

			// Side R
			22, 21, 20,
			20, 23, 22,
		];
	}
}