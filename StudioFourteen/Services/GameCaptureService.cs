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

namespace StudioFourteen.Services;

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using SharpDX;
using SharpDX.Direct3D11;
using SixLabors.ImageSharp.PixelFormats;
using StudioFourteen.Interop;
using StudioFourteen.Plugin;
using StudioFourteen.Utilities;

using Color = System.Windows.Media.Color;
using Device = SharpDX.Direct3D11.Device;
using Image = SixLabors.ImageSharp.Image;
using Point = System.Windows.Point;
using XivDevice = FFXIVClientStructs.FFXIV.Client.Graphics.Kernel.Device;

public interface ICaptureListener
{
	// Note: Invoked from the games main render thread. Don't do much work here.
	void OnCapture();
}
#pragma warning disable
public class GameCaptureService : ServiceBase
{
	private readonly object lockObj = new();
	private readonly HashSet<ICaptureListener> listeners = new();

	private byte[] bufferBgraData = Array.Empty<byte>();

	private int backBufferWidth = 0;
	private int backBufferHeight = 0;
	private Texture2D? backBufferTexture;
	private IntPtr pBackBuffer;
	private int backBufferLength;

	private byte[] depthBufferFloatData = Array.Empty<byte>();
	private int depthBufferWidth = 0;
	private int depthBufferHeight = 0;
	private Texture2D? depthBufferTexture;
	private IntPtr pDepthBuffer;
	private int depthBufferLength;

	private int captureId = 0;
	private int convertId = 0;
	private bool forceCapture = false;

	public float DepthMinimum { get; set; } = 0.0f;
	public float DepthMaximum { get; set; } = 1.0f;

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

	/*public override void Attach()
	{
		base.Attach();

		this.Services.Tick.Add(TickService.Channels.GameTick, this.OnGameTick);

		if (SwapChainHelper.IsReshade)
		{
			Hooks.ReshadeOnPresent.Enable(this.ReshadeOnPresentDetour);
			InterfaceManager.DisableReshadePresent();
		}

		////Thread conversionThread = new(new ThreadStart(this.ConversionThread));
		////conversionThread.Start();
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

		this.backBufferTexture?.Dispose();
		this.backBufferTexture = null;
		this.depthBufferTexture?.Dispose();
		this.depthBufferTexture = null;
	}*/

	public async Task<(Image? BackBuffer, Image? DepthBuffer)> ToImage()
	{
		if (this.listeners.Count <= 0)
		{
			// If we have no listeners, then we won't be capturing anything,
			// so perform a capture manually.
			this.forceCapture = true;
			int waitForId = this.captureId + 10;
			while (this.convertId < waitForId)
				await Task.Delay(100);

			this.forceCapture = false;
		}

		lock (this.lockObj)
		{
			Image? backBuffer = null;
			Image? depthBuffer = null;

			if (this.backBufferWidth != 0 && this.backBufferHeight != 0 && this.bufferBgraData.Length > 0)
				backBuffer = Image.LoadPixelData<Bgra32>(this.bufferBgraData, this.backBufferWidth, this.backBufferHeight);

			if (this.depthBufferWidth != 0 && this.depthBufferHeight != 0 && this.depthBufferFloatData.Length > 0)
				depthBuffer = Image.LoadPixelData<GreyscaleFloat>(this.depthBufferFloatData, this.depthBufferWidth, this.depthBufferHeight);

			return (backBuffer, depthBuffer);
		}
	}

	public Color GetColor(Point point)
	{
		lock (this.lockObj)
		{
			int x = (int)point.X;
			int y = (int)point.Y;

			int index = (y * (this.backBufferWidth * 4)) + (x * 4);

			if (index + 4 > this.backBufferLength)
				return Colors.Transparent;

			byte b = this.bufferBgraData[index];
			byte g = this.bufferBgraData[index + 1];
			byte r = this.bufferBgraData[index + 2];

			return Color.FromArgb(255, r, g, b);
		}
	}

	/// <summary>
	/// Draw the contents of the latest back capture to the given bitmap.
	/// </summary>
	public unsafe void DrawBackBufferToBitmap(ref WriteableBitmap? destination)
	{
		lock (this.lockObj)
		{
			if (this.backBufferWidth == 0 || this.backBufferHeight == 0)
				return;

			if (destination == null
				|| destination.PixelWidth != this.backBufferWidth
				|| destination.PixelHeight != this.backBufferHeight
				|| destination.Format != PixelFormats.Bgra32)
			{
				destination = new WriteableBitmap(
					this.backBufferWidth,
					this.backBufferHeight,
					300,
					300,
					PixelFormats.Bgra32,
					null);
			}

			destination.WritePixels(new Int32Rect(0, 0, this.backBufferWidth, this.backBufferHeight), this.bufferBgraData, this.backBufferWidth * 4, 0);
		}
	}

	/// <summary>
	/// Draw the contents of the latest depth capture to the given bitmap.
	/// </summary>
	public unsafe void DrawDepthBufferToBitmap(ref WriteableBitmap? destination)
	{
		lock (this.lockObj)
		{
			if (this.depthBufferWidth == 0 || this.depthBufferHeight == 0)
				return;

			if (destination == null
				|| destination.PixelWidth != this.depthBufferWidth
				|| destination.PixelHeight != this.depthBufferHeight
				|| destination.Format != PixelFormats.Gray32Float)
			{
				destination = new WriteableBitmap(
					this.depthBufferWidth,
					this.depthBufferHeight,
					300,
					300,
					PixelFormats.Gray32Float,
					null);
			}

			destination.WritePixels(new Int32Rect(0, 0, this.depthBufferWidth, this.depthBufferHeight), this.depthBufferFloatData, this.depthBufferWidth * 4, 0);
		}
	}

	protected void OnGameTick()
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
		Hooks.ReshadeOnPresent.Original(swapChain, flags, presentParams);

		this.Capture();

		InterfaceManager.ReshadeOnPresentDetour(swapChain, flags, presentParams);
	}

	/// <summary>
	/// Capture the contents of the games swap chain back buffer.
	/// </summary>
	private unsafe void Capture()
	{
		if (ServiceManager.ShutdownRequested)
			return;

		if (!this.Services.Studio.IsOpen)
			return;

		if (this.listeners.Count <= 0 && !this.forceCapture)
			return;

		this.CaptureBack();
		////this.CaptureDepth();

		this.captureId++;

		// how long you plan on keeping this open for?
		if (this.captureId >= int.MaxValue)
		{
			this.captureId = 0;
		}
	}

	private unsafe void CaptureBack()
	{
		TickService.VerifyGameTickThread();

		try
		{
			lock (this.lockObj)
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

				Texture2D backBuffer = (Texture2D)(nint)swapChain->BackBuffer->D3D11Texture2D;
				if (backBuffer == null)
					return;

				Device device = backBuffer.Device;
				if (device == null)
					return;

				Texture2DDescription description = backBuffer.Description;

				if (description.Format != SharpDX.DXGI.Format.R8G8B8A8_UNorm)
					throw new Exception($"wrong format in back buffer texture {description.Format}");

				bool sizeDirty =
					this.backBufferWidth != (int)description.Width
					|| this.backBufferHeight != (int)description.Height;

				this.backBufferWidth = (int)description.Width;
				this.backBufferHeight = (int)description.Height;

				if (this.backBufferWidth == 0 || this.backBufferHeight == 0)
					return;

				description.BindFlags = 0;
				description.CpuAccessFlags = CpuAccessFlags.Read;
				description.Usage = ResourceUsage.Staging;

				if (sizeDirty || this.backBufferTexture == null)
				{
					this.backBufferTexture?.Dispose();

					this.Log.Information("Creating a back buffer texture");
					this.backBufferTexture = new Texture2D(device, description);
				}

				DeviceContext context = device.ImmediateContext;
				if (context == null)
					return;

				context.CopyResource(backBuffer, this.backBufferTexture);

				DataStream stream;

				DataBox data = context.MapSubresource(this.backBufferTexture, 0, MapMode.Read, MapFlags.None, out stream);
				if (data.IsEmpty)
					throw new Exception("Failed to map subresource for back buffer");

				int len = this.backBufferWidth * this.backBufferHeight * 4;
				this.pBackBuffer = data.DataPointer;
				this.backBufferLength = len;

				using MemoryStream ms = new();
				stream.CopyTo(ms);
				this.bufferBgraData = ms.ToArray();

				for (int i = 0; i < len; i++)
				{
					if (this.bufferBgraData[i] == 0)
						this.bufferBgraData[i] = 255;
				}

				// Convert the back buffer ARGB32 to BGRA32
				// https://stackoverflow.com/questions/21428272/show-rgba-image-from-memory
				/*int numPixels = this.backBufferHeight * this.backBufferWidth;
				if (numPixels > 0 && this.backBufferLength > 0)
				{
					unsafe
					{
						if (this.bufferBgraData.Length != this.backBufferLength)
							this.bufferBgraData = new byte[this.backBufferLength];

						fixed (byte* pDestData = &this.bufferBgraData[0])
						{
							uint* pCurrent = (uint*)this.pBackBuffer;
							uint* pBitmapData = (uint*)pDestData;

							for (int n = 0; n < numPixels; n++)
							{
								uint x = *(pCurrent++);

								if (x != 0)
									x = 1;

								// Swap R and B
								*(pBitmapData + n) =
									0xFF000000 | // force alpha to 255
									((x & 0x00FF0000) >> 16) |
									(x & 0x0000FF00) |
									((x & 0x000000FF) << 16);
							}
						}
					}
				}*/

				context.UnmapSubresource(this.backBufferTexture, 0);
				this.convertId = this.captureId;

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

	/// <summary>
	/// Capture the contents of the games depth buffer.
	/// </summary>
	/*private unsafe void CaptureDepth()
	{
		TickService.VerifyGameTickThread();

		try
		{
			lock (this.lockObj)
			{
				// would be real nice if we could get the depth buffer address directly, instead
				// of relying on the reshade add-on to find it for us.
				if (!this.Services.Reshade.IsReshade)
					return;

				if (this.Services.Reshade.DepthBufferAddress == 0)
					return;

				Texture2D buffer = (Texture2D)this.Services.Reshade.DepthBufferAddress;
				if (buffer == null)
					return;

				Device device = buffer.Device;
				if (device == null)
					return;

				Texture2DDescription description = buffer.Description;

				if (description.Format != SharpDX.DXGI.Format.R24G8_Typeless)
					throw new Exception($"wrong format in depth buffer texture {description.Format}");

				bool sizeDirty =
						this.depthBufferWidth != (int)description.Width
						|| this.depthBufferHeight != (int)description.Height;

				this.depthBufferWidth = (int)description.Width;
				this.depthBufferHeight = (int)description.Height;

				if (this.depthBufferWidth == 0 || this.depthBufferHeight == 0)
					return;

				description.BindFlags = 0;
				description.CpuAccessFlags = CpuAccessFlags.Read;
				description.Usage = ResourceUsage.Staging;

				if (sizeDirty || this.depthBufferTexture == null)
				{
					this.depthBufferTexture?.Dispose();

					this.Log.Information("Creating a depth buffer texture");
					this.depthBufferTexture = new(device, description);
				}

				DeviceContext context = device.ImmediateContext;
				if (context == null)
					return;

				context.CopyResource(this.depthBufferTexture, buffer);

				DataBox data = context.MapSubresource(this.depthBufferTexture, 0, MapMode.Read, MapFlags.DoNotWait);
				if (data.IsEmpty)
					throw new Exception("Failed to map subresource for depth");

				int len = this.depthBufferWidth * this.depthBufferHeight * 4;
				this.pDepthBuffer = data.DataPointer;
				this.depthBufferLength = len;

				context.UnmapSubresource(buffer, 0);
			}
		}
		catch (Exception ex)
		{
			this.Log.Error(ex, "Error in graphics capture");
		}
	}*/

	// A thread responsible for converting captures from rgba32 to bgra32 for use in WPF.
	/*private void ConversionThread()
	{
		while (this.IsAlive && this.IsAttached)
		{
			Thread.Sleep(10);

			try
			{
				if (this.captureId == this.convertId)
					continue;

				lock (this.lockObj)
				{
					int convertingCaptureId = this.captureId;

					// Convert the back buffer ARGB32 to BGRA32
					// https://stackoverflow.com/questions/21428272/show-rgba-image-from-memory
					int numPixels = this.backBufferHeight * this.backBufferWidth;
					if (numPixels > 0 && this.backBufferLength > 0)
					{
						unsafe
						{
							if (this.bufferBgraData.Length != this.backBufferLength)
								this.bufferBgraData = new byte[this.backBufferLength];

							fixed (byte* pDestData = &this.bufferBgraData[0])
							{
								uint* pCurrent = (uint*)this.pBackBuffer;
								uint* pBitmapData = (uint*)pDestData;

								for (int n = 0; n < numPixels; n++)
								{
									uint x = *(pCurrent++);

									// Swap R and B
									*(pBitmapData + n) =
										0xFF000000 | // force alpha to 255
										((x & 0x00FF0000) >> 16) |
										(x & 0x0000FF00) |
										((x & 0x000000FF) << 16);
								}
							}
						}
					}

					// Convert the depth stencil buffer R24G8 to Floats
					numPixels = this.depthBufferHeight * this.depthBufferWidth;
					if (numPixels > 0 && this.depthBufferLength > 0)
					{
						unsafe
						{
							if (this.depthBufferFloatData.Length != this.depthBufferLength)
								this.depthBufferFloatData = new byte[this.depthBufferLength];

							fixed (byte* pDestData = &this.depthBufferFloatData[0])
							{
								uint* pCurrent = (uint*)this.pDepthBuffer;
								float* pBitmapData = (float*)pDestData;

								for (int n = 0; n < numPixels; n++)
								{
									uint x = *(pCurrent++);
									*(pBitmapData + n) = (x & 0x00FFFFFF) / (float)0x00FFFFFF;

									float v = *(pBitmapData + n);
									v = MathUtility.InverseLerp(this.DepthMinimum, this.DepthMaximum, v);
									*(pBitmapData + n) = v;
								}
							}
						}
					}

					this.convertId = convertingCaptureId;
				}

				foreach (ICaptureListener listener in this.listeners)
				{
					listener.OnCapture();
				}
			}
			catch (Exception ex)
			{
				// Abort the conversion thread.
				this.Log.Error(ex, "Error processing game capture");
				this.convertId = int.MaxValue;
				return;
			}
		}
	}*/
}
