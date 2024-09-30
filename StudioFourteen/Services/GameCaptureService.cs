// Dalamud
// https://github.com/goatcorp/Dalamud/blob/master/Dalamud/Interface/Internal/SwapChainHelper.cs
// https://github.com/goatcorp/Dalamud/blob/master/Dalamud/Utility/TerraFxCom/TerraFxD3D11Extensions.cs
// https://github.com/goatcorp/Dalamud/blob/master/Dalamud/Interface/Internal/InterfaceManager.cs

namespace ScreenshotStudio.Services;

using Dalamud.Hooking;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Graphics.Kernel;
using FFXIVClientStructs.FFXIV.Client.Graphics.Render;
using ScreenshotStudio.Plugin;
using ScreenshotStudio.Utilities;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TerraFX.Interop.DirectX;
using TerraFX.Interop.Windows;

using Color = System.Windows.Media.Color;
using Image = SixLabors.ImageSharp.Image;
using Point = System.Windows.Point;

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
	private byte[] bufferBgraData = Array.Empty<byte>();
	private int bufferWidth = 0;
	private int bufferHeight = 0;
	private ComPtr<ID3D11Texture2D> bufferTexture = default;
	private int captureId = 0;

	private IntPtr pBuffer;
	private int bufferLength;

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

	public override Task Start()
	{
		if (SwapChainHelper.IsReshade)
		{
			this.reshadeOnPresentHook = InteropService.HookFromAddress<InterfaceManager.ReshadeOnPresentDelegate>(SwapChainHelper.ReshadeOnPresent, this.ReshadeOnPresentDetour);
			this.reshadeOnPresentHook?.Enable();

			InterfaceManager.DisableReshadePresent();
		}

		Thread conversionThread = new(new ThreadStart(this.ConversionThread));
		conversionThread.Start();

		return base.Start();
	}

	public override Task Stop()
	{
		if (SwapChainHelper.IsReshade)
		{
			InterfaceManager.EnableReshadePresent();
		}

		if (this.reshadeOnPresentHook != null && !this.reshadeOnPresentHook.IsDisposed)
			this.reshadeOnPresentHook.Dispose();

		this.bufferTexture.Dispose();

		return base.Stop();
	}

	public Image? ToImage()
	{
		lock (this.lockObj)
		{
			if (this.bufferWidth == 0 || this.bufferHeight == 0)
				return null;

			return Image.LoadPixelData<Bgra32>(this.bufferBgraData, this.bufferWidth, this.bufferHeight);
		}
	}

	public Color GetColor(Point point)
	{
		lock (this.lockObj)
		{
			int x = (int)point.X;
			int y = (int)point.Y;

			int index = (y * (this.bufferWidth * 4)) + (x * 4);

			if (index + 4 > this.bufferLength)
				return Colors.Transparent;

			byte b = this.bufferBgraData[index];
			byte g = this.bufferBgraData[index + 1];
			byte r = this.bufferBgraData[index + 2];

			return Color.FromArgb(255, r, g, b);
		}
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

			destination.WritePixels(new Int32Rect(0, 0, this.bufferWidth, this.bufferHeight), this.bufferBgraData, this.bufferWidth * 4, 0);
		}
	}

	protected override void OnFrameworkUpdate(IFramework framework)
	{
		// If not using reshade, fallback to just run before ImGUI within dalamud's present
		if (!SwapChainHelper.IsReshade)
		{
			InterfaceManager.RunBeforeImGuiRender(this.Capture);
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
				if (!this.Services.Studio.IsOpen)
					return;

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
				this.pBuffer = (IntPtr)mapped.pData;
				this.bufferLength = len;

				context.Get()->Unmap((ID3D11Resource*)buffer, 0u);
				this.captureId++;

				// how long you plan on keeping this open for?
				if (this.captureId >= int.MaxValue)
				{
					this.captureId = 0;
				}
			}
		}
		catch (Exception ex)
		{
			this.Log.Error(ex, "Error in graphics capture");
		}
	}

	// A thread responsible for converting captures from rgba32 to bgra32 for use in WPF.
	private void ConversionThread()
	{
		int lastCaptureId = 0;

		while (this.IsAlive)
		{
			Thread.Sleep(10);

			try
			{
				if (this.captureId == lastCaptureId)
					continue;

				lock (this.lockObj)
				{
					// https://stackoverflow.com/questions/21428272/show-rgba-image-from-memory
					int numPixels = this.bufferHeight * this.bufferWidth;
					unsafe
					{
						if (this.bufferBgraData.Length != this.bufferLength)
							this.bufferBgraData = new byte[this.bufferLength];

						fixed (byte* pDestData = &this.bufferBgraData[0])
						{
							uint* pCurrent = (uint*)this.pBuffer;
							uint* pBitmapData = (uint*)pDestData;

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
					}

					lastCaptureId = this.captureId;
				}

				foreach (ICaptureListener listener in this.listeners)
				{
					listener.OnCapture();
				}
			}
			catch(Exception ex)
			{
				this.Log.Error(ex, "Error processing game capture");
			}
		}
	}
}
