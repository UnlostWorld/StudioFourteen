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

using StudioFourteen.Content;
using StudioFourteen.Rendering.Materials;
using StudioFourteen.Rendering.MeshGenerators;

public static class Meshes
{
	public static readonly IContent<Mesh> Cube = new JsonContentReference<Mesh>("Meshes/Cube.jsonc");
	public static readonly IContent<Mesh> FlatCube = new JsonContentReference<Mesh>("Meshes/FlatCube.jsonc");
	public static readonly IContent<Mesh> Quad = new JsonContentReference<Mesh>("Meshes/Quad.jsonc");
	public static readonly IContent<Mesh> Plane = new JsonContentReference<Mesh>("Meshes/Plane.jsonc");
	public static readonly IContent<Mesh> WireCube = new JsonContentReference<Mesh>("Meshes/WireCube.jsonc");
	public static readonly IContent<Mesh> WireCircle = new WireCircle();
	public static readonly IContent<Mesh> Bone = new JsonContentReference<Mesh>("Meshes/Bone.jsonc");
}

public static class Material
{
	public static readonly BlitMaterial Blit = new();
	public static readonly BlitAlphaMaskMaterial BlitAlphaMask = new();
	public static readonly VertexColor GeometryVertexColor = new();
	public static readonly LineMaterial Line = new();
	public static readonly DotMaterial Dot = new();
}