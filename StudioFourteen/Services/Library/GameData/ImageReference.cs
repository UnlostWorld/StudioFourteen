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

public class ImageReference
{
	private readonly string path;

	public ImageReference(string path)
	{
		this.path = path;
	}

	public ImageReference(uint imageId)
		: this($"ui/icon/{imageId / 1000u * 1000:000000}/{imageId:000000}_hr1.tex")
	{
	}

	public ImageReference(ushort imageId)
		: this((uint)imageId)
	{
	}

	public ImageReference(int imageId)
		: this((uint)imageId)
	{
	}

	/*public ImageSource? Source
	{
		get
		{
			ImageSource? img;
			if (this.cachedImage != null && this.cachedImage.TryGetTarget(out img))
			{
				return img;
			}

			try
			{
				this.Log.Verbose($"Loading image {this.path}");
				TexFile? tex = Studio.DataManager.GetFile<TexFile>(this.path);

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
				Studio.Log.Warning(ex, $"Failed to load Image: {this.path} ");
			}

			return null;
		}
	}*/
}
