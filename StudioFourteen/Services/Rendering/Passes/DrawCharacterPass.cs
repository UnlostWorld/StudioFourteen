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

using System.Numerics;
using System.Runtime.InteropServices;
using FFXIVClientStructs.FFXIV.Client.Graphics.Kernel;
using FFXIVClientStructs.FFXIV.Client.Graphics.Render;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using StudioFourteen;
using StudioFourteen.Services.Content;
using StudioFourteen.Services.Numerics;
using StudioFourteen.Services.Rendering;
using StudioFourteen.Services.Rendering.Draw;
using StudioFourteen.Services.Rendering.Materials;
using StudioFourteen.Services.Rendering.Passes;

using Device = SharpDX.Direct3D11.Device;
using Format = SharpDX.DXGI.Format;

public unsafe class DrawCharacterPass : InstanceRenderPassBase<DrawCharacterPass.PassDataStruct>
{
	private readonly MeshRenderer<Effect> quad = new(MeshContent.Quad);

	private Texture2D? characterTexture;
	private ShaderResourceView? characterResourceView;
	private RenderTargetView? backBufferTargetView;
	private BlendState? blend;
	private CharaView* pView;

	public unsafe override void Render(Renderer renderer, Device device, DeviceContext deviceContext)
	{
		if (renderer.BackBuffer == null)
			return;

		RenderTargetManagerEx* pRenderTargetManager = RenderTargetManagerEx.Instance();
		if (pRenderTargetManager == null)
			return;

		base.Render(renderer, device, deviceContext);

		// Create a shader resource copy of the back buffer so it can be accessed in the shader
		if (this.characterTexture == null)
		{
			////this.characterTexture?.Dispose();
			this.characterResourceView?.Dispose();

			this.pView = CharaView.Create();

			// Set customization options to anything we want. =]
			this.pView->ModelData.CustomizeData.Race = 0;
			this.pView->ModelData.CustomizeData.Sex = 1;

			// Use object Id 1 as its guaranteed to be the current characters minion/mount/whatever,
			//  which wont ever have its own chara view, so we can safely use it for our purposes.
			this.pView->Initialize(null, 1, 0);
			Texture* pTexture = pRenderTargetManager->Base.GetCharaViewTexture(1);
			this.characterTexture = new((nint)pTexture->D3D11Texture2D);
			this.characterResourceView = new(device, this.characterTexture);
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

		this.pView->Render(1);

		// Pass the buffers into the shader
		deviceContext.PixelShader.SetShaderResource(2, this.characterResourceView);

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
		////this.pView->Release();

		this.quad.Dispose();
		this.characterResourceView?.Dispose();
		////this.characterTexture?.Dispose();
		this.backBufferTargetView?.Dispose();

		base.Dispose();
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct PassDataStruct
	{
		public Vector4 Unused;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct Effect : IMaterial
	{
		public Vector4 Unused;

		public IContent<ShaderBytecode>? GetVertexShader() => new ShaderReference("Shaders/Effect_Copy.hlsl", "vs_4_0", "vert");
		public IContent<ShaderBytecode>? GetPixelShader() => new ShaderReference("Shaders/Effect_Copy.hlsl", "ps_4_0", "pixel");
		public IContent<ShaderBytecode>? GetGeometryShader() => null;

		public void Initialize()
		{
		}
	}
}