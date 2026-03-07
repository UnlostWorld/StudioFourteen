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

namespace StudioFourteen.Services.Rendering.Materials;

using System.Runtime.InteropServices;
using SharpDX.D3DCompiler;
using StudioFourteen.Services.Content;
using StudioFourteen.Services.Rendering.Draw;

[StructLayout(LayoutKind.Sequential)]
public struct LineMaterial : IMaterial
{
	public Color Color;
	public float DepthOffset;
	public float Unused1;
	public float Unused2;
	public float Unused3;

	public bool ShouldDraw => this.Color.A > 0.1f;

	public IContent<ShaderBytecode>? GetVertexShader() => new ShaderReference("Shaders/Line.hlsl", "vs_4_0", "vert");
	public IContent<ShaderBytecode>? GetPixelShader() => new ShaderReference("Shaders/Line.hlsl", "ps_4_0", "pixel");
	public IContent<ShaderBytecode>? GetGeometryShader() => new ShaderReference("Shaders/Line.hlsl", "gs_4_0", "geometry");

	public void Initialize()
	{
		this.Color = Color.White;
		this.DepthOffset = 0;
	}
}
