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
using System.Numerics;
using FFXIVClientStructs.FFXIV.Client.Graphics.Kernel;
using SharpDX.Direct3D11;
using StudioFourteen.Input;
using StudioFourteen.Plugin;
using StudioFourteen.Rendering.Draw.Handles;
using StudioFourteen.Rendering.Passes;
using StudioFourteen.Services;
using StudioFourteen.Utilities;
using XivDevice = FFXIVClientStructs.FFXIV.Client.Graphics.Kernel.Device;

public class RendererStudioCamera : RendererCamera
{
	public override Vector3 GetCameraPosition(Renderer renderer) => ServiceManager.Instance.Camera.CurrentPosition;
	public override Matrix4x4 GetViewMatrix(Renderer renderer) => ServiceManager.Instance.Camera.LastView;
	public override Matrix4x4 GetProjectionMatrix(Renderer renderer) => ServiceManager.Instance.Camera.LastProjection;
}

public class OverlayRendererInput : RendererInput
{
	private readonly Input2DListener dragListener = new(
		InputAction.Handle_Right,
		InputAction.Handle_Left,
		InputAction.Handle_Down,
		InputAction.Handle_Up,
		"Handle Service Move");

	private readonly Input0DListener selectListener = new(
		InputAction.Handle_Select,
		"Handle Service Select");

	private bool didDrag = false;

	public void Attach()
	{
		this.selectListener.Enable();
		this.Timeout();
	}

	public void Detach()
	{
		this.selectListener.Disable();
	}

	public override void OnHandlePress(Handle? handle)
	{
		base.OnHandlePress(handle);

		if (handle != null)
		{
			this.dragListener.Enable();
		}
		else
		{
			this.dragListener.Disable();
		}
	}

	protected override void ProcessInput(
		out InputStates inputState,
		out Vector2? cursorPosition,
		out Vector2 dragDelta)
	{
		inputState = this.selectListener.GetState();
		cursorPosition = this.Services.Input.Mouse?.GetPosition();
		dragDelta = this.dragListener.Value;

		if (inputState == InputStates.Deactivated && !this.didDrag)
		{
			this.Services.Selection.Clear();
		}

		if (inputState == InputStates.Held)
		{
			this.didDrag = this.Services.Input.Mouse?.IsAnyDragging == true;
		}

		if (inputState == InputStates.Deactivated && this.didDrag && this.CurrentPress != null && this.CurrentPress.CanDrag)
		{
			Vector2 pos = this.CurrentPress.GetScreenPosition(Vector3.Zero);
			CursorUtility.SetPosition(pos);
		}
	}
}

public class GameOverlayRenderer : Renderer
{
	public readonly ForwardPass Forward = new();
	public readonly ForwardPass Interface = new();

	private readonly RendererStudioCamera camera = new();
	private readonly OverlayRendererInput input = new();
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
	public override RendererCamera Camera => this.camera;
	public override RendererInput Input => this.input;

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
		this.input.Attach();

		this.Services.Tick.Add(TickService.Channels.GameTick, this.OnGameTick);
		this.Services.Reshade.ReshadeBeforeEffects += this.OnBeforeReshadeRender;
		this.Services.Reshade.ReshadeAfterEffects += this.OnAfterReshadeRender;

		InterfaceManager.RunBeforeImGuiRender(this.OnBeforeImGuiRender);
		this.IsAttached = true;
	}

	public void Detach()
	{
		this.input.Detach();

		this.Services.Tick.Remove(TickService.Channels.GameTick, this.OnGameTick);
		this.Services.Reshade.ReshadeBeforeEffects -= this.OnBeforeReshadeRender;
		this.Services.Reshade.ReshadeAfterEffects -= this.OnAfterReshadeRender;

		this.IsAttached = false;
	}

	protected override bool TrySetUpRender()
	{
		if (this.Services.Studio.HideUi)
			return false;

		return base.TrySetUpRender();
	}

	protected override void RenderPass(RenderPassBase pass)
	{
		if (this.DeviceContext == null)
			return;

		if (!pass.IncludeInScreenshots && this.Services.Photos.IsCapturing)
			return;

		this.generateUiMaskPass.Bind(this.DeviceContext);
		base.RenderPass(pass);
	}

	protected unsafe override Texture2D? GetBackBuffer()
	{
		if (ServiceManager.ShutdownRequested)
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

	private void OnBeforeImGuiRender()
	{
		if (this.IsAttached)
		{
			this.needsImGuiRequeue = true;

			if (this.Services.Reshade.IsReshadeEnabled)
				return;

			this.SetUpRender();
			this.Input.Process(this);
			this.RenderUiMask();
			this.RenderBeforeEffectsPasses();
			this.RenderAfterEffectsPasses();
		}
	}

	private void OnBeforeReshadeRender()
	{
		this.SetUpRender();
		this.Input.Process(this);
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
			this.Log.Error(ex, $"Error in render Ui Mask");
			this.Detach();
		}
	}

	private void RenderBeforeEffectsPasses() => this.RenderPasses(this.beforeEffectsPasses);
	private void RenderAfterEffectsPasses() => this.RenderPasses(this.afterEffectsPasses);
}
