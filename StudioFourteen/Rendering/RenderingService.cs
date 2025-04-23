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

namespace StudioFourteen.Rendering;

using System;
using System.Numerics;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using StudioFourteen.Interop;
using StudioFourteen.Plugin;
using StudioFourteen.Services;

using Buffer = SharpDX.Direct3D11.Buffer;
using Device = SharpDX.Direct3D11.Device;
using XivDevice = FFXIVClientStructs.FFXIV.Client.Graphics.Kernel.Device;

public class RenderingService : ServiceBase
{
	private readonly TheCube cube = new();

	public unsafe override void Attach()
	{
		this.Services.Tick.Add(TickService.Channels.GameTick, this.OnGameTick);

		if (SwapChainHelper.IsReshade)
		{
			Hooks.ReshadeOnPresent.Enable(this.ReshadeOnPresentDetour);
			InterfaceManager.DisableReshadePresent();
		}

		base.Attach();
	}

	public override void Detach()
	{
		base.Detach();

		this.Services.Tick.Remove(TickService.Channels.GameTick, this.OnGameTick);

		if (SwapChainHelper.IsReshade)
		{
			InterfaceManager.EnableReshadePresent();
		}

		Hooks.ReshadeOnPresent.Disable();
	}

	protected void OnGameTick()
	{
		// If not using reshade, fallback to just run before ImGUI within dalamud's present
		if (!SwapChainHelper.IsReshade)
		{
			InterfaceManager.RunBeforeImGuiRender(this.Render);
		}
	}

	private void ReshadeOnPresentDetour(nint swapChain, uint flags, nint presentParams)
	{
		Hooks.ReshadeOnPresent.Original(swapChain, flags, presentParams);
		this.Render();
	}

	private unsafe void Render()
	{
		var kernelDev = XivDevice.Instance();
		if (kernelDev == null)
			return;

		var swapChain = kernelDev->SwapChain;
		if (swapChain == null)
			return;

		// BackBuffer should be something from IDXGISwapChain->GetBuffer, which means that IDXGISwapChain itself
		// must have been fully initialized.
		if (swapChain->BackBuffer == null)
			return;

		Texture2D backBuffer = (Texture2D)(nint)swapChain->BackBuffer->D3D11Texture2D;
		if (backBuffer == null)
			return;

		Device device = backBuffer.Device;
		if (device == null)
			return;

		RenderTargetViewDescription desc = default;
		desc.Format = Format.R8G8B8A8_UNorm;
		desc.Dimension = RenderTargetViewDimension.Texture2D;
		desc.Texture2D = new() { };

		using RenderTargetView renderTargetView = new(device, backBuffer, desc);
		using DeviceContext context = new(device);

		////context.ClearRenderTargetView(this.renderTargetView, new(0, 0, 0, 0));
		context.Rasterizer.SetViewport(0, 0, kernelDev->Width, kernelDev->Height);
		context.OutputMerger.SetTargets(renderTargetView);

		this.cube.Initialize(device, context);
		this.cube.Draw(device, context);

		using CommandList cmds = context.FinishCommandList(false);
		device.ImmediateContext.ExecuteCommandList(cmds, true);
		context.ClearState();
	}
}

public class TheCube : IDisposable
{
	private bool isInitialized = false;

	private VertexShader? vertexShader;
	private PixelShader? pixelShader;
	private Buffer? vertices;
	private InputLayout? layout;

	public void Initialize(Device device, DeviceContext context)
	{
		if (this.isInitialized)
			return;

		this.isInitialized = true;

		string shaderStr = @"
			float4 vert(float4 position : POSITION) : SV_POSITION
			{
			   return position;
			}

			float4 pixel(float4 position : SV_POSITION) : SV_TARGET
			{
			   return float4(1.0, 0.0, 0.0, 1.0);
			}";

		var vertexShaderByteCode = ShaderBytecode.Compile(shaderStr, "vert", "vs_4_0", ShaderFlags.Debug);
		this.vertexShader = new VertexShader(device, vertexShaderByteCode);

		var pixelShaderByteCode = ShaderBytecode.Compile(shaderStr, "pixel", "ps_4_0", ShaderFlags.Debug);
		this.pixelShader = new PixelShader(device, pixelShaderByteCode);

		var signature = ShaderSignature.GetInputSignature(vertexShaderByteCode);

		// Layout from VertexShader input signature
		this.layout = new InputLayout(
			device,
			signature,
			[
				new InputElement("POSITION", 0, Format.R32G32B32_Float, 0),
	        ]);

		// Instantiate Vertex buiffer from vertex data
		this.vertices = Buffer.Create(
			device,
			BindFlags.VertexBuffer,
			[
				new Vector3(-0.5f, 0.5f, 0.0f), new Vector3(0.5f, 0.5f, 0.0f), new Vector3(0.0f, -0.5f, 0.0f)
			]);
	}

	public unsafe void Draw(Device device, DeviceContext context)
	{
		context.InputAssembler.InputLayout = this.layout;

		context.VertexShader.Set(this.vertexShader);
		context.PixelShader.Set(this.pixelShader);
		context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
		context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(this.vertices, SharpDX.Utilities.SizeOf<Vector3>(), 0));

		context.Draw(3, 0);
	}

	public void Dispose()
	{
		this.vertices?.Dispose();
		this.pixelShader?.Dispose();
		this.vertexShader?.Dispose();
		this.layout?.Dispose();
	}
}