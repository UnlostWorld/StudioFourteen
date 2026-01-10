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
public struct PhotoGuidesEffect : IMaterial
{
	public float LeftRight;
	public float TopBottom;
	public uint GuidesMode;
	public float Unused4;

	public enum GuideModes : uint
	{
		None,
		Thirds,
		Crosshair,
	}

	public IContent<ShaderBytecode>? GetVertexShader() => new ShaderReference("Shaders/Blit_PhotoGuides.hlsl", "vs_4_0", "vert");
	public IContent<ShaderBytecode>? GetPixelShader() => new ShaderReference("Shaders/Blit_PhotoGuides.hlsl", "ps_4_0", "pixel");
	public IContent<ShaderBytecode>? GetGeometryShader() => null;

	public void Initialize()
	{
		this.LeftRight = 0;
		this.TopBottom = 0;
		this.GuidesMode = 0;
	}
}