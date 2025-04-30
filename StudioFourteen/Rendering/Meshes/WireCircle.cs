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

namespace StudioFourteen.Rendering.MeshGenerators;

using System;
using System.Numerics;
using SharpDX.Direct3D;

public class WireCircle : Mesh
{
	private const int NumPoints = 144;

	public WireCircle()
	{
		this.Topology = PrimitiveTopology.LineStrip;

		for (int i = 0; i < NumPoints; i++)
		{
			float p = i / (float)(NumPoints - 1);
			float r = p * (MathF.PI * 2);

			Vector4 to = new Vector4(MathF.Cos(r), 0, MathF.Sin(r), 1);
			this.Vertices.Add(new Vertex(to));
		}
	}
}