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

namespace StudioFourteen.Interface.Controls;

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Lumina.Data.Files;

public class Texture : Image
{
	public static readonly StyledProperty<string> PathProperty;
	public static readonly StyledProperty<Rect> RectProperty;

	private static readonly Dictionary<string, CroppedBitmap> Cache = new();

	static Texture()
	{
		PathProperty = AvaloniaProperty.Register<Texture, string>(nameof(Texture.Path));
		RectProperty = AvaloniaProperty.Register<Texture, Rect>(nameof(Texture.Rect));
	}

	public string Path
	{
		get => this.GetValue(PathProperty);
		set => this.SetValue(PathProperty, value);
	}

	public Rect Rect
	{
		get => this.GetValue(RectProperty);
		set => this.SetValue(RectProperty, value);
	}

	protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
	{
		base.OnPropertyChanged(change);

		if (change.Property == PathProperty
		|| change.Property == RectProperty)
		{
			this.LoadTexture();
		}
	}

	private void LoadTexture()
	{
		try
		{
			if (string.IsNullOrEmpty(this.Path))
				return;

			string cacheKey = $"{this.Path} ({this.Rect})";

			if (Cache.TryGetValue(cacheKey, out CroppedBitmap? source))
			{
				this.Source = source;
				return;
			}

			Studio.Log.Information($"Load xiv texture {cacheKey}");
			TexFile? tex = Studio.Library.GameData.GetFile<TexFile>(this.Path);

			if (tex == null)
				return;

			Vector dpi = new Vector(96, 96);

			WriteableBitmap bitmap = new WriteableBitmap(
					new PixelSize(tex.Header.Width, tex.Header.Height),
					dpi,
					PixelFormat.Bgra8888,
					AlphaFormat.Premul);

			using (var frameBuffer = bitmap.Lock())
			{
				Marshal.Copy(tex.ImageData, 0, frameBuffer.Address, tex.ImageData.Length);
			}

			int width = (int)Math.Min(bitmap.PixelSize.Width, this.Rect.Width);
			int height = (int)Math.Min(bitmap.PixelSize.Height, this.Rect.Height);
			PixelRect rect = new((int)this.Rect.X, (int)this.Rect.Y, width, height);

			CroppedBitmap newSource = new(bitmap, rect);
			this.Source = newSource;

			Cache.Add(cacheKey, newSource);
		}
		catch (Exception ex)
		{
			Studio.Log.Error(ex, $"Error loading xiv texture {this.Path}");
		}
	}
}