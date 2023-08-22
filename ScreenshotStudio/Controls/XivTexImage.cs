// © XivTools.
// Licensed under the MIT license.

namespace ScreenshotStudio.Controls;

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Lumina.Data.Files;
using ScreenshotStudio.Plugin;
using Serilog;

public class XivTexImage : Image
{
	protected readonly ILogger Log = Logging.ForContext<XivTexImage>();

	// dont cache across threads?
	private static readonly Dictionary<string, CroppedBitmap> Cache = new();

	public XivTexImage()
	{
		try
		{
			string cacheKey = $"{this.Path} ({this.Rect})";

			if (Cache.TryGetValue(cacheKey, out CroppedBitmap? source))
			{
				this.Source = source;
				return;
			}

			this.Log.Information($"Load xiv texture {cacheKey}");
			TexFile? tex = DalamudServices.DataManager.GetFile<TexFile>(this.Path);

			if (tex == null)
				return;

			BitmapSource bmp = BitmapSource.Create(tex.Header.Width, tex.Header.Height, 96, 96, PixelFormats.Bgra32, null, tex.ImageData, tex.Header.Width * 4);
			bmp.Freeze();

			Int32Rect rect = new((int)this.Rect.X, (int)this.Rect.Y, (int)this.Rect.Width, (int)this.Rect.Height);
			CroppedBitmap newSource = new CroppedBitmap(bmp, rect);
			this.Source = newSource;

			Cache.Add(cacheKey, newSource);
		}
		catch (Exception ex)
		{
			this.Log.Error(ex, $"Error loading xiv texture {this.Path}");
		}
	}

	public string Path { get; set; } = "ui/uld/icona_frame.tex";
	public Rect Rect { get; set; } = new Rect(0, 0, 48, 48);
}