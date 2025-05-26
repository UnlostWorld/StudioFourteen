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

namespace StudioFourteen.Rendering.Materials;

using System.Runtime.InteropServices;
using SharpDX.D3DCompiler;
using StudioFourteen.Content;
using StudioFourteen.Rendering.Scene;

[StructLayout(LayoutKind.Sequential)]
public struct BoneCapMaterial : IMaterial
{
	public Color Color;
	public float Size;
	public float DepthOffset;
	public float Unused2;
	public float Unused3;

	public IContent<ShaderBytecode>? GetVertexShader() => new ShaderReference("Shaders/BoneCap.hlsl", "vs_4_0", "vert");
	public IContent<ShaderBytecode>? GetPixelShader() => new ShaderReference("Shaders/BoneCap.hlsl", "ps_4_0", "pixel");
	public IContent<ShaderBytecode>? GetGeometryShader() => new ShaderReference("Shaders/BoneCap.hlsl", "gs_4_0", "geometry");

	public void Initialize()
	{
		this.Color = Color.White;
		this.Size = 1.0f;
		this.DepthOffset = 0.0f;
	}
}
