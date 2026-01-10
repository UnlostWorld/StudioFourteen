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

using System.Numerics;
using System.Runtime.InteropServices;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;
using StudioFourteen.Services.Content;
using StudioFourteen.Services.Numerics;
using StudioFourteen.Services.Rendering.Draw;
using StudioFourteen.Services.Rendering.Materials;
using Device = SharpDX.Direct3D11.Device;
using Format = SharpDX.DXGI.Format;

[StructLayout(LayoutKind.Sequential)]
public struct ScreenEffectPassData
{
	public Vector2 ScreenSize;
	public float Unused1;
	public float Unused2;
}

public class ScreenEffectPass<TMaterialData>() : InstanceRenderPassBase<ScreenEffectPassData>
	where TMaterialData : unmanaged, IMaterial
{
	private readonly MeshRenderer<TMaterialData> quad = new(MeshContent.Quad);

	private Texture2D? backBufferCopyTexture;
	private ShaderResourceView? backBufferResourceView;
	private RenderTargetView? backBufferTargetView;
	private BlendState? blend;

	public ref TMaterialData Material => ref this.quad.Material;

	public override void OnResolutionChanged()
	{
		this.backBufferResourceView?.Dispose();
		this.backBufferResourceView = null;

		this.backBufferCopyTexture?.Dispose();
		this.backBufferCopyTexture = null;

		this.backBufferTargetView?.Dispose();
		this.backBufferTargetView = null;

		base.OnResolutionChanged();
	}

	public override void Render(Renderer renderer, Device device, DeviceContext deviceContext)
	{
		if (renderer.BackBuffer == null)
			return;

		this.PassData.ScreenSize = new(renderer.Width, renderer.Height);

		base.Render(renderer, device, deviceContext);

		// Create a shader resource copy of the back buffer so it can be accessed in the shader
		if (this.backBufferCopyTexture == null)
		{
			this.backBufferCopyTexture?.Dispose();
			this.backBufferResourceView?.Dispose();

			Texture2DDescription desc = renderer.BackBuffer.Description;
			desc.BindFlags = BindFlags.ShaderResource;
			this.backBufferCopyTexture = new Texture2D(device, desc);

			this.backBufferResourceView = new(device, this.backBufferCopyTexture);
		}

		if (this.backBufferTargetView == null)
		{
			this.backBufferTargetView?.Dispose();

			RenderTargetViewDescription desc = default;
			desc.Format = Format.R8G8B8A8_UNorm;
			desc.Dimension = RenderTargetViewDimension.Texture2D;
			desc.Texture2D = new() { };

			this.backBufferTargetView = new(device, renderer.BackBuffer, desc);
		}

		if (this.blend == null)
		{
			BlendStateDescription blendDesc = default;
			blendDesc.AlphaToCoverageEnable = false;
			blendDesc.RenderTarget[0].IsBlendEnabled = true;
			blendDesc.RenderTarget[0].SourceBlend = BlendOption.SourceAlpha;
			blendDesc.RenderTarget[0].DestinationBlend = BlendOption.InverseSourceAlpha;
			blendDesc.RenderTarget[0].BlendOperation = BlendOperation.Add;
			blendDesc.RenderTarget[0].SourceAlphaBlend = BlendOption.Zero;
			blendDesc.RenderTarget[0].DestinationAlphaBlend = BlendOption.Zero;
			blendDesc.RenderTarget[0].AlphaBlendOperation = BlendOperation.Add;
			blendDesc.RenderTarget[0].RenderTargetWriteMask = ColorWriteMaskFlags.Red | ColorWriteMaskFlags.Green | ColorWriteMaskFlags.Blue;

			this.blend = new(device, blendDesc);
		}

		// Copy the back buffer into the copy
		deviceContext.CopyResource(renderer.BackBuffer, this.backBufferCopyTexture);

		// Pass the buffers into the shader
		deviceContext.PixelShader.SetShaderResource(2, this.backBufferResourceView);

		// Set the output
		deviceContext.Rasterizer.SetViewport(0, 0, renderer.Width, renderer.Height);
		deviceContext.OutputMerger.SetBlendState(this.blend, null, -1);
		deviceContext.OutputMerger.SetTargets(this.backBufferTargetView);

		this.quad.Draw(renderer, Transform.Identity, device, deviceContext);

		using CommandList cmds = deviceContext.FinishCommandList(false);
		device.ImmediateContext.ExecuteCommandList(cmds, true);
		deviceContext.ClearState();
	}

	public override void Dispose()
	{
		this.quad.Dispose();
		this.backBufferResourceView?.Dispose();
		this.backBufferCopyTexture?.Dispose();
		this.backBufferTargetView?.Dispose();

		base.Dispose();
	}
}