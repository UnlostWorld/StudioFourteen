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
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using StudioFourteen.Services.Library.GameData;

#pragma warning disable

public class TextureIcon : Image
{
	public static readonly StyledProperty<uint> IdProperty;
	public static readonly StyledProperty<uint> OffsetProperty;

	private TextureIconReference? reference;

	static TextureIcon()
	{
		IdProperty = AvaloniaProperty.Register<TextureIcon, uint>(nameof(TextureIcon.Id));
		OffsetProperty = AvaloniaProperty.Register<TextureIcon, uint>(nameof(TextureIcon.Offset));
	}

	public uint Id
	{
		get => this.GetValue(IdProperty);
		set => this.SetValue(IdProperty, value);
	}

	public uint Offset
	{
		get => this.GetValue(OffsetProperty);
		set => this.SetValue(OffsetProperty, value);
	}

	protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
	{
		base.OnPropertyChanged(change);

		if (change.Property == IdProperty || change.Property == OffsetProperty)
		{
			this.LoadTexture();
		}
	}

	private void LoadTexture()
	{
		if (this.Id + this.Offset == 0)
			return;

		try
		{
			this.reference = new(this.Id + this.Offset);
			Bitmap? src = this.reference.Source;
			this.Source = src;

			if (this.Width == double.NaN && this.Height == double.NaN)
			{
				this.Width = src.PixelSize.Width;
				this.Height = src.PixelSize.Height;
			}
		}
		catch (Exception ex)
		{
			Studio.Log.Error(ex, "Error loading texture icon");
		}
	}
}