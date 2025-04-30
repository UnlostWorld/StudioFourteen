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

namespace StudioFourteen.Rendering;

using StudioFourteen.Rendering.Materials;
using StudioFourteen.Rendering.MeshGenerators;

public static class Meshes
{
	public static readonly Mesh Cube = Mesh.LoadEmbedded("Cube.jsonc");
	public static readonly Mesh FlatCube = Mesh.LoadEmbedded("FlatCube.jsonc");
	public static readonly Mesh Quad = Mesh.LoadEmbedded("Quad.jsonc");
	public static readonly Mesh Plane = Mesh.LoadEmbedded("Plane.jsonc");
	public static readonly Mesh WireCube = Mesh.LoadEmbedded("WireCube.jsonc");
	public static readonly Mesh WireCircle = new WireCircle();
}

public static class Material
{
	public static readonly BlitMaterial Blit = new();
	public static readonly BlitAlphaMaskMaterial BlitAlphaMask = new();
	public static readonly VertexColor GeometryVertexColor = new();
	public static readonly LineMaterial Line = new();
}