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

namespace StudioFourteen.Icons;

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using DependencyPropertyGenerator;
using FontAwesome.Sharp;

[DependencyProperty<IconChar>("Icon", DefaultValue = IconChar.Star)]
[DependencyProperty<IconFont>("IconFont", DefaultValue = IconFont.Auto)]
public partial class FaIconView : TextBlock
{
	private static readonly FontFamily RegularFont = typeof(IconHelper).Assembly.LoadFont("fonts", "Font Awesome 6 Free Regular");
	private static readonly FontFamily SolidFont = typeof(IconHelper).Assembly.LoadFont("fonts", "Font Awesome 6 Free Solid");
	private static readonly FontFamily BrandsFont = typeof(IconHelper).Assembly.LoadFont("fonts", "Font Awesome 6 Brands Regular");

	public FaIconView()
	{
		this.Loaded += this.OnLoaded;
	}

	private void OnLoaded(object sender, RoutedEventArgs e)
	{
		this.Loaded -= this.OnLoaded;

		this.OnIconChanged(this.Icon);
		this.OnIconFontChanged(this.IconFont);
	}

	partial void OnIconChanged(IconChar newValue)
	{
		IConvertible cvt = newValue;
		this.Text = char.ConvertFromUtf32(cvt.ToInt32(CultureInfo.InvariantCulture));
	}

	partial void OnIconFontChanged(IconFont newValue)
	{
		this.FontFamily = this.GetFont(newValue);
	}

	private FontFamily GetFont(IconFont font)
	{
		switch (font)
		{
			case IconFont.Auto:
			case IconFont.Regular: return RegularFont;
			case IconFont.Solid: return SolidFont;
			case IconFont.Brands: return BrandsFont;
		}

		throw new Exception("Invalid font");
	}

	/*protected override FontFamily FontFor(IconChar icon)
	{
		FontFamily? font = base.FontFor(icon);
		if (font == null)
			return base.FontFor(IconChar.Exclamation);

		return font;
	}*/
}
