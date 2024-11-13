namespace StudioFourteen.Controls;

using Lumina.Data.Files;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

public class XivTexImage : Image
{
	public static readonly DependencyProperty PathProperty = DependencyProperty.Register(
		nameof(XivTexImage.Path),
		typeof(string),
		typeof(XivTexImage),
		new(string.Empty, OnPathChanged));

	public static readonly DependencyProperty RectProperty = DependencyProperty.Register(
		nameof(XivTexImage.Rect),
		typeof(Rect),
		typeof(XivTexImage),
		new(Rect.Empty, OnRectChanged));

	protected readonly ILogger Log = Logging.ForContext<XivTexImage>();
	private static readonly Dictionary<string, CroppedBitmap> Cache = new();

	public XivTexImage()
	{
		this.UpdateSource();
	}

	public string Path
	{
		get => (string)this.GetValue(PathProperty);
		set => this.SetValue(PathProperty, value);
	}

	public Rect Rect
	{
		get => (Rect)this.GetValue(RectProperty);
		set => this.SetValue(RectProperty, value);
	}

	private static void OnPathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is XivTexImage image)
		{
			image.UpdateSource();
		}
	}

	private static void OnRectChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is XivTexImage image)
		{
			image.UpdateSource();
		}
	}

	private void UpdateSource()
	{
		if (DesignerProperties.GetIsInDesignMode(this))
			return;

		try
		{
			if (string.IsNullOrEmpty(this.Path))
				return;

			if (this.Rect == Rect.Empty)
				return;

			string cacheKey = $"{this.Path} ({this.Rect})";

			if (Cache.TryGetValue(cacheKey, out CroppedBitmap? source))
			{
				this.Source = source;
				return;
			}

			this.Log.Information($"Load xiv texture {cacheKey}");
			TexFile? tex = ServiceManager.Instance.GameData.GetFile<TexFile>(this.Path);

			if (tex == null)
				return;

			BitmapSource bmp = BitmapSource.Create(tex.Header.Width, tex.Header.Height, 96, 96, PixelFormats.Bgra32, null, tex.ImageData, tex.Header.Width * 4);
			bmp.Freeze();

			Int32Rect rect = new((int)this.Rect.X, (int)this.Rect.Y, (int)this.Rect.Width, (int)this.Rect.Height);
			rect.Width = (int)Math.Min(bmp.Width, rect.Width);
			rect.Height = (int)Math.Min(bmp.Height, rect.Height);

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
}