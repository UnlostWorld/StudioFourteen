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
using StudioFourteen.Rendering.Materials;
using StudioFourteen.Rendering.Scene;

using Device = SharpDX.Direct3D11.Device;

public class GenerateUiMaskPass : RenderPassBase
{
	private readonly MeshRenderer<BlitAlphaMaskMaterial> quad = new(MeshContent.Quad);

	private Texture2D? backBufferCopyTexture;
	private ShaderResourceView? backBufferResourceView;
	private Texture2D? depthStencilTexture;
	private Texture2D? depthStencilCopyTexture;
	private ShaderResourceView? depthResourceView;
	private Texture2D? maskTexture;
	private RenderTargetView? maskRenderTargetView;
	private ShaderResourceView? maskResourceView;

	public override void OnResolutionChanged()
	{
		this.maskTexture?.Dispose();
		this.maskTexture = null;

		this.depthStencilCopyTexture?.Dispose();
		this.depthStencilCopyTexture = null;

		this.depthResourceView?.Dispose();
		this.depthResourceView = null;

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

		this.depthStencilTexture = (Texture2D)(nint)pRenderTargetManager->DepthStencil->D3D11Texture2D;

		// Create a handle to the depth stencil
		if (this.depthStencilCopyTexture == null)
		{
			this.depthStencilCopyTexture?.Dispose();
			this.depthStencilCopyTexture?.Dispose();

			Texture2DDescription desc = this.depthStencilTexture.Description;
			desc.BindFlags = BindFlags.ShaderResource;
			desc.Format = Format.R24_UNorm_X8_Typeless;

			this.depthStencilCopyTexture = new Texture2D(device, desc);
			this.depthResourceView = new(device, this.depthStencilCopyTexture);
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

		// Copy the back buffer
		deviceContext.CopyResource(service.BackBuffer, this.backBufferCopyTexture);

		// Copy the depth stencil
		deviceContext.CopyResource(this.depthStencilTexture, this.depthStencilCopyTexture);

		// Set the output target to the new buffer
		deviceContext.OutputMerger.SetTargets(this.maskRenderTargetView);

		// Pass the buffers into the shader
		deviceContext.PixelShader.SetShaderResource(2, this.backBufferResourceView);

		deviceContext.Rasterizer.SetViewport(0, 0, service.Width, service.Height);

		this.quad.Draw(Transform.Identity, device, deviceContext);

		using CommandList cmds = deviceContext.FinishCommandList(false);
		device.ImmediateContext.ExecuteCommandList(cmds, true);
		deviceContext.ClearState();
	}

	public void Bind(DeviceContext deviceContext)
	{
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