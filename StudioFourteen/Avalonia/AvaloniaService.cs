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

namespace StudioFourteen.Avalonia;

using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using SharpDX.D3DCompiler;
using StudioFourteen.Content;
using StudioFourteen.Rendering;
using StudioFourteen.Rendering.Draw;
using StudioFourteen.Rendering.Materials;
using StudioFourteen.Services;

[Service]
public partial class AvaloniaService : ServiceBase
{
	private readonly MeshRenderer<AvaloniaUiMaterial> windowRenderer = new(MeshContent.Quad);

	public override Task Start()
	{
		this.windowRenderer.IsVisible = true;
		this.windowRenderer.CullMode = SharpDX.Direct3D11.CullMode.None;
		this.windowRenderer.WriteDepth = false;
		this.windowRenderer.StencilMode = SharpDX.Direct3D11.Comparison.Always;

		this.windowRenderer.Transform = Transform.FromTRS(new Vector3(0, 0, 0), Quaternion.Identity, new Vector3(0.25f, 0.25f, 1));

		this.Services.Rendering.OverlayRenderer.Interface.Add(this.windowRenderer);

		return base.Start();
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct AvaloniaUiMaterial : IMaterial
	{
		public Vector4 Unused;

		public IContent<ShaderBytecode>? GetVertexShader() => new ShaderReference("Shaders/AvaloniaUiComp.hlsl", "vs_4_0", "vert");
		public IContent<ShaderBytecode>? GetPixelShader() => new ShaderReference("Shaders/AvaloniaUiComp.hlsl", "ps_4_0", "pixel");
		public IContent<ShaderBytecode>? GetGeometryShader() => null;

		public void Initialize()
		{
		}
	}
}