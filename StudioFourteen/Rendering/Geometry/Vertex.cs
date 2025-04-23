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

using System.Numerics;
using System.Runtime.InteropServices;
using SharpDX.Direct3D11;

[StructLayout(LayoutKind.Sequential)]
public struct Vertex
{
	public Vector4 Position;
	public Vector4 Color;

	public Vertex(Vector4 position, Vector4 color)
    {
        this.Position = position;
        this.Color = color;
    }

	public Vertex(Vector4 position)
    {
        this.Position = position;
        this.Color = Vector4.One;
    }

	public Vertex(float x, float y, float z, float w = 1.0f)
    {
        this.Position = new (x, y, z, w);
        this.Color = Vector4.One;
    }

	public Vertex(float x, float y, float z, float r, float g, float b)
    {
        this.Position = new(x, y, z, 1.0f);
        this.Color = new(r, g, b, 1.0f);
    }

	public InputElement[] GetInputElements() =>
	[
		new InputElement("POSITION", 0, SharpDX.DXGI.Format.R32G32B32_Float, 0, 0, InputClassification.PerVertexData, 0),
		new InputElement("COLOR", 0, SharpDX.DXGI.Format.R32G32B32_Float, 16, 0, InputClassification.PerVertexData, 0),
    ];
}