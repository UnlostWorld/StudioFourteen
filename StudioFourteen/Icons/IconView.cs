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
using System.Windows.Media;
using System.Windows.Controls;
using DependencyPropertyGenerator;
using SVGImage.SVG;
using System.Windows;
using System.Collections.Generic;
using FontAwesome.Sharp;

[DependencyProperty<IconDefinitionBase>("Icon")]
[DependencyProperty<Brush>("Foreground")]
public partial class IconView : ContentPresenter
{
	partial void OnIconChanged(IconDefinitionBase? newValue)
	{
		this.Content = newValue;
	}
}

[DependencyProperty<IconChar>("Icon")]
[DependencyProperty<IconFont>("IconFont")]
public partial class FaIconView : Control
{
}

public partial class SvgIconView : SVGImage
{
	static SvgIconView()
	{
		ForegroundProperty.OverrideMetadata(typeof(SvgIconView), new FrameworkPropertyMetadata(OnForegroundChanged));
	}

	public SvgIconView()
	{
		this.Loaded += this.OnLoaded;
	}

	private static void OnForegroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is SvgIconView iconView)
		{
			iconView.UpdateBrushes();
		}
	}

	private void OnLoaded(object sender, RoutedEventArgs e)
	{
		this.UpdateBrushes();
	}

	private void UpdateBrushes()
	{
		Dictionary<string, Brush>? oldcustomBrushes = this.CustomBrushes;
		if (oldcustomBrushes == null)
			oldcustomBrushes = new();

		Dictionary<string, Brush> customBrushes = new();
		foreach((string key, Brush brush) in oldcustomBrushes)
		{
			customBrushes.Add(key, brush);
		}

		customBrushes["foreground"] = this.Foreground;
		customBrushes["black"] = this.Foreground;
		this.CustomBrushes = customBrushes;
	}
}