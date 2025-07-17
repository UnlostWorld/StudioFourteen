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

namespace StudioFourteen.Rendering.Passes;

using System;
using System.IO;
using FFXIVClientStructs.FFXIV.Client.Graphics.Render;
using SharpDX;
using SharpDX.Direct3D11;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

#pragma warning disable

public class CapturePass : RenderPassBase
{
	public CapturePass()
	{
		this.IncludeInScreenshots = true;
	}

	public Image? Capture { get; private set; }
	public Image? DepthCapture { get; private set; }

	public void DoCapture()
	{
		this.Capture = null;
		this.DepthCapture = null;
	}

	public unsafe override void Render(Renderer renderer, Device device, DeviceContext deviceContext)
	{
		if (renderer.BackBuffer == null)
			return;

		if (this.Capture == null)
		{
			this.Capture = this.CaptureTexture(device, renderer.BackBuffer);
		}

		// TODO: Use a BLIt to unpack the depth into something we can actually save.
		/*if (this.DepthCapture == null)
		{
			RenderTargetManagerEx* pRenderTargetManager = RenderTargetManagerEx.Instance();
			if (pRenderTargetManager == null)
				return;

			Texture2D depthStencilTexture = new((nint)pRenderTargetManager->DepthStencil->D3D11Texture2D);
			this.DepthCapture = this.CaptureTexture(device, depthStencilTexture);
		}*/
	}

	private unsafe Image CaptureTexture(Device device, Texture2D texture)
	{
		Texture2DDescription desc = texture.Description;
		desc.BindFlags = 0;
		desc.CpuAccessFlags = CpuAccessFlags.Read;
		desc.Usage = ResourceUsage.Staging;

		using Texture2D bufferTexture = new Texture2D(device, desc);

		device.ImmediateContext.CopyResource(texture, bufferTexture);

		DataStream stream;

		DataBox data = device.ImmediateContext.MapSubresource(bufferTexture, 0, MapMode.Read, MapFlags.None, out stream);
		if (data.IsEmpty)
			throw new Exception("Failed to map subresource for back buffer");

		if (texture.Description.Format == SharpDX.DXGI.Format.R8G8B8A8_UNorm)
		{
			using MemoryStream ms = new();
			stream.CopyTo(ms);
			byte[] pixels = ms.ToArray();
			return Image.LoadPixelData<Rgba32>(pixels, texture.Description.Width, texture.Description.Height);
		}
		else if (texture.Description.Format == SharpDX.DXGI.Format.R16G16B16A16_Float)
		{
			using MemoryStream ms = new();
			stream.CopyTo(ms);
			byte[] pixels = ms.ToArray();
			return Image.LoadPixelData<Rgba64>(pixels, texture.Description.Width, texture.Description.Height);
		}
		else if (texture.Description.Format == SharpDX.DXGI.Format.B8G8R8A8_UNorm)
		{
			using MemoryStream ms = new();
			stream.CopyTo(ms);
			byte[] pixels = ms.ToArray();
			return Image.LoadPixelData<Bgra32>(pixels, texture.Description.Width, texture.Description.Height);
		}
		else if (texture.Description.Format == SharpDX.DXGI.Format.R16G16_Float)
		{
			using MemoryStream ms = new();
			stream.CopyTo(ms);
			byte[] pixels = ms.ToArray();
			return Image.LoadPixelData<Rg32>(pixels, texture.Description.Width, texture.Description.Height);
		}

		throw new NotImplementedException($"No support for {texture.Description.Format} buffers");
	}
}