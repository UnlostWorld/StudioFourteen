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
using DependencyPropertyGenerator;
using SVGImage.SVG;
using System.Windows;
using System.Collections.Generic;
using System;

[DependencyProperty<Brush>("Midground")]
public partial class SvgIconView : SVGImage
{
	static SvgIconView()
	{
		ForegroundProperty.OverrideMetadata(typeof(SvgIconView), new FrameworkPropertyMetadata(OnForegroundChanged));
		UriSourceProperty.OverrideMetadata(typeof(SvgIconView), new FrameworkPropertyMetadata(OnSourceChanged));
	}

	public SvgIconView()
	{
		this.UpdateBrushes();
	}

	protected override void OnInitialized(EventArgs e)
	{
		base.OnInitialized(e);
		this.UpdateBrushes();
	}

	private static void OnForegroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is SvgIconView iconView)
		{
			iconView.UpdateBrushes();
		}
	}

	private static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is SvgIconView iconView)
		{
			iconView.UpdateBrushes();
		}
	}

	partial void OnMidgroundChanged(Brush? newValue)
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

		if (this.Midground != null)
			customBrushes["background"] = this.Midground;

		customBrushes["foreground"] = this.Foreground;
		customBrushes["black"] = this.Foreground;
		this.CustomBrushes = customBrushes;
	}
}