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
using System.IO;
using global::Avalonia.Controls.Converters;
using global::Avalonia;
using global::Avalonia.Controls;
using global::Avalonia.Media;
using global::Avalonia.Svg;
using global::Svg.Model;
using ShimSkiaSharp;
using StudioFourteen.Services.Content;
using StudioFourteen.Services.Tick;
using Avalonia.Media.Immutable;

// https://github.com/wieslawsoltes/Svg.Skia/blob/master/src/Svg.Controls.Avalonia/Svg.cs
public class Svg : Control
{
	public static readonly StyledProperty<string?> SourceProperty;
	public static readonly StyledProperty<Stretch> StretchProperty;
	public static readonly StyledProperty<StretchDirection> StretchDirectionProperty;
	public static readonly StyledProperty<IBrush> ForegroundProperty;

	private SvgReference? reference;
	private SKPicture? picture;
	private AvaloniaPicture? avaloniaPicture;

	static Svg()
	{
		SourceProperty = AvaloniaProperty.Register<Svg, string?>(nameof(Source));
		StretchProperty = AvaloniaProperty.Register<Svg, Stretch>(nameof(Stretch), Stretch.Uniform);
		StretchDirectionProperty = AvaloniaProperty.Register<Svg, StretchDirection>(nameof(StretchDirection), StretchDirection.Both);
		ForegroundProperty = AvaloniaProperty.Register<Svg, IBrush>(nameof(Foreground), new SolidColorBrush(Colors.Black));

		AffectsRender<Svg>(SourceProperty, StretchProperty, StretchDirectionProperty);
		AffectsMeasure<Svg>(SourceProperty, StretchProperty, StretchDirectionProperty);
	}

	public string? Source
	{
		get => this.GetValue(SourceProperty);
		set => this.SetValue(SourceProperty, value);
	}

	public Stretch Stretch
	{
		get => this.GetValue(StretchProperty);
		set => this.SetValue(StretchProperty, value);
	}

	public StretchDirection StretchDirection
	{
		get => this.GetValue(StretchDirectionProperty);
		set => this.SetValue(StretchDirectionProperty, value);
	}

	public IBrush Foreground
	{
		get => this.GetValue(ForegroundProperty);
		set => this.SetValue(ForegroundProperty, value);
	}

	public SKPicture? Model => this.reference?.Get();

	public override void Render(DrawingContext context)
	{
		if (this.picture == null)
			return;

		var viewPort = new Rect(this.Bounds.Size);
		var sourceSize = new Size(this.picture.CullRect.Width, this.picture.CullRect.Height);
		if (sourceSize.Width <= 0 || sourceSize.Height <= 0)
			return;

		Vector scale = this.Stretch.CalculateScaling(this.Bounds.Size, sourceSize, this.StretchDirection);
		Size scaledSize = sourceSize * scale;
		Rect destRect = viewPort
			.CenterRect(new Rect(scaledSize))
			.Intersect(viewPort);
		Rect sourceRect = new Rect(sourceSize)
			.CenterRect(new Rect(destRect.Size / scale));

		SKRect bounds = this.picture.CullRect;
		Matrix scaleMatrix = Matrix.CreateScale(
			destRect.Width / sourceRect.Width,
			destRect.Height / sourceRect.Height);
		Matrix translateMatrix = Matrix.CreateTranslation(
			-sourceRect.X + destRect.X - bounds.Top,
			-sourceRect.Y + destRect.Y - bounds.Left);

		using (context.PushClip(destRect))
		using (context.PushTransform(scaleMatrix * translateMatrix))
		{
			if (this.avaloniaPicture is { })
			{
				this.avaloniaPicture.Draw(context);
			}
		}
	}

	protected override Size MeasureOverride(Size availableSize)
	{
		if (this.picture == null)
			return default;

		Size sourceSize = this.picture is { }
			? new Size(this.picture.CullRect.Width, this.picture.CullRect.Height)
			: default;

		return this.Stretch.CalculateSize(availableSize, sourceSize, this.StretchDirection);
	}

	protected override Size ArrangeOverride(Size finalSize)
	{
		if (this.picture == null)
			return default;

		Size sourceSize = this.picture is { }
			? new Size(this.picture.CullRect.Width, this.picture.CullRect.Height)
			: default;

		return Stretch.Uniform.CalculateSize(finalSize, sourceSize);
	}

	protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
	{
		if (change.Property == SourceProperty)
			this.OnSourceChanged(this.Source);

		if (change.Property == ForegroundProperty)
			this.OnForegroundChanged(this.Foreground);

		base.OnPropertyChanged(change);
	}

	private void OnSourceChanged(string? newValue)
	{
		try
		{
			this.reference?.Reloaded -= this.OnSvgReloaded;
			this.reference?.Dispose();

			if (string.IsNullOrEmpty(newValue))
				return;

			this.reference = new(newValue);
			this.reference.Reloaded += this.OnSvgReloaded;

			this.picture = this.reference.Get();
			this.avaloniaPicture = AvaloniaPicture.Record(this.picture);
			this.InvalidateVisual();
		}
		catch (Exception ex)
		{
			Studio.Log.Error(ex, "Error loading SVG");
		}
	}

	private void OnForegroundChanged(IBrush newValue)
	{
		if (this.reference == null)
			return;

		if (newValue is SolidColorBrush colorBrush)
		{
			this.reference.Foreground = colorBrush.Color;
		}
		else if (newValue is ImmutableSolidColorBrush immutableColorBrush)
		{
			this.reference.Foreground = immutableColorBrush.Color;
		}
		else
		{
			throw new NotSupportedException("Only SolidColorBrush is supported for SVG foreground property");
		}

		this.reference.Reload();
	}

	private void OnSvgReloaded()
	{
		Studio.Tick.Dispatch(TickChannels.Ui, () =>
		{
			if (this.reference == null)
				return;

			this.picture = this.reference.Get();
			this.avaloniaPicture = AvaloniaPicture.Record(this.picture);
			this.InvalidateVisual();
		});
	}

	public class SvgReference : ContentReference<SKPicture>
	{
		public Color Foreground = Colors.White;
		public Color Background = Colors.Black;

		public SvgReference(string path)
			: base(path)
		{
		}

		protected override SKPicture Load(Stream stream)
		{
			string foregroundColor = ColorToHexConverter.ToHexString(this.Foreground, AlphaComponentPosition.Trailing, false, true);
			string css = $".foreground {{ stroke: {foregroundColor}; }}";
			SvgParameters parameters = new(null, css);

			SKPicture? picture = SvgSource.LoadPicture(stream, parameters);
			if (picture == null)
				throw new Exception("Failed to load svg");

			return picture;
		}
	}
}