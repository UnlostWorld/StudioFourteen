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

namespace StudioFourteen.GameData;

using Lumina.Data.Files;
using StudioFourteen.Plugin;
using Serilog;
using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Input;
using WpfUtils.Commands;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Formats.Png;
using StudioFourteen.Files;
using Microsoft.Win32;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Bmp;

public class ImageReference
{
	protected readonly ILogger Log = Logging.ForContext<ImageReference>();

	private WeakReference<ImageSource>? cachedImage;
	private uint imageId;

	public ImageReference(uint imageId)
	{
		this.ImageId = imageId;

		this.ExportCommand = new SimpleCommand(this.Export);
	}

	public ImageReference(ushort imageId)
		: this((uint)imageId)
	{
	}

	public ImageReference(int imageId)
		: this((uint)imageId)
	{
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
				TexFile? tex = ServiceManager.Instance.GameData.GetFile<TexFile>(path);

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

	public bool CanExport => true;
	public ICommand ExportCommand { get; init; }

	private void Export()
	{
		string path = $"ui/icon/{this.ImageId / 1000u * 1000:000000}/{this.ImageId:000000}_hr1.tex";
		TexFile? tex = ServiceManager.Instance.GameData.GetFile<TexFile>(path);

		if (tex == null)
			return;

		PngEncoder encoder = new()
		{
			ColorType = PngColorType.RgbWithAlpha,
			TransparentColorMode = PngTransparentColorMode.Preserve,
			CompressionLevel = PngCompressionLevel.NoCompression,
		};

		using Image image = Image.LoadPixelData<Bgra32>(tex.ImageData, tex.Header.Width, tex.Header.Height);

		SaveFileDialog dlg = new();
		dlg.Filter = "Image|*.png";
		dlg.FileName = $"{this.ImageId:000000}_hr1";
		if (dlg.ShowDialog() == false)
			return;

		image.SaveAsPng(dlg.FileName, encoder);
	}
}
