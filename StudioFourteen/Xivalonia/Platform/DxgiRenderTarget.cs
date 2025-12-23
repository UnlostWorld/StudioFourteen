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

namespace StudioFourteen.Xivalonia.Platform;

using System;
using Avalonia;
using Avalonia.OpenGL.Egl;
using Avalonia.OpenGL.Surfaces;
using SharpDX.Direct3D11;

[Logger]
[Services]
public partial class DxgiRenderTarget(WindowImpl window, EglContext context)
	 : EglPlatformSurfaceRenderTargetBase(context)
{
	public Texture2D? Texture;
	private Exception? lastException;

	public unsafe override IGlPlatformSurfaceRenderingSession BeginDrawCore()
	{
		if (this.lastException != null)
			throw this.lastException;

		try
		{
			if (window.IsDisposed)
				throw new Exception("Attempt to draw disposed window");

			if (RenderingService.OverlayRenderer.BackBuffer == null)
				throw new Exception("Attempt to draw before renderer is ready");

			PixelSize size = new((int)(window.FrameSize?.Width ?? 256), (int)(window.FrameSize?.Height ?? 256));

			if (this.Texture == null)
			{
				Texture2DDescription desc = RenderingService.OverlayRenderer.BackBuffer.Description;
				desc.Width = size.Width;
				desc.Height = size.Height;
				desc.Format = SharpDX.DXGI.Format.B8G8R8A8_UNorm;
				desc.MipLevels = 1;
				desc.ArraySize = 1;
				desc.Usage = ResourceUsage.Default;
				desc.BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource;
				desc.CpuAccessFlags = CpuAccessFlags.None;
				desc.OptionFlags = ResourceOptionFlags.Shared;

				this.Texture = new(RenderingService.OverlayRenderer.BackBuffer.Device, desc);
			}

			var resource = this.Texture.QueryInterface<SharpDX.DXGI.Resource1>();
			nint handle = resource.SharedHandle;
			if (handle == 0)
				throw new Exception("Failed tp get shared handle to render texture");

			int* attrs = stackalloc[]
			{
				EglConsts.EGL_WIDTH, size.Width,
				EglConsts.EGL_HEIGHT, size.Height,
				EglConsts.EGL_TEXTURE_TARGET, EglConsts.EGL_TEXTURE_2D,
				EglConsts.EGL_TEXTURE_FORMAT, EglConsts.EGL_TEXTURE_RGBA,
				EglConsts.EGL_NONE,
			};

			EglSurface? surface = this.Context.Display.CreatePBufferFromClientBuffer(
				EglConsts.EGL_D3D_TEXTURE_2D_SHARE_HANDLE_ANGLE,
				handle,
				attrs);

			return this.BeginDraw(
				surface,
				size,
				1,
				() =>
				{
					this.Log.Information($"drawn!");
					surface.Dispose();
				},
				true);
		}
		catch (Exception ex)
		{
			this.lastException = ex;
			this.Log.Error(ex, "Error in DxgiRenderTarget draw");
			throw;
		}
	}
}
