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

using System.Numerics;
using System.Runtime.InteropServices;
using SharpDX.Direct3D11;

[StructLayout(LayoutKind.Sequential)]
public struct Vertex
{
	public Vector4 Position;
	public Color Color = Color.White;
	public Vector2 TexCoord;
	public Vector4 Normal = Vector4.UnitY;

	public Vertex()
	{
	}

	public Vertex(Vector4 position)
	{
		this.Position = position;
	}

	public Vertex(Vector4 position, Color color)
	{
		this.Position = position;
		this.Color = color;
	}

	public Vertex(Vector4 position, Color color, Vector2 texCoord)
	{
		this.Position = position;
		this.Color = color;
		this.TexCoord = texCoord;
	}

	public InputElement[] GetInputElements() =>
	[
		new InputElement("POSITION", 0, SharpDX.DXGI.Format.R32G32B32A32_Float, 0, 0, InputClassification.PerVertexData, 0),
		new InputElement("COLOR", 0, SharpDX.DXGI.Format.R32G32B32A32_Float, InputElement.AppendAligned, 0, InputClassification.PerVertexData, 0),
		new InputElement("TEXCOORD", 0, SharpDX.DXGI.Format.R32G32_Float, InputElement.AppendAligned, 0, InputClassification.PerVertexData, 0),
		new InputElement("NORMAL", 0, SharpDX.DXGI.Format.R32G32B32A32_Float, InputElement.AppendAligned, 0, InputClassification.PerVertexData, 0),
	];
}