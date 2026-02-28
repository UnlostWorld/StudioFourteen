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

namespace StudioFourteen.Services.Avalonia.Platform;

using System;
using global::Avalonia;
using global::Avalonia.OpenGL.Egl;
using global::Avalonia.OpenGL.Surfaces;
using SharpDX.Direct3D11;
using SharpDX.DXGI;

public partial class DxgiRenderTarget(WindowImpl window, EglContext context)
	 : EglPlatformSurfaceRenderTargetBase(context)
{
	public Texture2D? Texture;
	public override bool IsCorrupted => base.IsCorrupted;

	public override void Dispose()
	{
		base.Dispose();
		this.Texture?.Dispose();
		this.Texture = null;
	}

	public unsafe override IGlPlatformSurfaceRenderingSession BeginDrawCore()
	{
		try
		{
			if (!Studio.Rendering.OverlayRenderer.CanRender)
				throw new Exception("Attempt to draw before renderer is ready");

			PixelSize size = new((int)(window.FrameSize?.Width ?? 256), (int)(window.FrameSize?.Height ?? 256));

			if (this.Texture != null
				&& (this.Texture.Description.Width != size.Width
					|| this.Texture.Description.Height != size.Height))
			{
				this.Texture.Dispose();
				this.Texture = null;
			}

			if (this.Texture == null)
			{
				Texture2DDescription desc = default;
				desc.Width = size.Width;
				desc.Height = size.Height;
				desc.Format = SharpDX.DXGI.Format.B8G8R8A8_UNorm;
				desc.MipLevels = 1;
				desc.ArraySize = 1;
				desc.Usage = ResourceUsage.Default;
				desc.BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource;
				desc.CpuAccessFlags = CpuAccessFlags.None;
				desc.OptionFlags = ResourceOptionFlags.Shared;
				desc.SampleDescription.Count = 1;
				desc.SampleDescription.Quality = 0;

				this.Texture = new(Studio.Rendering.OverlayRenderer.Device, desc);

				Studio.Log.Verbose($"Create new UI render target: {size}");
			}

			if (this.Texture == null)
			{
				return null!;
			}

			Resource1 resource = this.Texture.QueryInterface<SharpDX.DXGI.Resource1>();
			nint handle = resource.SharedHandle;
			if (handle == 0)
				throw new Exception("Failed to get shared handle to render texture");

			IDisposable contextLock = this.Context.EnsureCurrent();

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
				window.RenderScaling,
				() =>
				{
					surface.Dispose();
					contextLock.Dispose();
				},
				true);
		}
		catch (Exception ex)
		{
			Studio.Log.Error(ex, "Error in DxgiRenderTarget draw");
			throw;
		}
	}
}
