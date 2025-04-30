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

using StudioFourteen.Rendering.Geometries;
using StudioFourteen.Rendering.Materials;
using StudioFourteen.Rendering.Meshes;

public static class Geometry
{
	public static readonly GeometryBase Cube = new EmbeddedGeometry("Cube.jsonc");
	public static readonly GeometryBase FlatCube = new EmbeddedGeometry("FlatCube.jsonc");
	public static readonly GeometryBase Quad = new EmbeddedGeometry("Quad.jsonc");
	public static readonly GeometryBase Plane = new EmbeddedGeometry("Plane.jsonc");
	public static readonly GeometryBase WireCube = new EmbeddedGeometry("WireCube.jsonc");
	public static readonly GeometryBase WireCircle = new GeneratedGeometry<WireCircle>();
}

public static class Material
{
	public static readonly BlitMaterial Blit = new();
	public static readonly BlitAlphaMaskMaterial BlitAlphaMask = new();
	public static readonly VertexColor GeometryVertexColor = new();
	public static readonly LineMaterial Line = new();
}