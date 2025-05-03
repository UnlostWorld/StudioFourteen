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

namespace StudioFourteen.Rendering.Passes;

using FFXIVClientStructs.FFXIV.Client.Graphics.Render;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using StudioFourteen.Rendering.Scene;

using Device = SharpDX.Direct3D11.Device;
using Material = StudioFourteen.Rendering.Material;

public class GenerateMaskDepthPass : RenderPassBase
{
	private readonly MeshRenderer quad = new(Meshes.Quad, Material.BlitAlphaMask);

	private Texture2D? backBufferCopyTexture;
	private ShaderResourceView? backBufferResourceView;
	private Texture2D? depthStencilTexture;
	private ShaderResourceView? depthResourceView;
	private Texture2D? maskTexture;
	private RenderTargetView? maskRenderTargetView;
	private ShaderResourceView? maskResourceView;

	public override void OnResolutionChanged()
	{
		this.maskTexture?.Dispose();
		this.maskTexture = null;

		this.depthStencilTexture = null;

		this.backBufferResourceView?.Dispose();
		this.backBufferResourceView = null;

		this.backBufferCopyTexture?.Dispose();
		this.backBufferCopyTexture = null;

		base.OnResolutionChanged();
	}

	public unsafe override void Render(RenderingService service, Device device, DeviceContext deviceContext)
	{
		if (service.BackBuffer == null)
			return;

		RenderTargetManagerEx* pRenderTargetManager = RenderTargetManagerEx.Instance();
		if (pRenderTargetManager == null)
			return;

		// Create a shader resource copy of the back buffer so it can be accessed in the shader
		if (this.backBufferCopyTexture == null)
		{
			this.backBufferCopyTexture?.Dispose();
			this.backBufferResourceView?.Dispose();

			Texture2DDescription desc = service.BackBuffer.Description;
			desc.BindFlags = BindFlags.ShaderResource;
			this.backBufferCopyTexture = new Texture2D(device, desc);

			this.backBufferResourceView = new(device, this.backBufferCopyTexture);
		}

		// Create a handle to the depth stencil
		if (this.depthStencilTexture == null && pRenderTargetManager->DepthStencil != null)
		{
			this.depthStencilTexture = new((nint)pRenderTargetManager->DepthStencil->D3D11Texture2D);
			////this.depthStencilTexture = new(service.Services.Reshade.DepthBufferAddress);
			this.depthResourceView = new(device, this.depthStencilTexture);
		}

		// Create an output texture
		if (this.maskTexture == null)
		{
			this.maskTexture?.Dispose();
			this.maskRenderTargetView?.Dispose();
			this.maskResourceView?.Dispose();

			Texture2DDescription desc = service.BackBuffer.Description;
			desc.BindFlags = BindFlags.ShaderResource | BindFlags.RenderTarget;
			this.maskTexture = new(device, desc);

			RenderTargetViewDescription rtDesc = default;
			rtDesc.Format = Format.R8G8B8A8_UNorm;
			rtDesc.Dimension = RenderTargetViewDimension.Texture2D;
			rtDesc.Texture2D = new() { };
			this.maskRenderTargetView = new(device, this.maskTexture, rtDesc);

			this.maskResourceView = new(device, this.maskTexture);
		}

		// Copy the back buffer into the copy
		deviceContext.CopyResource(service.BackBuffer, this.backBufferCopyTexture);

		// Set the output target to the new buffer
		deviceContext.OutputMerger.SetTargets(this.maskRenderTargetView);

		// Pass the buffers into the shader
		deviceContext.PixelShader.SetShaderResource(0, this.backBufferResourceView);

		deviceContext.Rasterizer.SetViewport(0, 0, service.Width, service.Height);

		this.quad.Draw(Transform.Identity, device, deviceContext);

		using CommandList cmds = deviceContext.FinishCommandList(false);
		device.ImmediateContext.ExecuteCommandList(cmds, true);
		deviceContext.ClearState();

		// Pass the mask depth into future shaders
		deviceContext.PixelShader.SetShaderResource(0, this.maskResourceView);
		deviceContext.PixelShader.SetShaderResource(1, this.depthResourceView);
	}

	public override void Dispose()
	{
		this.quad.Dispose();
		this.backBufferCopyTexture?.Dispose();
		this.depthStencilTexture?.Dispose();
		this.maskTexture?.Dispose();
		this.maskRenderTargetView?.Dispose();
		this.maskResourceView?.Dispose();
		this.depthResourceView?.Dispose();
		this.depthStencilTexture?.Dispose();
		base.Dispose();
	}
}