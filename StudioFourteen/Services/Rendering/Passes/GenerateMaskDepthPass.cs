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

namespace StudioFourteen.Services.Rendering.Passes;

using System;
using System.Numerics;
using System.Runtime.InteropServices;
using FFXIVClientStructs.FFXIV.Client.Graphics.Render;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using StudioFourteen.Services.Numerics;
using StudioFourteen.Services.Rendering.Draw;
using StudioFourteen.Services.Rendering.Materials;

using Device = SharpDX.Direct3D11.Device;
using XivDevice = FFXIVClientStructs.FFXIV.Client.Graphics.Kernel.Device;

public class GenerateUiMaskPass : RenderPassBase
{
	private readonly MeshRenderer<CopyUiMaskEffect> quad = new(MeshContent.Quad);

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

	public unsafe override void Render(Renderer renderer, Device device, DeviceContext deviceContext)
	{
		if (renderer.BackBuffer == null)
			return;

		RenderTargetManagerEx* pRenderTargetManager = RenderTargetManagerEx.Instance();
		if (pRenderTargetManager == null)
			return;

		// Create a shader resource copy of the back buffer so it can be accessed in the shader
		if (this.backBufferCopyTexture == null)
		{
			try
			{
				this.backBufferCopyTexture?.Dispose();
				this.backBufferResourceView?.Dispose();

				Texture2DDescription desc = renderer.BackBuffer.Description;
				desc.BindFlags = BindFlags.ShaderResource;

				this.backBufferCopyTexture = new Texture2D(device, desc);
				this.backBufferResourceView = new(device, this.backBufferCopyTexture);
			}
			catch (Exception ex)
			{
				throw new Exception("Failed to create back buffer resource", ex);
			}
		}

		this.depthStencilTexture = (Texture2D)(nint)pRenderTargetManager->DepthStencil->D3D11Texture2D;

		// Create a handle to the depth stencil
		if (this.depthStencilCopyTexture == null)
		{
			try
			{
				this.depthStencilCopyTexture?.Dispose();
				this.depthResourceView?.Dispose();

				Texture2DDescription desc = this.depthStencilTexture.Description;
				desc.BindFlags = BindFlags.ShaderResource;

				if (Studio.Platform.OperatingSystem == OSPlatform.Windows)
				{
					desc.Format = Format.R24_UNorm_X8_Typeless;
				}
				else
				{
					desc.Format = Format.R24G8_Typeless;
				}

				this.depthStencilCopyTexture = new Texture2D(device, desc);
				this.depthResourceView = new(device, this.depthStencilCopyTexture);
			}
			catch (Exception ex)
			{
				throw new Exception("Failed to create depth resource", ex);
			}
		}

		float renderWidth = pRenderTargetManager->Base.Resolution_Width;
		float renderHeight = pRenderTargetManager->Base.Resolution_Height;
		float bufferWidth = this.depthStencilTexture.Description.Width;
		float bufferHeight = this.depthStencilTexture.Description.Height;
		renderer.RenderScale = new Vector2(renderWidth / bufferWidth, renderHeight / bufferHeight);

		this.quad.Material.Scale = renderer.RenderScale;

		// Create an output texture
		if (this.maskTexture == null)
		{
			this.maskTexture?.Dispose();
			this.maskRenderTargetView?.Dispose();
			this.maskResourceView?.Dispose();

			Texture2DDescription desc = renderer.BackBuffer.Description;
			desc.BindFlags = BindFlags.ShaderResource | BindFlags.RenderTarget;
			this.maskTexture = new(device, desc);

			try
			{
				RenderTargetViewDescription rtDesc = default;
				rtDesc.Format = Format.R8G8B8A8_UNorm;
				rtDesc.Dimension = RenderTargetViewDimension.Texture2D;
				rtDesc.Texture2D = new() { };
				this.maskRenderTargetView = new(device, this.maskTexture, rtDesc);

				this.maskResourceView = new(device, this.maskTexture);
			}
			catch (Exception ex)
			{
				throw new Exception("Failed to create mask resource", ex);
			}
		}

		// Copy the back buffers
		deviceContext.CopyResource(renderer.BackBuffer, this.backBufferCopyTexture);

		// Copy the depth stencil
		deviceContext.CopyResource(this.depthStencilTexture, this.depthStencilCopyTexture);

		// Set the output target to the new buffer
		deviceContext.OutputMerger.SetTargets(this.maskRenderTargetView);

		// Pass the buffers into the shader
		deviceContext.PixelShader.SetShaderResource(2, this.backBufferResourceView);

		deviceContext.Rasterizer.SetViewport(0, 0, renderer.Width, renderer.Height);

		this.quad.Draw(renderer, Transform.Identity, device, deviceContext);

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
		this.depthStencilCopyTexture?.Dispose();
		this.maskTexture?.Dispose();
		this.maskRenderTargetView?.Dispose();
		this.maskResourceView?.Dispose();
		this.depthResourceView?.Dispose();
		base.Dispose();
	}
}