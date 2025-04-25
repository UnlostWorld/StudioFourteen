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

using FFXIVClientStructs.FFXIV.Client.Graphics.Kernel;
using SharpDX.Direct3D11;
using SharpDX.DXGI;

using Device = SharpDX.Direct3D11.Device;

public class DrawBufferPass : RenderPassBase
{
	private readonly Renderable renderable;

	private Texture2D? buffer;
	private Texture2D? bufferCopyTexture;
	private ShaderResourceView? bufferResourceView;
	private RenderTargetView? backBufferTargetView;

	public DrawBufferPass()
	{
		this.renderable = new(EmbeddedMaterial.Blit, EmbeddedGeometry.Quad);
	}

	public unsafe DrawBufferPass(Texture* pTexture)
		: this()
	{
		nint address = (nint)pTexture->D3D11Texture2D;
		if (address == 0)
			return;

		this.Set(address);
	}

	public DrawBufferPass(nint address)
		: this()
	{
		this.Set(address);
	}

	public void Set(nint address)
	{
		if (this.buffer != null && this.buffer.NativePointer == address)
			return;

		this.buffer?.Dispose();
		this.buffer = null;

		if (address == 0)
			return;

		this.buffer = new(address);

		Logging.Shared.Information($"Got Buffer: {this.buffer}");
		Logging.Shared.Information($"    Format: {this.buffer.Description.Format}");
		Logging.Shared.Information($"      Size: {this.buffer.Description.Width}x{this.buffer.Description.Height}");
		Logging.Shared.Information($"      Bind: {this.buffer.Description.BindFlags}");
		Logging.Shared.Information($"       CPU: {this.buffer.Description.CpuAccessFlags}");
		Logging.Shared.Information($"   Options: {this.buffer.Description.OptionFlags}");
		Logging.Shared.Information($"     Usage: {this.buffer.Description.Usage}");
	}

	public override void Render(RenderingService service, Device device, DeviceContext deviceContext)
	{
		if (service.BackBuffer == null || this.buffer == null)
			return;

		// Create a shader resource copy of the buffer so it can be accessed in the shader
		if (this.bufferCopyTexture == null
			|| this.bufferCopyTexture.Description.Width != this.buffer.Description.Width
			|| this.bufferCopyTexture.Description.Height != this.buffer.Description.Height)
		{
			this.bufferCopyTexture?.Dispose();
			this.bufferResourceView?.Dispose();

			Texture2DDescription desc = this.buffer.Description;
			desc.BindFlags |= BindFlags.ShaderResource;
			////desc.Format = Format.R24_UNorm_X8_Typeless;
			this.bufferCopyTexture = new Texture2D(device, desc);

			this.bufferResourceView = new(device, this.bufferCopyTexture);
		}

		if (this.backBufferTargetView == null)
		{
			RenderTargetViewDescription desc = default;
			desc.Format = Format.R8G8B8A8_UNorm;
			desc.Dimension = RenderTargetViewDimension.Texture2D;
			this.backBufferTargetView = new(device, service.BackBuffer, desc);
		}

		// Copy the buffer
		deviceContext.CopyResource(this.buffer, this.bufferCopyTexture);

		// Pass the buffer into the shader
		deviceContext.PixelShader.SetShaderResource(0, this.bufferResourceView);
		deviceContext.OutputMerger.SetTargets(this.backBufferTargetView);
		deviceContext.Rasterizer.SetViewport(0, 0, service.Width, service.Height);

		this.renderable.Draw(service, device, deviceContext);

		using CommandList cmds = deviceContext.FinishCommandList(false);
		device.ImmediateContext.ExecuteCommandList(cmds, true);
		deviceContext.ClearState();
	}

	public override void Dispose()
	{
		this.renderable.Dispose();
		this.bufferCopyTexture?.Dispose();
		this.backBufferTargetView?.Dispose();
		base.Dispose();
	}
}