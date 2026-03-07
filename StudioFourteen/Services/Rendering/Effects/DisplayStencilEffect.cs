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

namespace StudioFourteen.Services.Rendering.Effects;

using System.Numerics;
using System.Runtime.InteropServices;
using SharpDX.D3DCompiler;
using StudioFourteen.Services.Content;
using StudioFourteen.Services.Rendering.Draw;
using StudioFourteen.Services.Rendering.Materials;

[StructLayout(LayoutKind.Sequential)]
public struct DisplayStencilEffect : IMaterial
{
	public Vector4 Unused;

	public IContent<ShaderBytecode>? GetVertexShader() => new ShaderReference("Shaders/Effect_DisplayStencil.hlsl", "vs_4_0", "vert");
	public IContent<ShaderBytecode>? GetPixelShader() => new ShaderReference("Shaders/Effect_DisplayStencil.hlsl", "ps_4_0", "pixel");
	public IContent<ShaderBytecode>? GetGeometryShader() => null;

	public void Initialize()
	{
	}
}