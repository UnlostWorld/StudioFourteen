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

namespace StudioFourteen.Photos;

using StudioFourteen.Services;
using PropertyChanged.SourceGenerator;
using FFXIVClientStructs.FFXIV.Client.UI;
using WpfUtils.Extensions;
using StudioFourteen.Mvm;
using SixLabors.ImageSharp;
using System.Threading.Tasks;
using StudioFourteen.Serialization;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using System;
using System.IO;
using FFXIVClientStructs.FFXIV.Client.Graphics.Kernel;
using StudioFourteen.Utilities;

public partial class PhotosService : ServiceBase
{
	[Notify] private bool isPhotoMode;
	[Notify] private Guides guide;
	[Notify] private double aspectRatio = 0;
	[Notify] private bool isPortrait;
	[Notify] private bool showDepth = false;
	[Notify] private bool isCapturing = false;

	public enum Guides
	{
		None,
		Thirds,
		Circle,
	}

	public enum Formats
	{
		Jpeg,
		Bmp,
		Png,
		Tga,
		Tiff,
		WebP,
	}

	public FastObservableCollection<AspectRatioEntry> AspectRatios { get; init; } = new()
	{
		new("Monitor", 0),

		new("Square, Instagram (1:1)", 1),
		new("(4:3)", 4.0 / 3.0),
		new("Widescreen (16:9)", 16.0 / 9.0),
		new("(16:10)", 16.0 / 10.0),
		new("Cinematic (21:9)", 21.0 / 9.0),
		new("Ultrawide (21:9~)", 3440.0 / 1440.0), // Not actually 21:9! thanks monitor industry!
		new("Super-Ultrawide (32:9)", 32.0 / 9.0),
	};

	public int GuideThickness => 2;

	public void Capture()
	{
		this.CaptureAsync().Run();
	}

	public async Task CaptureAsync()
	{
		if (this.Settings.PhotoDirectory == null)
			return;

		this.IsCapturing = true;

		bool superResolution = false;
		uint originalWidth = 0;
		uint originalHeight = 0;
		bool success = false;

		if (superResolution)
		{
			await Threads.FrameworkThread();

			// 4096 x 2160
			// 8192 x 4320
			success = this.SetResolution(8192, 4320, out originalWidth, out originalHeight);
			if (!success)
			{
				this.IsCapturing = false;
				return;
			}

			success = await this.Services.Reshade.WaitForEffectsToLoad();
			if (!success)
			{
				await Threads.FrameworkThread();
				this.SetResolution(originalWidth, originalHeight, out _, out _);
				this.IsCapturing = false;
				return;
			}
		}

		await Threads.NonUiThread();

		// Capture the screenshot
		try
		{
			(Image? backBuffer, Image? depthBuffer) = await this.Services.GameCapture.ToImage();

			// Add Metadata
			if (backBuffer != null && this.Settings.PhotoIncludeMetaData)
			{
				ImageMetadata metadata = new();

				string metaDataJson = Serializer.Serialize(metadata);
				backBuffer.Metadata.ExifProfile = new();
				backBuffer.Metadata.ExifProfile.SetValue(ExifTag.UserComment, metaDataJson);
			}

			// Save the screenshot
			if (!Directory.Exists(this.Settings.PhotoDirectory))
				Directory.CreateDirectory(this.Settings.PhotoDirectory);

			// Custom formatting to avoid culture formats producing invalid file names.
			string fileName = $"{this.Settings.PhotoDirectory}/{DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss")}";

			switch (this.Settings.PhotoFormat)
			{
				case Formats.Jpeg:
				{
					JpegEncoder encoder = new()
					{
						Quality = 95,
						Interleaved = false,
					};

					await backBuffer.SaveAsJpegAsync($"{fileName}.jpg", encoder);
					break;
				}

				case Formats.Bmp:
				{
					await backBuffer.SaveAsBmpAsync($"{fileName}.bmp");
					break;
				}

				case Formats.Png:
				{
					await backBuffer.SaveAsPngAsync($"{fileName}.png");
					break;
				}

				case Formats.Tga:
				{
					await backBuffer.SaveAsTgaAsync($"{fileName}.tga");
					break;
				}

				case Formats.Tiff:
				{
					await backBuffer.SaveAsTiffAsync($"{fileName}.tiff");
					break;
				}

				case Formats.WebP:
				{
					await backBuffer.SaveAsWebpAsync($"{fileName}.webp");
					break;
				}
			}

			if (depthBuffer != null)
			{
				PngEncoder encoder = new()
				{
					ColorType = PngColorType.Grayscale,
					BitDepth = PngBitDepth.Bit16,
				};

				await depthBuffer.SaveAsPngAsync($"{fileName} depth.png", encoder);
			}
		}
		catch(Exception ex)
		{
			this.Log.Error(ex, "Error in photo capture");
		}

		// Restore the resolution
		if (superResolution)
		{
			await Threads.FrameworkThread();
			this.SetResolution(originalWidth, originalHeight, out _, out _);

			await this.Services.Reshade.WaitForEffectsToLoad();
		}

		this.IsCapturing = false;
	}

	private unsafe void OnIsPhotoModeChanged(bool oldValue, bool newValue)
	{
		RaptureAtkModule.Instance()->IsUiVisible = newValue;
	}

	private unsafe bool SetResolution(uint width, uint height, out uint oldWidth, out uint oldHeight)
	{
		Threads.VerifyFrameworkThread();

		oldWidth = 0;
		oldHeight = 0;

		var kernelDev = Device.Instance();
		if (kernelDev == null)
			return false;

		oldWidth = kernelDev->Width;
		oldHeight = kernelDev->Height;

		kernelDev->NewWidth = width;
		kernelDev->NewHeight = height;
		kernelDev->RequestResolutionChange = 1;

		return true;
	}

	public class AspectRatioEntry(string name, double aspect)
	{
		public string? Name { get; set; } = name;
		public double Aspect { get; set; } = aspect;
	}

	public class ImageMetadata : AutoViewModel
	{
		[AutoNotify] public uint MapId { get; set; } = 0;
	}
}
