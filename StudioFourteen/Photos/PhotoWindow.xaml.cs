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

using PropertyChanged.SourceGenerator;
using StudioFourteen.Mvm;
using StudioFourteen.Panels;
using StudioFourteen.Plugin;
using System;
using System.Windows;

public partial class PhotoWindow : Panel
{
	[AutoNotify] public ImageMetadata MetaData { get; set; } = new();
	[AutoNotify] public string SaveDirectory { get; set; } = string.Empty;

	public bool HideUI
	{
		get => this.Persistence.GetPersistence<bool>();
		set
		{
			this.Persistence.SetPersistence(value);
			this.Services.Photos.IsPhotoMode = this.HideUI;
		}
	}

	protected override void OnOpened()
	{
		base.OnOpened();

		if (DalamudServices.ClientState == null)
			return;

		AutoPropertyNotifyService.Register(this.MetaData);

		this.MetaData.MapId = DalamudServices.ClientState.MapId;

		this.SaveDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

		this.Services.Photos.IsPhotoMode = this.HideUI;
	}

	protected override void OnClosed()
	{
		base.OnClosed();
		this.Services.Photos.IsPhotoMode = false;
	}

	private void OnSaveClicked(object sender, RoutedEventArgs e)
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

	public class ImageMetadata : AutoViewModel
	{
		[AutoNotify] public uint MapId { get; set; } = 0;
	}
}
