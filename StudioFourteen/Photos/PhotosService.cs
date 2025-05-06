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
using System.Threading;
using StudioFourteen.Rendering.Passes;

public partial class PhotosService : ServiceBase
{
	private readonly CapturePass renderPass = new();
	private CancellationTokenSource captureCancellation = new();

	[Notify] private bool isPhotoMode;
	[Notify] private double aspectRatio = 0;
	[Notify] private uint width = 0;
	[Notify] private uint height = 0;
	[Notify] private bool isCapturing = false;
	[Notify] private CapturePhases capturePhase;
	[Notify] private string? lastSavedImagePath;

	public delegate Task CapturePhaseChangeDelegate(CapturePhases fromPhase, CapturePhases toPhase, CancellationToken cancellationToken, bool animate);

	public event CapturePhaseChangeDelegate? PhaseChanged;

	public enum Formats
	{
		Jpeg,
		Bmp,
		Png,
		Tga,
		Tiff,
		WebP,
	}

	public enum CapturePhases
	{
		Starting,
		ChangingResolution,
		WaitingForReshade,
		Capturing,
		Saving,
		Saved,
		RestoreResolution,
		WaitingForReshadeReset,
		Done,
	}

	public FastObservableCollection<AspectRatioEntry> AspectRatios { get; init; } = new()
	{
		new("Native", "Monitor", 0, 0, 0),
		new("Native", "Instagram (1:1)", 1.0, 0, 0),
		new("Native", "Surface (3:2)", 3.0 / 2.0, 0, 0),
		new("Native", "Widescreen (16:9)", 16.0 / 9.0, 0, 0),
		new("Native", "Widescreen (16:10)", 16.0 / 10.0, 0, 0),
		new("Native", "Cinematic (21:9)", 21.0 / 9.0, 0, 0),
		new("Native", "Ultrawide (21:9~)", 43 / 18.0, 0, 0),
		new("Native", "Super-Ultrawide (32:9)", 32.0 / 9.0, 0, 0),

		new("Surface (3:2)", "Full HD Plus", 3.0 / 2.0, 1920, 1280),
		new("Surface (3:2)", "Surface Pro 3", 3.0 / 2.0, 2160, 1440),
		new("Surface (3:2)", "Surface Pro 4", 3.0 / 2.0, 2736, 1824),
		new("Surface (3:2)", "Surface Pro X", 3.0 / 2.0, 2880, 1920),
		new("Surface (3:2)", "Surface Studio", 3.0 / 2.0, 4500, 3000),

		new("Widescreen (16:9)", "720p", 16.0 / 9.0, 1280, 720),
		new("Widescreen (16:9)", "1080p", 16.0 / 9.0, 1920, 1080),
		new("Widescreen (16:9)", "1440p", 16.0 / 9.0, 2560, 1440),
		new("Widescreen (16:9)", "2160p", 16.0 / 9.0, 3840, 2160),
		new("Widescreen (16:9)", "2880p", 16.0 / 9.0, 5120, 2880),
		new("Widescreen (16:9)", "4320p", 16.0 / 9.0, 7680, 4320),

		new("Widescreen (16:10)", "Wide SXGA", 8.0 / 5.0, 1440, 900),
		new("Widescreen (16:10)", "15\" Retina", 8.0 / 5.0, 2880, 1800),
		new("Widescreen (16:10)", "16\" Retina", 8.0 / 5.0, 3072, 1920),

		new("Cinematic (21:9)", "UltraWide FHD", 21.0 / 9.0, 2560, 1080),
		new("Cinematic (21:9)", "Ultra-Wide 4K", 21.0 / 9.0, 3840, 1600),
		new("Cinematic (21:9)", "Ultra-Wide 5K", 21.0 / 9.0, 5120, 2160),

		new("Ultrawide (21:9~)", "1440p", 3440.0 / 1440.0, 3440, 1440),
		new("Ultrawide (21:9~)", "2160p", 3440.0 / 1440.0, 5160, 2160),
		new("Ultrawide (21:9~)", "2880p", 3440.0 / 1440.0, 6880, 2880),
		new("Ultrawide (21:9~)", "4320p", 3440.0 / 1440.0, 10320, 4320),

		new("Super-Ultrawide (32:9)", "1080p", 32.0 / 9, 3840, 1080),
		new("Super-Ultrawide (32:9)", "1440p", 32.0 / 9, 5120, 1440),
		new("Super-Ultrawide (32:9)", "2160p", 32.0 / 9, 7680, 2160),
	};

	public int GuideThickness => 2;

	public void Capture(string? name = null, bool animate = true)
	{
		this.CaptureAsync(name, animate).Run();
	}

	public string GetDefaultFileName() => DateTime.Now.ToString("yyyy-MM-dd HH-mm");

	public async Task CaptureAsync(string? name = null, bool animate = true)
	{
		if (this.Settings.PhotoDirectory == null)
			return;

		if (this.IsCapturing)
		{
			// If we're already waiting on a queued capture, don't queue another one.
			if (this.captureCancellation.IsCancellationRequested)
				return;

			this.captureCancellation.Cancel();

			while (this.IsCapturing)
			{
				await Task.Delay(10);
			}
		}

		this.IsCapturing = true;
		this.captureCancellation = new();

		await this.DispatchCapturePhaseChange(CapturePhases.Starting, animate);

		bool customResolution = this.width > 0 && this.height > 0;
		uint originalWidth = 0;
		uint originalHeight = 0;
		bool success = false;

		if (customResolution)
		{
			await this.DispatchCapturePhaseChange(CapturePhases.ChangingResolution, animate);
			await TickService.GameTick();

			success = this.SetResolution(this.width, this.height, out originalWidth, out originalHeight);
			if (!success)
			{
				this.IsCapturing = false;
				return;
			}

			await this.DispatchCapturePhaseChange(CapturePhases.WaitingForReshade, animate);
			success = await this.Services.Reshade.WaitForEffectsToLoad();
			if (!success)
			{
				await TickService.GameTick();
				this.SetResolution(originalWidth, originalHeight, out _, out _);
				this.IsCapturing = false;
				return;
			}
		}

		await Threads.NonUiThread();

		// Capture the screenshot
		try
		{
			await this.DispatchCapturePhaseChange(CapturePhases.Capturing, animate);

			this.Services.Rendering.AddAfterEffectsPass(this.renderPass);
			this.renderPass.DoCapture();

			while(this.renderPass.Capture == null)
				await Task.Delay(10);

			Image? backBuffer = this.renderPass.Capture;
			Image? depthBuffer = this.renderPass.DepthCapture;

			this.Services.Rendering.RemoveAfterEffectsPass(this.renderPass);

			// Add Metadata
			if (backBuffer != null && this.Settings.PhotoIncludeMetaData)
			{
				ImageMetadata metadata = new();

				string metaDataJson = Serializer.Serialize(metadata);
				backBuffer.Metadata.ExifProfile = new();
				backBuffer.Metadata.ExifProfile.SetValue(ExifTag.UserComment, metaDataJson);
			}

			await this.DispatchCapturePhaseChange(CapturePhases.Saving, animate);

			// Save the screenshot
			if (!Directory.Exists(this.Settings.PhotoDirectory))
				Directory.CreateDirectory(this.Settings.PhotoDirectory);

			if (name == null)
				name = this.GetDefaultFileName();

			// Custom formatting to avoid culture formats producing invalid file names.
			string fileName;
			string extension = this.Settings.PhotoFormat switch
			{
				Formats.Jpeg => "jpg",
				Formats.Bmp => "bmp",
				Formats.Png => "png",
				Formats.Tga => "tga",
				Formats.Tiff => "tiff",
				Formats.WebP => "webp",
				_ => throw new NotSupportedException(),
			};

			int count = 0;
			do
			{
				fileName = $"{this.Settings.PhotoDirectory}/{name}.{extension}";

				if (count > 0)
					fileName = $"{this.Settings.PhotoDirectory}/{name} ({count}).{extension}";

				count++;
			}
			while (File.Exists(fileName));

			string? dirName = Path.GetDirectoryName(fileName);
			if (dirName != null && !Directory.Exists(dirName))
				Directory.CreateDirectory(dirName);

			switch (this.Settings.PhotoFormat)
			{
				case Formats.Jpeg:
				{
					JpegEncoder encoder = new()
					{
						Quality = 95,
						Interleaved = false,
					};

					await backBuffer.SaveAsJpegAsync(fileName, encoder);
					break;
				}

				case Formats.Bmp:
				{
					await backBuffer.SaveAsBmpAsync(fileName);
					break;
				}

				case Formats.Png:
				{
					await backBuffer.SaveAsPngAsync(fileName);
					break;
				}

				case Formats.Tga:
				{
					await backBuffer.SaveAsTgaAsync(fileName);
					break;
				}

				case Formats.Tiff:
				{
					await backBuffer.SaveAsTiffAsync(fileName);
					break;
				}

				case Formats.WebP:
				{
					await backBuffer.SaveAsWebpAsync(fileName);
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

			this.LastSavedImagePath = fileName;
			await this.DispatchCapturePhaseChange(CapturePhases.Saved, animate);
		}
		catch (Exception ex)
		{
			this.Log.Error(ex, "Error in photo capture");
		}

		// Restore the resolution
		if (customResolution)
		{
			await this.DispatchCapturePhaseChange(CapturePhases.RestoreResolution, animate);

			// Give a small delay for animations to catch up.
			await Task.Delay(150);

			await TickService.GameTick();
			this.SetResolution(originalWidth, originalHeight, out _, out _);

			await this.DispatchCapturePhaseChange(CapturePhases.WaitingForReshadeReset, animate);
			await this.Services.Reshade.WaitForEffectsToLoad();
		}

		await this.DispatchCapturePhaseChange(CapturePhases.Done, animate);
		this.IsCapturing = false;
	}

	private async Task DispatchCapturePhaseChange(CapturePhases newPhase, bool animate)
	{
		if (this.PhaseChanged == null)
			return;

		try
		{
			await this.PhaseChanged(this.CapturePhase, newPhase, this.captureCancellation.Token, animate);
		}
		catch (TaskCanceledException)
		{
		}

		this.CapturePhase = newPhase;
	}

	private unsafe void OnIsPhotoModeChanged(bool oldValue, bool newValue)
	{
		RaptureAtkModule.Instance()->IsUiVisible = newValue;
	}

	private unsafe bool SetResolution(uint width, uint height, out uint oldWidth, out uint oldHeight)
	{
		TickService.VerifyGameTickThread();

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

	public class ImageMetadata : AutoViewModel
	{
		[AutoNotify] public uint MapId { get; set; } = 0;
	}
}

public class AspectRatioEntry(string groupName, string name, double aspect, uint width, uint height)
{
	public string? Name { get; set; } = name;
	public string? GroupName { get; set; } = groupName;
	public double Aspect { get; set; } = aspect;
	public uint Width { get; set; } = width;
	public uint Height { get; set; } = height;
}