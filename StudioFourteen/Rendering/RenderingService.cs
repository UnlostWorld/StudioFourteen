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

using System.Collections.Generic;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using StudioFourteen.Interop;
using StudioFourteen.Plugin;
using StudioFourteen.Services;
using StudioFourteen.Rendering.Materials;
using StudioFourteen.Rendering.Geometry;

using Device = SharpDX.Direct3D11.Device;
using XivDevice = FFXIVClientStructs.FFXIV.Client.Graphics.Kernel.Device;

public class RenderingService : ServiceBase
{
	private readonly List<Renderable> renderables = new();

	private Device? device;
	private RenderTargetView? backBufferTargetView;
	private DeviceContext? deviceContext;

	public RenderingService()
	{
		MaterialBase mat = new TestMaterial();
		GeometryBase geo = new TriangleGeometry();
		this.Add(new(mat, geo));
	}

	public void Add(Renderable renderable)
	{
		this.renderables.Add(renderable);
	}

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

		this.backBufferTargetView?.Dispose();
		this.backBufferTargetView = null;
		this.deviceContext?.Dispose();
		this.deviceContext = null;

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

		this.device = backBuffer.Device;
		if (this.device == null)
			return;

		if (this.backBufferTargetView == null)
		{
			RenderTargetViewDescription desc = default;
			desc.Format = Format.R8G8B8A8_UNorm;
			desc.Dimension = RenderTargetViewDimension.Texture2D;
			desc.Texture2D = new() { };
			this.backBufferTargetView = new(this.device, backBuffer, desc);
		}

		if (this.deviceContext == null)
			this.deviceContext = new(this.device);

		////context.ClearRenderTargetView(this.renderTargetView, new(0, 0, 0, 0));
		this.deviceContext.Rasterizer.SetViewport(0, 0, kernelDev->Width, kernelDev->Height);
		this.deviceContext.OutputMerger.SetTargets(this.backBufferTargetView);

		foreach(Renderable renderable in this.renderables)
		{
			renderable.Draw(this.device, this.deviceContext);
		}

		using CommandList cmds = this.deviceContext.FinishCommandList(false);
		this.device.ImmediateContext.ExecuteCommandList(cmds, true);
		this.deviceContext.ClearState();
	}
}