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
using StudioFourteen.Rendering.Draw;

[StructLayout(LayoutKind.Sequential)]
public struct GridMaterial : IMaterial
{
	public Color Color;
	public Color XColor;
	public Color ZColor;
	public float GridSize;
	public float LineThickness;
	public float Height;
	public float UseCameraPosition;
	public IContent<ShaderBytecode>? GetVertexShader() => new ShaderReference("Shaders/Grid.hlsl", "vs_4_0", "vert");
	public IContent<ShaderBytecode>? GetPixelShader() => new ShaderReference("Shaders/Grid.hlsl", "ps_4_0", "pixel");
	public IContent<ShaderBytecode>? GetGeometryShader() => null;

	public void Initialize()
	{
		this.Color = Color.White;
		this.GridSize = 1.0f;
		this.LineThickness = 0.1f;
		this.Height = 0;
		this.XColor = Axes.XColor;
		this.ZColor = Axes.ZColor;
		this.UseCameraPosition = 0;
	}
}