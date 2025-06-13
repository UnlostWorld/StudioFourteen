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
using System.Linq;
using FFXIVClientStructs.FFXIV.Client.Graphics.Kernel;
using SharpDX;
using SharpDX.Direct3D11;
using SixLabors.ImageSharp;
using StudioFourteen.Plugin;
using StudioFourteen.Rendering.Passes;
using StudioFourteen.Rendering.Draw;
using StudioFourteen.Services;

using Device = SharpDX.Direct3D11.Device;
using XivDevice = FFXIVClientStructs.FFXIV.Client.Graphics.Kernel.Device;

// Thanks to Pictomancy for much of the initial DX11 Setup logic.
// https://github.com/sourpuh/ffxiv_pictomancy/tree/master
public class RenderingService : ServiceBase
{
	public readonly ShaderCache ShaderCache = new();
	public readonly ForwardPass Forward = new();

	private readonly GenerateUiMaskPass generateUiMaskPass = new();
	private readonly List<RenderPassBase> beforeEffectsPasses = new();
	private readonly List<RenderPassBase> afterEffectsPasses = new();
	private Device? device;
	private DeviceContext? deviceContext;
	private int resolutionChangeCoolDown = 15;
	private bool needsImGuiRequeue = false;
	private bool canRender = false;

	public RenderingService()
	{
		this.afterEffectsPasses.Add(this.Forward);
	}

	public Texture2D? BackBuffer { get; private set; }
	public uint Width { get; private set; }
	public uint Height { get; private set; }

	public void AddBeforeEffectsPass(RenderPassBase pass)
	{
		this.beforeEffectsPasses.Add(pass);
	}

	public void RemoveBeforeEffectsPass(RenderPassBase pass)
	{
		this.beforeEffectsPasses.Remove(pass);
	}

	public void AddAfterEffectsPass(RenderPassBase pass)
	{
		this.afterEffectsPasses.Add(pass);
	}

	public void RemoveAfterEffectsPass(RenderPassBase pass)
	{
		this.afterEffectsPasses.Remove(pass);
	}

	public unsafe override void Attach()
	{
		this.generateUiMaskPass.Attach();
		foreach(RenderPassBase pass in this.beforeEffectsPasses)
		{
			pass.Attach();
		}

		foreach(RenderPassBase pass in this.afterEffectsPasses)
		{
			pass.Attach();
		}

		this.Services.Tick.Add(TickService.Channels.GameTick, this.OnGameTick);
		this.Services.Reshade.ReshadeBeforeEffects += this.OnBeforeReshadeRender;
		this.Services.Reshade.ReshadeAfterEffects += this.OnAfterReshadeRender;

		base.Attach();

		InterfaceManager.RunBeforeImGuiRender(this.OnBeforeImGuiRender);
	}

	public override void Detach()
	{
		base.Detach();

		this.Services.Tick.Remove(TickService.Channels.GameTick, this.OnGameTick);
		this.Services.Reshade.ReshadeBeforeEffects -= this.OnBeforeReshadeRender;
		this.Services.Reshade.ReshadeAfterEffects -= this.OnAfterReshadeRender;

		this.generateUiMaskPass.Detach();
		foreach (RenderPassBase pass in this.beforeEffectsPasses)
		{
			pass.Detach();
		}

		foreach(RenderPassBase pass in this.afterEffectsPasses)
		{
			pass.Detach();
		}
	}

	public override void Dispose()
	{
		this.deviceContext?.Dispose();
		this.deviceContext = null;

		this.generateUiMaskPass.Dispose();
		foreach(RenderPassBase pass in this.beforeEffectsPasses)
		{
			pass.Dispose();
		}

		foreach(RenderPassBase pass in this.afterEffectsPasses)
		{
			pass.Dispose();
		}

		this.ShaderCache.Dispose();

		base.Dispose();
	}

	public void LogInternalError(string message, Exception ex)
	{
		this.Log.Error(ex, message);
	}

	private void OnBeforeImGuiRender()
	{
		if (this.IsAttached)
		{
			this.needsImGuiRequeue = true;

			if (this.Services.Reshade.IsReshadeEnabled)
				return;

			this.SetUpRender();
			this.RenderUiMask();
			this.RenderBeforeEffectsPasses();
			this.RenderAfterEffectsPasses();
		}
	}

	private void OnBeforeReshadeRender()
	{
		this.SetUpRender();
		this.RenderUiMask();
		this.RenderBeforeEffectsPasses();
	}

	private void OnAfterReshadeRender()
	{
		this.RenderAfterEffectsPasses();
	}

	private void OnGameTick()
	{
		if (this.needsImGuiRequeue)
		{
			InterfaceManager.RunBeforeImGuiRender(this.OnBeforeImGuiRender);
			this.needsImGuiRequeue = false;
		}

		if (this.device != null)
		{
			this.ShaderCache.OnTick(this.device);
		}
	}

	private void SetUpRender()
	{
		this.canRender = this.TrySetUpRender();
	}

	private unsafe bool TrySetUpRender()
	{
		try
		{
			if (!this.IsAttached || ServiceManager.ShutdownRequested)
				return false;

			XivDevice* xivDevice = XivDevice.Instance();
			if (xivDevice == null)
				return false;

			SwapChain* swapChain = xivDevice->SwapChain;
			if (swapChain == null)
				return false;

			// BackBuffer should be something from IDXGISwapChain->GetBuffer, which means that IDXGISwapChain itself
			// must have been fully initialized.
			if (swapChain->BackBuffer == null)
				return false;

			this.BackBuffer = (Texture2D)(nint)swapChain->BackBuffer->D3D11Texture2D;
			if (this.BackBuffer == null)
				return false;

			if (this.BackBuffer.Description.Format != SharpDX.DXGI.Format.R8G8B8A8_UNorm)
				throw new Exception($"wrong format in back buffer texture {this.BackBuffer.Description.Format}");

			this.device = this.BackBuffer.Device;
			if (this.device == null)
				return false;

			if (this.deviceContext == null)
				this.deviceContext = new(this.device);

			if (this.Width != xivDevice->Width || this.Height != xivDevice->Height)
			{
				this.Width = xivDevice->Width;
				this.Height = xivDevice->Height;
				this.Log.Information($"Resolution Changed: {this.Width}x{this.Height}");
				this.resolutionChangeCoolDown = 15;

				foreach (RenderPassBase pass in this.beforeEffectsPasses)
				{
					pass.OnResolutionChanged();
				}

				foreach (RenderPassBase pass in this.afterEffectsPasses)
				{
					pass.OnResolutionChanged();
				}

				return false;
			}

			if (this.Width != xivDevice->NewWidth || this.Height != xivDevice->NewHeight)
			{
				this.Log.Information($"Resolution Changing: {xivDevice->Width}x{xivDevice->Height} -> {xivDevice->NewWidth}x{xivDevice->NewHeight}");

				foreach(RenderPassBase pass in this.beforeEffectsPasses)
				{
					pass.OnResolutionChanging();
				}

				foreach(RenderPassBase pass in this.afterEffectsPasses)
				{
					pass.OnResolutionChanging();
				}

				this.resolutionChangeCoolDown = 15;
				return false;
			}

			if (this.resolutionChangeCoolDown > 0)
			{
				this.resolutionChangeCoolDown--;
				return false;
			}

			return true;
		}
		catch (Exception ex)
		{
			this.Log.Error(ex, "Error attempting to set up render device");
			this.Detach();
		}

		return false;
	}

	private void RenderUiMask()
	{
		if (!this.canRender || this.device == null || this.deviceContext == null)
			return;

		try
		{
			this.generateUiMaskPass.Render(this, this.device, this.deviceContext);
		}
		catch (Exception ex)
		{
			this.Log.Error(ex, $"Error in render Ui Mask");
			this.Detach();
		}
	}

	private void RenderBeforeEffectsPasses() => this.RenderPasses(this.beforeEffectsPasses);
	private void RenderAfterEffectsPasses() => this.RenderPasses(this.afterEffectsPasses);

	private void RenderPasses(IEnumerable<RenderPassBase> passes)
	{
		if (!this.canRender || this.device == null || this.deviceContext == null)
			return;

		// Perform render passes.
		foreach(RenderPassBase pass in passes.ToArray())
		{
			if (!pass.IncludeInScreenshots && this.Services.Photos.IsCapturing)
				continue;

			try
			{
				this.generateUiMaskPass.Bind(this.deviceContext);
				pass.Render(this, this.device, this.deviceContext);
			}
			catch(Exception ex)
			{
				this.Log.Error(ex, $"Error in rendering pass: {pass}");
				this.Detach();
				return;
			}
		}
	}
}