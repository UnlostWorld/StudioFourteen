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

using System;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using StudioFourteen.Services.Content;
using StudioFourteen.Services.Rendering;
using StudioFourteen.Services.Rendering.Draw;
using StudioFourteen.Services.Rendering.Materials;
using StudioFourteen.Services.Rendering.Passes;

using Device = SharpDX.Direct3D11.Device;

public class SaveTexturePass(Texture2D texture)
	: InstanceRenderPassBase<SaveTexturePass.PassDataStruct>
{
	public Image? Image;

	public unsafe override void Render(Renderer renderer, Device device, DeviceContext deviceContext)
	{
		base.Render(renderer, device, deviceContext);
		this.Image = this.Convert(device, texture);
	}

	private Image Convert(Device device, Texture2D texture)
	{
		Texture2DDescription desc = texture.Description;
		desc.BindFlags = 0;
		desc.CpuAccessFlags = CpuAccessFlags.Read;
		desc.Usage = ResourceUsage.Staging;

		using Texture2D bufferTexture = new(device, desc);

		device.ImmediateContext.CopyResource(texture, bufferTexture);

		DataStream stream;

		DataBox data = device.ImmediateContext.MapSubresource(
			bufferTexture,
			0,
			MapMode.Read,
			SharpDX.Direct3D11.MapFlags.None,
			out stream);

		if (data.IsEmpty)
			throw new Exception("Failed to map subresource for back buffer");

		using MemoryStream ms = new();
		stream.CopyTo(ms);
		byte[] pixels = ms.ToArray();

		if (texture.Description.Format == SharpDX.DXGI.Format.R8G8B8A8_UNorm)
		{
			return Image.LoadPixelData<Rgba32>(pixels, texture.Description.Width, texture.Description.Height);
		}
		else if (texture.Description.Format == SharpDX.DXGI.Format.R16G16B16A16_Float)
		{
			return Image.LoadPixelData<Rgba64>(pixels, texture.Description.Width, texture.Description.Height);
		}
		else if (texture.Description.Format == SharpDX.DXGI.Format.B8G8R8A8_UNorm)
		{
			return Image.LoadPixelData<Bgra32>(pixels, texture.Description.Width, texture.Description.Height);
		}
		else if (texture.Description.Format == SharpDX.DXGI.Format.R16G16_Float)
		{
			return Image.LoadPixelData<Rg32>(pixels, texture.Description.Width, texture.Description.Height);
		}

		throw new NotImplementedException($"No support for {texture.Description.Format} buffers");
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct PassDataStruct
	{
		public Vector4 Unused;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct Effect : IMaterial
	{
		public Vector4 Unused;

		public IContent<ShaderBytecode>? GetVertexShader() => new ShaderReference("Shaders/Effect_Copy.hlsl", "vs_4_0", "vert");
		public IContent<ShaderBytecode>? GetPixelShader() => new ShaderReference("Shaders/Effect_Copy.hlsl", "ps_4_0", "pixel");
		public IContent<ShaderBytecode>? GetGeometryShader() => null;

		public void Initialize()
		{
		}
	}
}