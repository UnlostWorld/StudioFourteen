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

namespace StudioFourteen.Services.Rendering;

using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using FFXIVClientStructs.FFXIV.Client.Graphics.Kernel;
using FFXIVClientStructs.FFXIV.Client.Graphics.Render;
using SharpDX.Direct3D11;
using StudioFourteen.Services.Dalamud;
using StudioFourteen.Services.Rendering.Draw.Handles;
using StudioFourteen.Services.Rendering.Passes;
using StudioFourteen.Services.Tick;
using XivDevice = FFXIVClientStructs.FFXIV.Client.Graphics.Kernel.Device;

public class GameOverlayRenderer : Renderer
{
	public readonly ForwardPass Forward = new();
	public readonly ForwardPass Interface = new();

	private readonly GenerateUiMaskPass generateUiMaskPass = new();
	private readonly List<RenderPassBase> beforeEffectsPasses = new();
	private readonly List<RenderPassBase> afterEffectsPasses = new();

	private bool needsImGuiRequeue = false;

	public GameOverlayRenderer()
	{
		this.Forward.ViewportScale = 0.8f;

		this.afterEffectsPasses.Add(this.Forward);
		this.AddPass(this.Forward);
		this.AddPass(this.generateUiMaskPass);
		this.afterEffectsPasses.Add(this.Interface);
		this.AddPass(this.Interface);
	}

	public bool IsAttached { get; private set; }

	public void AddBeforeEffectsPass(RenderPassBase pass)
	{
		this.beforeEffectsPasses.Add(pass);
		this.AddPass(pass);
	}

	public void RemoveBeforeEffectsPass(RenderPassBase pass)
	{
		this.beforeEffectsPasses.Remove(pass);
		this.RemovePass(pass);
	}

	public void AddAfterEffectsPass(RenderPassBase pass)
	{
		this.afterEffectsPasses.Add(pass);
		this.AddPass(pass);
	}

	public void RemoveAfterEffectsPass(RenderPassBase pass)
	{
		this.afterEffectsPasses.Remove(pass);
		this.RemovePass(pass);
	}

	public unsafe void Attach()
	{
		////this.input.Attach();

		Studio.Tick.Add(TickService.Channels.GameTick, this.OnGameTick);
		////Studio.Reshade.ReshadeBeforeEffects += this.OnBeforeReshadeRender;
		////Studio.Reshade.ReshadeAfterEffects += this.OnAfterReshadeRender;

		InterfaceManager.RunBeforeImGuiRender(this.OnBeforeImGuiRender);
		this.IsAttached = true;
	}

	public void Detach()
	{
		////this.input.Detach();

		Studio.Tick.Remove(TickService.Channels.GameTick, this.OnGameTick);
		////Studio.Reshade.ReshadeBeforeEffects -= this.OnBeforeReshadeRender;
		////Studio.Reshade.ReshadeAfterEffects -= this.OnAfterReshadeRender;

		this.IsAttached = false;
	}

	public override unsafe Matrix4x4 GetProjectionMatrix(Renderer renderer)
	{
		return Studio.Camera.CurrentProjection;
	}

	public override unsafe Matrix4x4 GetViewMatrix(Renderer renderer)
	{
		return Studio.Camera.CurrentView;
	}

	public override unsafe Vector3 GetCameraPosition(Renderer renderer)
	{
		return Studio.Camera.CurrentPosition;
	}

	protected override bool TrySetUpRender()
	{
		////if (this.Services.Studio.HideUi)
		////	return false;

		return base.TrySetUpRender();
	}

	protected override void RenderPass(RenderPassBase pass)
	{
		if (this.DeviceContext == null)
			return;

		////if (!pass.IncludeInScreenshots && this.Services.Photos.IsCapturing)
		////	return;

		this.generateUiMaskPass.Bind(this.DeviceContext);
		base.RenderPass(pass);
	}

	protected unsafe override Texture2D? GetBackBuffer()
	{
		if (Studio.IsDisposed)
			return null;

		RenderTargetManagerEx* pRenderTargetManager = RenderTargetManagerEx.Instance();
		if (pRenderTargetManager == null)
			return null;

		XivDevice* xivDevice = XivDevice.Instance();
		if (xivDevice == null)
			return null;

		SwapChain* swapChain = xivDevice->SwapChain;
		if (swapChain == null)
			return null;

		// BackBuffer should be something from IDXGISwapChain->GetBuffer, which means that IDXGISwapChain itself
		// must have been fully initialized.
		if (swapChain->BackBuffer == null)
			return null;

		Texture2D backBuffer = new((nint)swapChain->BackBuffer->D3D11Texture2D);

		if (backBuffer.Description.Format != SharpDX.DXGI.Format.R8G8B8A8_UNorm)
			throw new Exception($"wrong format in back buffer texture {backBuffer.Description.Format}");

		return backBuffer;
	}

	protected override SharpDX.Direct3D11.Device? GetDevice()
	{
		if (this.BackBuffer == null)
			return null;

		return this.BackBuffer.Device;
	}

	private void OnBeforeImGuiRender()
	{
		if (this.IsAttached)
		{
			this.needsImGuiRequeue = true;

			////if (Studio.Reshade.IsReshadeEnabled)
			////	return;

			this.SetUpRender();
			////this.Input.Process(this);
			this.RenderUiMask();
			this.RenderBeforeEffectsPasses();
			this.RenderAfterEffectsPasses();
		}
	}

	private void OnBeforeReshadeRender()
	{
		this.SetUpRender();
		////this.Input.Process(this);
		this.RenderUiMask();
		this.RenderBeforeEffectsPasses();
	}

	private void OnAfterReshadeRender()
	{
		this.RenderAfterEffectsPasses();
	}

	private unsafe void OnGameTick()
	{
		if (this.needsImGuiRequeue)
		{
			InterfaceManager.RunBeforeImGuiRender(this.OnBeforeImGuiRender);
			this.needsImGuiRequeue = false;
		}

		XivDevice* xivDevice = XivDevice.Instance();
		if (xivDevice == null)
			return;

		this.NewWidth = (int)xivDevice->NewWidth;
		this.NewHeight = (int)xivDevice->NewHeight;
	}

	private void RenderUiMask()
	{
		if (!this.CanRender || this.Device == null || this.DeviceContext == null)
			return;

		try
		{
			this.generateUiMaskPass.Render(this, this.Device, this.DeviceContext);
		}
		catch (Exception ex)
		{
			Studio.Log.Error(ex, $"Error in render Ui Mask");
			this.Detach();
		}
	}

	private void RenderBeforeEffectsPasses() => this.RenderPasses(this.beforeEffectsPasses);
	private void RenderAfterEffectsPasses() => this.RenderPasses(this.afterEffectsPasses);
}
