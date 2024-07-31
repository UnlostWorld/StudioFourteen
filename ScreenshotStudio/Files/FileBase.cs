namespace ScreenshotStudio.Files;

using ScreenshotStudio.Tags;
using System;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

[Serializable]
public abstract class FileBase
{
	public string? Author { get; set; }
	public string? Description { get; set; }
	public string? Version { get; set; }
	public string? Base64Image { get; set; }
	public TagCollection? Tags { get; set; }

	public virtual void GetAutoTags(TagCollection tags)
	{
		if (this.Author != null)
		{
			tags.Add(this.Author);
		}
	}

	public ImageSource? GetImage()
	{
		if (this.Base64Image == null)
			return null;

		byte[] binaryData = Convert.FromBase64String(this.Base64Image);

		BitmapImage bi = new BitmapImage();
		bi.BeginInit();
		bi.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
		bi.StreamSource = new MemoryStream(binaryData);
		bi.EndInit();
		bi.CacheOption = BitmapCacheOption.OnDemand;
		bi.Freeze();

		return bi;
	}

	public void SetImage(byte[] binaryData)
	{
		this.Base64Image = Convert.ToBase64String(binaryData);
	}
}
