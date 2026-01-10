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
public struct GizmoLineMaterial : IMaterial
{
	public Color Color;
	public Color OutlineColor;
	public float Thickness;
	public float EndCaps;
	public float FadeOutDepth;
	public float MinAlpha;
	public float DepthOffset;
	public float Unused1;
	public float Unused2;
	public float Unused3;

	public bool ShouldDraw => this.Color.A > 0.1f;

	public IContent<ShaderBytecode>? GetVertexShader() => new ShaderReference("Shaders/GizmoLine.hlsl", "vs_4_0", "vert");
	public IContent<ShaderBytecode>? GetPixelShader() => new ShaderReference("Shaders/GizmoLine.hlsl", "ps_4_0", "pixel");
	public IContent<ShaderBytecode>? GetGeometryShader() => new ShaderReference("Shaders/GizmoLine.hlsl", "gs_4_0", "geometry");

	public void Initialize()
	{
		this.Color = Color.White;
		this.Thickness = 1.0f;
		this.EndCaps = 1.0f;
		this.OutlineColor = Color.Black;
		this.FadeOutDepth = 0;
		this.MinAlpha = 1;
		this.DepthOffset = 0;
	}
}
