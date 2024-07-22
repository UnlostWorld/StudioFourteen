// Dalamud
// https://github.com/goatcorp/Dalamud/blob/master/Dalamud/Interface/Internal/SwapChainHelper.cs
// https://github.com/goatcorp/Dalamud/blob/master/Dalamud/Utility/TerraFxCom/TerraFxD3D11Extensions.cs
// https://github.com/goatcorp/Dalamud/blob/master/Dalamud/Interface/Internal/InterfaceManager.cs

namespace ScreenshotStudio.Services;

using Dalamud.Hooking;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Graphics.Kernel;
using ScreenshotStudio.Plugin;
using ScreenshotStudio.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TerraFX.Interop.DirectX;
using TerraFX.Interop.Windows;

public interface ICaptureListener
{
	// Note: Invoked from the games main render thread. Don't do much work here.
	void OnCapture();
}

public class GameCaptureService : ServiceBase
{
	private readonly object lockObj = new();
	private readonly HashSet<ICaptureListener> listeners = new();

	private Hook<InterfaceManager.ReshadeOnPresentDelegate>? reshadeOnPresentHook;
	private byte[] bufferData = Array.Empty<byte>();
	private int bufferWidth = 0;
	private int bufferHeight = 0;
	private ComPtr<ID3D11Texture2D> bufferTexture = default;

	public void AddListener(ICaptureListener listener)
	{
		lock (this.lockObj)
		{
			this.listeners.Add(listener);
		}
	}

	public void RemoveListener(ICaptureListener listener)
	{
		lock (this.lockObj)
		{
			this.listeners.Remove(listener);
		}
	}

	public override Task Initialize()
	{
		if (DalamudServices.Framework != null)
			DalamudServices.Framework.Update += this.OnFrameworkUpdate;

		if (SwapChainHelper.IsReshade)
		{
			this.reshadeOnPresentHook = InteropService.HookFromAddress<InterfaceManager.ReshadeOnPresentDelegate>(SwapChainHelper.ReshadeOnPresent, this.ReshadeOnPresentDetour);
			this.reshadeOnPresentHook?.Enable();

			InterfaceManager.DisableReshadePresent();
		}

		return base.Initialize();
	}

	public override Task Shutdown()
	{
		if (SwapChainHelper.IsReshade)
		{
			InterfaceManager.EnableReshadePresent();
		}

		if (DalamudServices.Framework != null)
			DalamudServices.Framework.Update -= this.OnFrameworkUpdate;

		if (this.reshadeOnPresentHook != null && !this.reshadeOnPresentHook.IsDisposed)
			this.reshadeOnPresentHook.Dispose();

		this.bufferTexture.Dispose();

		return base.Shutdown();
	}

	/// <summary>
	/// Draw the contents of the latest game capture to the given bitmap.
	/// </summary>
	public unsafe void DrawBitmap(ref WriteableBitmap? destination)
	{
		lock (this.lockObj)
		{
			if (this.bufferWidth == 0 || this.bufferHeight == 0)
				return;

			if (destination == null || destination.PixelWidth != this.bufferWidth || destination.PixelHeight != this.bufferHeight)
			{
				destination = new WriteableBitmap(
					this.bufferWidth,
					this.bufferHeight,
					300,
					300,
					PixelFormats.Bgra32,
					null);
			}

			destination.Lock();

			// https://stackoverflow.com/questions/21428272/show-rgba-image-from-memory
			int numPixels = this.bufferHeight * this.bufferWidth;
			fixed (byte* pSrcData = &this.bufferData[0])
			{
				uint* pCurrent = (uint*)pSrcData;
				uint* pBitmapData = (uint*)destination.BackBuffer;

				for (int n = 0; n < numPixels; n++)
				{
					uint x = *(pCurrent++);

					// Swap R and B
					*(pBitmapData + n) =
							 0xFF000000 | // force alpha to 255
						(x & 0x00FF0000) >> 16 |
						(x & 0x0000FF00) |
						(x & 0x000000FF) << 16;
				}
			}

			destination.AddDirtyRect(new Int32Rect(0, 0, this.bufferWidth, this.bufferHeight));
			destination.Unlock();
		}
	}

	// When running reshade, we intercept the present call to capture the screen after reshade.
	// We also call into Dalamud's InterfaceManager detour to make sure the dalamud windows
	// get rendered _after_ our capture is complete, since we disable their hook.
	private void ReshadeOnPresentDetour(nint swapChain, uint flags, nint presentParams)
	{
		if (this.reshadeOnPresentHook == null)
			return;

		this.reshadeOnPresentHook.Original(swapChain, flags, presentParams);

		this.Capture();

		InterfaceManager.ReshadeOnPresentDetour(swapChain, flags, presentParams);
	}

	private void OnFrameworkUpdate(IFramework framework)
	{
		// If not using reshade, fallback to just run before ImGUI within dalamud's present
		if (!SwapChainHelper.IsReshade)
		{
			InterfaceManager.RunBeforeImGuiRender(this.Capture);
		}
	}

	/// <summary>
	/// Capture the contents of the games swap chain back buffer.
	/// </summary>
	private unsafe void Capture()
	{
		Threads.VerifyFrameworkThread();

		try
		{
			lock (this.lockObj)
			{
				// Don't capture if nothing is using the capture data.
				if (this.listeners.Count <= 0)
					return;

				var kernelDev = Device.Instance();
				if (kernelDev == null)
					return;

				var swapChain = kernelDev->SwapChain;
				if (swapChain == null)
					return;

				// BackBuffer should be something from IDXGISwapChain->GetBuffer, which means that IDXGISwapChain itself
				// must have been fully initialized.
				if (swapChain->BackBuffer == null)
					return;

				ID3D11Texture2D* buffer = (ID3D11Texture2D*)swapChain->BackBuffer->D3D11Texture2D;
				if (buffer == null)
					return;

				using ComPtr<ID3D11Device> device = default;
				buffer->GetDevice(device.GetAddressOf());
				if (device.Get() == null)
					return;

				D3D11_TEXTURE2D_DESC description;
				buffer->GetDesc(&description);

				if (description.Format != DXGI_FORMAT.DXGI_FORMAT_R8G8B8A8_UNORM)
					throw new Exception($"wrong format in buffer texture {description.Format}");

				this.bufferWidth = (int)description.Width;
				this.bufferHeight = (int)description.Height;

				if (this.bufferWidth == 0 || this.bufferHeight == 0)
					return;

				description.BindFlags = 0;
				description.CPUAccessFlags = (uint)D3D11_CPU_ACCESS_FLAG.D3D11_CPU_ACCESS_READ;
				description.Usage = D3D11_USAGE.D3D11_USAGE_STAGING;

				if (this.bufferTexture.Get() == null)
				{
					this.Log.Information("Creating a back buffer texture");
					HRESULT createResult = device.Get()->CreateTexture2D(&description, null, this.bufferTexture.GetAddressOf());

					if (createResult.FAILED)
					{
						throw new Exception("Failed to create texture");
					}
				}

				using ComPtr<ID3D11DeviceContext> context = default;
				device.Get()->GetImmediateContext(context.GetAddressOf());

				if (context.Get() == null)
					return;

				context.Get()->CopyResource((ID3D11Resource*)this.bufferTexture.Get(), (ID3D11Resource*)buffer);

				D3D11_MAPPED_SUBRESOURCE mapped = default(D3D11_MAPPED_SUBRESOURCE);
				HRESULT mapRsult = context.Get()->Map((ID3D11Resource*)this.bufferTexture.Get(), 0, D3D11_MAP.D3D11_MAP_READ, 0u, &mapped);
				if (mapRsult.FAILED)
					throw new Exception($"Failed to map texture resource");

				int len = this.bufferWidth * this.bufferHeight * 4;
				Span<byte> bufferPixels = new(mapped.pData, len);
				this.bufferData = bufferPixels.ToArray();

				context.Get()->Unmap((ID3D11Resource*)buffer, 0u);

				foreach (ICaptureListener listener in this.listeners)
				{
					listener.OnCapture();
				}
			}
		}
		catch (Exception ex)
		{
			this.Log.Error(ex, "Error in graphics capture");
		}
	}
}
