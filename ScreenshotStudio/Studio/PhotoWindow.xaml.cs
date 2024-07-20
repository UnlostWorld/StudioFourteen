// Dalamud
// https://github.com/goatcorp/Dalamud/blob/master/Dalamud/Interface/Internal/SwapChainHelper.cs
// https://github.com/goatcorp/Dalamud/blob/master/Dalamud/Utility/TerraFxCom/TerraFxD3D11Extensions.cs
// https://github.com/goatcorp/Dalamud/blob/master/Dalamud/Interface/Internal/InterfaceManager.cs

namespace ScreenshotStudio.Studio;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Graphics.Kernel;
using ScreenshotStudio.Plugin;
using ScreenshotStudio.Utilities;
using ScreenshotStudio.Windows;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TerraFX.Interop.DirectX;
using TerraFX.Interop.Windows;

public partial class PhotoWindow : PanelWindow
{
	private readonly WriteableBitmap bitmap = new WriteableBitmap(
					1920,
					1080,
					300,
					300,
					PixelFormats.Bgra32,
					null);

	private byte[] bufferData = Array.Empty<byte>();
	private int bufferWidth = 0;
	private int bufferHeight = 0;
	private ComPtr<ID3D11Texture2D> bufferTexture = default;

	public WriteableBitmap Bitmap => this.bitmap;

	protected override void OnOpened()
	{
		base.OnOpened();

		this.Dispatcher.BeginInvoke(this.DrawBitmap);
	}

	protected override void OnClosed()
	{
		base.OnClosed();

		this.bufferTexture.Dispose();
	}

	protected override void OnFrameworkUpdate(IFramework framework)
	{
		base.OnFrameworkUpdate(framework);

		InterfaceManager.RunBeforeImGuiRender(this.Capture);
	}

	private unsafe void Capture()
	{
		Threads.VerifyFrameworkThread();

		try
		{
			lock (this)
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
					HRESULT createResukt = device.Get()->CreateTexture2D(&description, null, this.bufferTexture.GetAddressOf());

					if (createResukt.FAILED)
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

	private void DrawBitmap()
	{
		lock (this)
		{
			Stopwatch sw = new();
			sw.Start();

			if (this.bufferWidth == 0 || this.bufferHeight == 0)
				return;

			if (this.bitmap.Width != this.bufferWidth || this.bitmap.Height != this.bufferHeight)
			{
				// Resize!
			}

			this.bitmap.Lock();

			// annoyingly, the buffer is in RGBA32, which is not supported by WPF here.
			for (var x = 0; x < this.bufferWidth; x++)
			{
				for (var y = 0; y < this.bufferHeight; y++)
				{
					var offset = ((y * this.bufferWidth) + x) * 4;

					IntPtr backbuffer = this.bitmap.BackBuffer;
					backbuffer += offset;

					var r = this.bufferData[offset];
					var g = this.bufferData[offset + 1];
					var b = this.bufferData[offset + 2];
					var a = this.bufferData[offset + 3];
					int color = a << 24 | r << 16 | g << 8 | b;

					Marshal.WriteInt32(backbuffer, color);
				}
			}

			this.bitmap.AddDirtyRect(new Int32Rect(0, 0, this.bufferWidth, this.bufferHeight));
			this.bitmap.Unlock();

			sw.Stop();
			this.Log.Information($"{sw.ElapsedMilliseconds}ms");
		}
	}
}
