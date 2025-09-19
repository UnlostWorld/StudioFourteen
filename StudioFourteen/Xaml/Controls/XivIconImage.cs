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

namespace StudioFourteen.Xaml;

using StudioFourteen.GameData;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

public class XivIconImage : Image
{
	public static readonly DependencyProperty IdProperty = DependencyProperty.Register(
		nameof(XivIconImage.Id),
		typeof(uint),
		typeof(XivIconImage),
		new(0U, OnIdChanged));

	private ImageReference? imageReference;

	public uint Id
	{
		get => (uint)this.GetValue(IdProperty);
		set => this.SetValue(IdProperty, value);
	}

	private static void OnIdChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is XivIconImage image)
		{
			image.UpdateSource();
		}
	}

	private void UpdateSource()
	{
		if (DesignerProperties.GetIsInDesignMode(this))
			return;

		this.imageReference = new(this.Id);
		this.Source = this.imageReference.Source;
	}
}
