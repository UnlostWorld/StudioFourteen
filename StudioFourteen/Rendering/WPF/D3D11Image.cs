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

namespace StudioFourteen.Rendering.WPF;

using System;
using System.Windows;
using System.Windows.Interop;

using Dx11Texture = SharpDX.Direct3D11.Texture2D;

using Dx9Device = SharpDX.Direct3D9.DeviceEx;
using Dx9Format = SharpDX.Direct3D9.Format;
using Dx9Pool = SharpDX.Direct3D9.Pool;
using Dx9Surface = SharpDX.Direct3D9.Surface;
using Dx9Texture = SharpDX.Direct3D9.Texture;
using Dx9Usage = SharpDX.Direct3D9.Usage;

using DxgiResource = SharpDX.DXGI.Resource;

public class D3D11Image : D3DImage, IDisposable
{
	private Dx9Texture? texture;
	private IntPtr textureSurfaceHandle;

	public D3D11Image(Dx9Device device, Dx11Texture renderTarget)
	{
		using DxgiResource resource = renderTarget.QueryInterface<DxgiResource>();
		nint handle = resource.SharedHandle;
		this.texture = new Dx9Texture(
			device,
			renderTarget.Description.Width,
			renderTarget.Description.Height,
			1,
			Dx9Usage.RenderTarget,
			Dx9Format.A8R8G8B8,
			Dx9Pool.Default,
			ref handle);

		using Dx9Surface surface = this.texture.GetSurfaceLevel(0);
		this.textureSurfaceHandle = surface.NativePointer;
		this.TrySetBackbufferPointer(this.textureSurfaceHandle);

		this.IsFrontBufferAvailableChanged += this.HandleIsFrontBufferAvailableChanged;
	}

	public void InvalidateRendering()
	{
		if (this.texture == null)
			return;

		this.Lock();
		this.AddDirtyRect(new Int32Rect(0, 0, this.PixelWidth, this.PixelHeight));
		this.Unlock();
	}

	public void TrySetBackbufferPointer(IntPtr ptr)
	{
		// TODO: use TryLock and check multithreading scenarios
		this.Lock();
		try
		{
			this.SetBackBuffer(D3DResourceType.IDirect3DSurface9, ptr);
		}
		finally
		{
			this.Unlock();
		}
	}

	public void Dispose()
	{
		if (this.texture == null)
			return;

		this.Dispatcher.BeginInvoke(
			() =>
			{
				this.IsFrontBufferAvailableChanged -= this.HandleIsFrontBufferAvailableChanged;
				this.texture.Dispose();
				this.texture = null;
				this.textureSurfaceHandle = IntPtr.Zero;

				this.TrySetBackbufferPointer(IntPtr.Zero);
			},
			System.Windows.Threading.DispatcherPriority.Send);
	}

	private void HandleIsFrontBufferAvailableChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		if (this.IsFrontBufferAvailable)
		{
			this.TrySetBackbufferPointer(this.textureSurfaceHandle);
		}
	}
}