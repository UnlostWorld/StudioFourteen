// Dalamud
// https://github.com/goatcorp/Dalamud/blob/master/Dalamud/Interface/Internal/SwapChainHelper.cs
// https://github.com/goatcorp/Dalamud/blob/master/Dalamud/Utility/TerraFxCom/TerraFxD3D11Extensions.cs
// https://github.com/goatcorp/Dalamud/blob/master/Dalamud/Interface/Internal/InterfaceManager.cs

namespace ScreenshotStudio.Studio;

using Dalamud.Hooking;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Graphics.Kernel;
using ScreenshotStudio.Plugin;
using ScreenshotStudio.Services;
using ScreenshotStudio.Utilities;
using ScreenshotStudio.Windows;
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TerraFX.Interop.DirectX;
using TerraFX.Interop.Windows;

public partial class PhotoWindow : PanelWindow
{
	private readonly object lockObj = new();
	private WriteableBitmap? bitmap;
	private nint bitmapBackBuffer;
	private byte[] bufferData = Array.Empty<byte>();
	private int bufferWidth = 0;
	private int bufferHeight = 0;
	private ComPtr<ID3D11Texture2D> bufferTexture = default;
	private Hook<InterfaceManager.ReshadeOnPresentDelegate>? reshadeOnPresentHook;

	[AutoNotify] public WriteableBitmap? Bitmap => this.bitmap;

	protected override void OnOpened()
	{
		if (SwapChainHelper.IsReshade)
		{
			InterfaceManager.DisableReshadePresent();

			this.reshadeOnPresentHook = InteropService.HookFromAddress<InterfaceManager.ReshadeOnPresentDelegate>(SwapChainHelper.ReshadeOnPresent, this.ReshadeOnPresentDetour);
			this.reshadeOnPresentHook?.Enable();
		}

		base.OnOpened();
	}

	protected override void OnClosed()
	{
		base.OnClosed();

		this.reshadeOnPresentHook?.Disable();
		this.bufferTexture.Dispose();

		if (SwapChainHelper.IsReshade)
		{
			InterfaceManager.EnableReshadePresent();
		}
	}

	protected override void OnFrameworkUpdate(IFramework framework)
	{
		base.OnFrameworkUpdate(framework);

		// If not using reshade, fallback to just run before ImGUI within dalamud's present
		if (!SwapChainHelper.IsReshade)
		{
			InterfaceManager.RunBeforeImGuiRender(this.Capture);
		}

		this.Dispatcher.BeginInvoke(this.DrawBitmap);
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
			}
		}
		catch (Exception ex)
		{
			this.Log.Error(ex, "Error in graphics capture");
		}
	}

	/// <summary>
	/// Draw the contents of the buffer to the bitmap being displayed in the window.
	/// </summary>
	private unsafe void DrawBitmap()
	{
		lock (this.lockObj)
		{
			if (this.bufferWidth == 0 || this.bufferHeight == 0)
				return;

			if (this.bitmap == null || this.bitmap.PixelWidth != this.bufferWidth || this.bitmap.PixelHeight != this.bufferHeight)
			{
				this.bitmap = new WriteableBitmap(
					this.bufferWidth,
					this.bufferHeight,
					300,
					300,
					PixelFormats.Bgra32,
					null);

				this.bitmapBackBuffer = this.bitmap.BackBuffer;
			}

			this.bitmap.Lock();

			// https://stackoverflow.com/questions/21428272/show-rgba-image-from-memory
			int numPixels = this.bufferHeight * this.bufferWidth;
			fixed (byte* pSrcData = &this.bufferData[0])
			{
				uint* pCurrent = (uint*)pSrcData;
				uint* pBitmapData = (uint*)this.bitmapBackBuffer;

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

			this.bitmap.AddDirtyRect(new Int32Rect(0, 0, this.bufferWidth, this.bufferHeight));
			this.bitmap.Unlock();
		}
	}
}
