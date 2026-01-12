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

namespace StudioFourteen.Services.Xivalonia.Platform;

using System;
using Avalonia;
using Avalonia.OpenGL.Egl;
using Avalonia.OpenGL.Surfaces;
using SharpDX.Direct3D11;

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

			if (!Studio.Rendering.OverlayRenderer.CanRender)
				throw new Exception("Attempt to draw before renderer is ready");

			PixelSize size = new((int)(window.FrameSize?.Width ?? 256), (int)(window.FrameSize?.Height ?? 256));

			if (this.Texture == null)
			{
				Texture2DDescription desc = Studio.Rendering.OverlayRenderer.BackBuffer!.Description;
				desc.OptionFlags = ResourceOptionFlags.Shared;

				this.Texture = new(Studio.Rendering.OverlayRenderer.Device, desc);
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
					Studio.Log.Information($"drawn!");
					surface.Dispose();
				},
				true);
		}
		catch (Exception ex)
		{
			this.lastException = ex;
			Studio.Log.Error(ex, "Error in DxgiRenderTarget draw");
			throw;
		}
	}
}
