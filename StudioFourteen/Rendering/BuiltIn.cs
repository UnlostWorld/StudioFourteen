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
	public static readonly EmbeddedGeometry Cube = new("Cube.jsonc");
	public static readonly EmbeddedGeometry FlatCube = new("FlatCube.jsonc");
	public static readonly EmbeddedGeometry Quad = new("Quad.jsonc");
	public static readonly EmbeddedGeometry WireCube = new("WireCube.jsonc");
	public static readonly GeneratedGeometry<WireCircle> WireCircle = new();
}

public static class Material
{
	public static readonly EmbeddedMaterial Blit = new("Blit_Copy.hlsl");
	public static readonly EmbeddedMaterial BlitAlphaMask = new("Blit_AlphaMask.hlsl");
	public static readonly EmbeddedMaterial GeometryVertexColor = new("Geometry_VertexColor.hlsl");
	public static readonly EmbeddedMaterial Line = new("Line.hlsl", true);
}