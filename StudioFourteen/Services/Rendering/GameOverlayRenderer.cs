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
using FFXIVClientStructs.FFXIV.Client.Graphics.Kernel;
using FFXIVClientStructs.FFXIV.Client.Graphics.Render;
using SharpDX.Direct3D11;
using StudioFourteen.Services.Dalamud;
using StudioFourteen.Services.Rendering.Passes;
using StudioFourteen.Services.Tick;

using XivDevice = FFXIVClientStructs.FFXIV.Client.Graphics.Kernel.Device;

public enum OverlayLayers
{
	BeforeEffects,
	AfterEffects,

	BeforeImGui,
	AfterImGui,
}

public class GameOverlayRenderer : Renderer
{
	public readonly ForwardPass Forward = new();

	private readonly GenerateUiMaskPass generateUiMaskPass = new();
	private readonly Dictionary<OverlayLayers, List<RenderPassBase>> layerPasses = new();

	private bool needsImGuiRequeue = false;

	public GameOverlayRenderer()
	{
		this.Forward.ViewportScale = 0.8f;

		this.AddPass(this.generateUiMaskPass);
		this.AddPass(OverlayLayers.BeforeEffects, this.Forward);
	}

	public bool IsAttached { get; private set; }

	public void AddPass(OverlayLayers layer, RenderPassBase pass)
	{
		if (!this.layerPasses.ContainsKey(layer))
			this.layerPasses.Add(layer, new());

		this.layerPasses[layer].Add(pass);
		this.AddPass(pass);
	}

	public void RemovePass(OverlayLayers layer, RenderPassBase pass)
	{
		if (!this.layerPasses.ContainsKey(layer))
			this.layerPasses.Add(layer, new());

		this.layerPasses[layer].Remove(pass);
		this.RemovePass(pass);
	}

	public unsafe void Attach()
	{
		////this.input.Attach();

		Studio.Tick.Add(TickChannels.Game, this.OnGameTick);
		////Studio.Reshade.ReshadeBeforeEffects += this.OnBeforeReshadeRender;
		////Studio.Reshade.ReshadeAfterEffects += this.OnAfterReshadeRender;

		InterfaceManager.RunBeforeImGuiRender(this.OnBeforeImGuiRender);
		InterfaceManager.RunAfterImGuiRender(this.OnAfterImGuiRender);
		this.IsAttached = true;
	}

	public void Detach()
	{
		////this.input.Detach();

		Studio.Tick.Remove(TickChannels.Game, this.OnGameTick);
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

			// If no reshade, this is the first layer to render.
			////if (!Studio.Reshade.IsReshadeEnabled)
			{
				this.SetUpRender();
				////this.Input.Process(this);
				this.RenderUiMask();

				this.RenderLayerPasses(OverlayLayers.BeforeEffects);
				this.RenderLayerPasses(OverlayLayers.AfterEffects);
			}

			this.RenderLayerPasses(OverlayLayers.BeforeImGui);
		}
	}

	private void OnAfterImGuiRender()
	{
		if (this.IsAttached)
		{
			this.needsImGuiRequeue = true;
			this.RenderLayerPasses(OverlayLayers.AfterImGui);
		}
	}

	private void OnBeforeReshadeRender()
	{
		this.SetUpRender();
		////this.Input.Process(this);
		this.RenderUiMask();
		this.RenderLayerPasses(OverlayLayers.BeforeEffects);
	}

	private void OnAfterReshadeRender()
	{
		this.RenderLayerPasses(OverlayLayers.AfterEffects);
	}

	private unsafe void OnGameTick()
	{
		if (this.needsImGuiRequeue)
		{
			InterfaceManager.RunBeforeImGuiRender(this.OnBeforeImGuiRender);
			InterfaceManager.RunAfterImGuiRender(this.OnAfterImGuiRender);
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

	private void RenderLayerPasses(OverlayLayers layer)
	{
		List<RenderPassBase>? passes;
		if (this.layerPasses.TryGetValue(layer, out passes) && passes != null)
		{
			this.RenderPasses(passes);
		}
	}
}
