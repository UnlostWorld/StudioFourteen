// © XivTools.
// Licensed under the MIT license.

namespace ScreenshotStudio.Controls;

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Dalamud.Interface.Internal;
using Dalamud.Plugin.Services;
using Lumina.Data.Files;
using ScreenshotStudio.Plugin;
using Serilog;

public class XivTexImage : Image
{
	protected readonly ILogger Log = Logging.ForContext<XivTexImage>();
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

			////string path = DalamudServices.TextureSubstitutionProvider.GetSubstitutedPath(this.Path);

			this.Log.Information($"Load xiv texture {cacheKey}");
			TexFile? tex = DalamudServices.DataManager.GetFile<TexFile>(this.Path);

			if (tex == null)
				return;

			BitmapSource bmp = BitmapSource.Create(tex.Header.Width, tex.Header.Height, 96, 96, PixelFormats.Bgra32, null, tex.ImageData, tex.Header.Width * 4);
			bmp.Freeze();

			Int32Rect rect = new((int)this.Rect.X, (int)this.Rect.Y, (int)this.Rect.Width, (int)this.Rect.Height);
			CroppedBitmap newSource = new CroppedBitmap(bmp, rect);
			newSource.Freeze();
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