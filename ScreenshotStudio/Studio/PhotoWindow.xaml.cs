// Dalamud
// https://github.com/goatcorp/Dalamud/blob/master/Dalamud/Interface/Internal/SwapChainHelper.cs
// https://github.com/goatcorp/Dalamud/blob/master/Dalamud/Utility/TerraFxCom/TerraFxD3D11Extensions.cs

namespace ScreenshotStudio.Studio;

using Dalamud.Interface.Textures;
using FFXIVClientStructs.FFXIV.Client.Graphics.Kernel;
using ImGuiNET;
using Lumina.Models.Materials;
using ScreenshotStudio.Plugin;
using ScreenshotStudio.Utilities;
using ScreenshotStudio.Windows;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TerraFX.Interop.DirectX;
using TerraFX.Interop.Windows;

public partial class PhotoWindow : PanelWindow
{
	protected override void OnOpened()
	{
		Threads.RunOnFrameworkThread(this.Try);
		base.OnOpened();
	}

	private unsafe void Try()
	{
		try
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

			description.BindFlags = 0;
			description.CPUAccessFlags = (uint)D3D11_CPU_ACCESS_FLAG.D3D11_CPU_ACCESS_READ;
			description.Usage = D3D11_USAGE.D3D11_USAGE_STAGING;
			description.Format = DXGI_FORMAT.DXGI_FORMAT_R8G8B8A8_UNORM;

			using ComPtr<ID3D11Texture2D> tmpTex = default;
			HRESULT hr = device.Get()->CreateTexture2D(&description, null, tmpTex.GetAddressOf());

			if (hr.FAILED)
				throw new Exception("Failed to create texture");

			using ComPtr<ID3D11DeviceContext> context = default;
			device.Get()->GetImmediateContext(context.GetAddressOf());

			context.Get()->CopyResource((ID3D11Resource*)tmpTex.Get(), (ID3D11Resource*)buffer);

			D3D11_MAPPED_SUBRESOURCE mapped = default(D3D11_MAPPED_SUBRESOURCE);
			hr = context.Get()->Map((ID3D11Resource*)tmpTex.Get(), 0, D3D11_MAP.D3D11_MAP_READ, 0u, &mapped);
			if (hr.FAILED)
				throw new Exception($"Failed to map texture resource {hr.Value}");

			int len = (int)description.Width * (int)description.Height * 4;

			BitmapSource bitmapSource = BitmapSource.Create(
				(int)description.Width,
				(int)description.Height,
				72,
				72,
				PixelFormats.Bgra32,
				BitmapPalettes.WebPalette,
				(nint)mapped.pData,
				len,
				4 * (int)description.Width);

			bitmapSource.Freeze();

			context.Get()->Unmap((ID3D11Resource*)buffer, 0u);

			this.Dispatcher.Invoke(() =>
			{
				this.Screen.Source = bitmapSource;
			});
		}
		catch (Exception ex)
		{
			this.Log.Error(ex, "Error in graphics capture");
		}
	}
}
