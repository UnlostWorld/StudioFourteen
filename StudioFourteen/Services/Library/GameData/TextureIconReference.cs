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

namespace StudioFourteen.Services.Library.GameData;

using System;
using System.Runtime.InteropServices;
using global::Avalonia;
using global::Avalonia.Media.Imaging;
using global::Avalonia.Platform;
using Lumina.Data.Files;

public class TextureIconReference
{
	private WeakReference<Bitmap>? cachedImage;

	public TextureIconReference(string path)
	{
		this.Path = path;
	}

	public TextureIconReference(uint imageId)
		: this($"ui/icon/{imageId / 1000u * 1000:000000}/{imageId:000000}_hr1.tex")
	{
	}

	public TextureIconReference(ushort imageId)
		: this((uint)imageId)
	{
	}

	public TextureIconReference(int imageId)
		: this((uint)imageId)
	{
	}

	public string Path { get; init; }

	public Bitmap? Source
	{
		get
		{
			if (this.cachedImage != null && this.cachedImage.TryGetTarget(out var img))
			{
				return img;
			}

			try
			{
				TexFile? tex = Studio.DataManager.GetFile<TexFile>(this.Path);

				if (tex == null)
					return null;

				Vector dpi = new Vector(96, 96);

				WriteableBitmap bitmap = new WriteableBitmap(
					new PixelSize(tex.Header.Width, tex.Header.Height),
					dpi,
					PixelFormat.Bgra8888,
					AlphaFormat.Unpremul);

				using (var frameBuffer = bitmap.Lock())
				{
					Marshal.Copy(tex.ImageData, 0, frameBuffer.Address, tex.ImageData.Length);
				}

				if (this.cachedImage == null)
					this.cachedImage = new WeakReference<Bitmap>(bitmap);

				this.cachedImage.SetTarget(bitmap);
				return bitmap;
			}
			catch (Exception ex)
			{
				Studio.Log.Warning(ex, $"Failed to load Image: {this.Path}");
			}

			return null;
		}
	}
}
