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
using SharpDX.Direct3D11;
using StudioFourteen.Interop;
using StudioFourteen.Plugin;
using StudioFourteen.Rendering.Stages;
using StudioFourteen.Services;

using Device = SharpDX.Direct3D11.Device;
using XivDevice = FFXIVClientStructs.FFXIV.Client.Graphics.Kernel.Device;

// Thanks to Pictomancy for much of the initial DX11 Setup logic.
// https://github.com/sourpuh/ffxiv_pictomancy/tree/master
public class RenderingService : ServiceBase
{
	public readonly GenerateMaskDepthStage GenerateMaskDepthStage = new(0);
	public readonly GeometryStage GeometryStage = new();

	private Device? device;
	private DeviceContext? deviceContext;

	public Texture2D? BackBuffer { get; private set; }
	public int Width => this.BackBuffer?.Description.Width ?? 0;
	public int Height => this.BackBuffer?.Description.Height ?? 0;

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

	public override void Dispose()
	{
		this.deviceContext?.Dispose();
		this.deviceContext = null;

		this.GenerateMaskDepthStage.Dispose();
		this.GeometryStage.Dispose();

		base.Dispose();
	}

	public void LogInternalError(string message, Exception ex)
	{
		this.Log.Error(ex, message);
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

		this.BackBuffer = (Texture2D)(nint)swapChain->BackBuffer->D3D11Texture2D;
		if (this.BackBuffer == null)
			return;

		if (this.BackBuffer.Description.Format != SharpDX.DXGI.Format.R8G8B8A8_UNorm)
			throw new Exception($"wrong format in back buffer texture {this.BackBuffer.Description.Format}");

		this.device = this.BackBuffer.Device;
		if (this.device == null)
			return;

		if (this.deviceContext == null)
			this.deviceContext = new(this.device);

		this.GenerateMaskDepthStage.Render(this, this.device, this.deviceContext);
		this.GeometryStage.Render(this, this.device, this.deviceContext);
	}
}