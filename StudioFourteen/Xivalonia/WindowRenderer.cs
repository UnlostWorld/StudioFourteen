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

namespace StudioFourteen.Xivalonia;

using System.Numerics;
using System.Runtime.InteropServices;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;
using StudioFourteen.Content;
using StudioFourteen.Rendering;
using StudioFourteen.Rendering.Draw;
using StudioFourteen.Rendering.Materials;
using StudioFourteen.Xivalonia.Platform;

public partial class WindowRenderer : MeshRenderer<WindowRenderer.XivaloniaUiMaterial>
{
	private readonly WindowImpl window;
	private readonly DxgiSurface surface;

	private ShaderResourceView? bufferResourceView;

	public WindowRenderer(WindowImpl window, DxgiSurface surface)
		: base(MeshContent.Quad)
	{
		this.window = window;
		this.surface = surface;

		this.IsVisible = true;
		this.CullMode = SharpDX.Direct3D11.CullMode.None;
		this.WriteDepth = false;
		this.StencilMode = SharpDX.Direct3D11.Comparison.Always;

		this.Transform = StudioFourteen.Transform.FromTRS(
			new Vector3(0, 0, 0),
			Quaternion.Identity,
			new Vector3(0.15f, 0.25f, 1));
	}

	public override void Draw(Renderer renderer, Transform transform, Device device, DeviceContext deviceContext)
	{
		if (this.surface.DxgiRenderTarget?.Texture == null)
			return;

		if (this.bufferResourceView == null)
			this.bufferResourceView = new(device, this.surface.DxgiRenderTarget.Texture);

		deviceContext.PixelShader.SetShaderResource(3, this.bufferResourceView);

		base.Draw(renderer, transform, device, deviceContext);
	}

	public override void Dispose()
	{
		this.bufferResourceView?.Dispose();
		this.bufferResourceView = null;

		base.Dispose();
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct XivaloniaUiMaterial : IMaterial
	{
		public Vector4 Unused;

		public IContent<ShaderBytecode>? GetVertexShader()
			=> new ShaderReference("Shaders/AvaloniaUiComp.hlsl", "vs_4_0", "vert");
		public IContent<ShaderBytecode>? GetPixelShader()
			=> new ShaderReference("Shaders/AvaloniaUiComp.hlsl", "ps_4_0", "pixel");
		public IContent<ShaderBytecode>? GetGeometryShader()
			=> null;

		public void Initialize()
		{
		}
	}
}