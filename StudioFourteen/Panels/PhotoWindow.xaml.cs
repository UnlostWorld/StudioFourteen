namespace StudioFourteen.Studio;

using StudioFourteen.Plugin;
using StudioFourteen.Serialization;
using StudioFourteen.Panels;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;
using System;
using System.Windows;
using StudioFourteen.Mvm;

public partial class PhotoWindow : Panel
{
	[AutoNotify] public ImageMetadata MetaData { get; set; } = new();
	[AutoNotify] public string SaveDirectory { get; set; } = string.Empty;

	protected override void OnOpened()
	{
		base.OnOpened();

		if (DalamudServices.ClientState == null)
			return;

		AutoPropertyNotifyService.Register(this.MetaData);

		this.MetaData.MapId = DalamudServices.ClientState.MapId;

		this.SaveDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
	}

	private void OnSaveClicked(object sender, RoutedEventArgs e)
	{
		Image? image = this.Capture.ToImage();
		if (image == null)
			return;

		this.Flash.BeginStoryboard("Flash");

		string metaDataJson = Serializer.Serialize(this.MetaData);
		image.Metadata.ExifProfile = new();
		image.Metadata.ExifProfile.SetValue(ExifTag.UserComment, metaDataJson);

		JpegEncoder encoder = new()
		{
			Quality = 95,
			Interleaved = false,
		};

		image.SaveAsJpeg($"{this.SaveDirectory}/test.jpg", encoder);
	}

	public class ImageMetadata : AutoViewModel
	{
		[AutoNotify] public uint MapId { get; set; } = 0;
	}
}
