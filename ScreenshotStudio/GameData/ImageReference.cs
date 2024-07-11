namespace ScreenshotStudio.GameData;

using Lumina.Data.Files;
using ScreenshotStudio.Plugin;
using Serilog;
using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;

public class ImageReference
{
	protected readonly ILogger Log = Logging.ForContext<ImageReference>();

	private WeakReference<ImageSource>? cachedImage;
	private uint imageId;

	public ImageReference(uint imageId)
	{
		this.ImageId = imageId;
	}

	public ImageReference(ushort imageId)
	{
		this.ImageId = imageId;
	}

	public ImageReference(int imageId)
	{
		this.ImageId = (uint)imageId;
	}

	public uint ImageId
	{
		get => this.imageId;
		set
		{
			this.cachedImage = null;
			this.imageId = value;
		}
	}

	public ImageSource? Source
	{
		get
		{
			if (this.ImageId == 0)
				return null;

			ImageSource? img;
			if (this.cachedImage != null && this.cachedImage.TryGetTarget(out img))
			{
				return img;
			}

			try
			{
				this.Log.Verbose($"Loading image {this.ImageId}");

				////string path = $"ui/icon/{this.ImageId / 1000u * 1000:000000}/{this.ImageId:000000}.tex";
				string path = $"ui/icon/{this.ImageId / 1000u * 1000:000000}/{this.ImageId:000000}_hr1.tex";
				TexFile? tex = DalamudServices.DataManager?.GetFile<TexFile>(path);

				if (tex == null)
					return null;

				BitmapSource bmp = BitmapSource.Create(tex.Header.Width, tex.Header.Height, 96, 96, PixelFormats.Bgra32, null, tex.ImageData, tex.Header.Width * 4);
				bmp.Freeze();
				img = bmp;

				if (this.cachedImage == null)
					this.cachedImage = new WeakReference<ImageSource>(img);

				this.cachedImage.SetTarget(img);
				return img;
			}
			catch (Exception ex)
			{
				this.Log.Warning(ex, $"Failed to load Image: {this.ImageId} ");
			}

			return null;
		}
	}
}
