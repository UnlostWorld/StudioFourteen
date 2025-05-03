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
using System.Collections.Generic;
using FFXIVClientStructs.FFXIV.Client.Graphics.Kernel;
using SharpDX.Direct3D11;
using StudioFourteen.Interop;
using StudioFourteen.Plugin;
using StudioFourteen.Rendering.Passes;
using StudioFourteen.Services;

using Device = SharpDX.Direct3D11.Device;
using XivDevice = FFXIVClientStructs.FFXIV.Client.Graphics.Kernel.Device;

// Thanks to Pictomancy for much of the initial DX11 Setup logic.
// https://github.com/sourpuh/ffxiv_pictomancy/tree/master
public class RenderingService : ServiceBase
{
	public readonly GenerateMaskDepthPass GenerateMaskDepth = new();
	public readonly ForwardPass Forward = new();
	public readonly DrawBufferPass DrawBuffer = new();

	private readonly List<RenderPassBase> passes = new();
	private Device? device;
	private DeviceContext? deviceContext;
	private int resolutionChangeCooldown = 15;

	public RenderingService()
	{
		this.passes.Add(this.GenerateMaskDepth);
		this.passes.Add(this.Forward);
	}

	public Texture2D? BackBuffer { get; private set; }
	public uint Width { get; private set; }
	public uint Height { get; private set; }

	public unsafe override void Attach()
	{
		this.Services.Tick.Add(TickService.Channels.GameTick, this.OnGameTick);

		if (SwapChainHelper.IsReshade)
		{
			Hooks.ReshadeOnPresent.Enable(this.ReshadeOnPresentDetour);
			InterfaceManager.DisableReshadePresent();
		}

		foreach(RenderPassBase pass in this.passes)
		{
			pass.Attach();
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

		foreach(RenderPassBase pass in this.passes)
		{
			pass.Detach();
		}
	}

	public override void Dispose()
	{
		this.deviceContext?.Dispose();
		this.deviceContext = null;

		this.GenerateMaskDepth?.Dispose();
		this.Forward?.Dispose();
		this.DrawBuffer?.Dispose();

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
		if (!this.IsAttached || ServiceManager.ShutdownRequested)
			return;

		XivDevice* xivDevice = XivDevice.Instance();
		if (xivDevice == null)
			return;

		if (this.Width != xivDevice->Width || this.Height != xivDevice->Height)
		{
			this.Width = xivDevice->Width;
			this.Height = xivDevice->Height;
			this.Log.Information($"Resolution Changed: {this.Width}x{this.Height}");
			this.resolutionChangeCooldown = 15;

			foreach(RenderPassBase pass in this.passes)
			{
				pass.OnResolutionChanged();
			}

			return;
		}

		if (this.Width != xivDevice->NewWidth || this.Height != xivDevice->NewHeight)
		{
			this.Log.Information($"Resolution Changing: {xivDevice->Width}x{xivDevice->Height} -> {xivDevice->NewWidth}x{xivDevice->NewHeight}");

			foreach(RenderPassBase pass in this.passes)
			{
				pass.OnResolutionChanging();
			}

			this.resolutionChangeCooldown = 15;
			return;
		}

		if (this.resolutionChangeCooldown > 0)
		{
			this.resolutionChangeCooldown--;
			return;
		}

		SwapChain* swapChain = xivDevice->SwapChain;
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

		// Perform render passes.
		foreach(RenderPassBase pass in this.passes)
		{
			try
			{
				pass.Render(this, this.device, this.deviceContext);
			}
			catch(Exception ex)
			{
				this.Log.Error(ex, $"Error in rendering pass: {pass}");
				this.Detach();
			}
		}
	}
}