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

public partial class PhotosService : ServiceBase
{
	[Notify] private bool isPhotoMode;
	[Notify] private Guides guide;
	[Notify] private double aspectRatio = 0;
	[Notify] private bool isPortrait;

	public enum Guides
	{
		None,
		Thirds,
		Circle,
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
		/*Image? image = this.Capture.ToImage();
		if (image == null)
			return;

		string metaDataJson = Serializer.Serialize(this.MetaData);
		image.Metadata.ExifProfile = new();
		image.Metadata.ExifProfile.SetValue(ExifTag.UserComment, metaDataJson);

		JpegEncoder encoder = new()
		{
			Quality = 95,
			Interleaved = false,
		};

		image.SaveAsJpeg($"{this.SaveDirectory}/test.jpg", encoder);*/
	}

	private unsafe void OnIsPhotoModeChanged(bool oldValue, bool newValue)
	{
		RaptureAtkModule.Instance()->IsUiVisible = newValue;
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
