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

namespace StudioFourteen.DragAndDrop;

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Effects;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;

public class DragObjectVisual : Window
{
	public DragObjectVisual(object content)
	{
		this.WindowStyle = WindowStyle.None;
		this.AllowsTransparency = true;
		this.Topmost = true;
		this.Background = new SolidColorBrush(Colors.Transparent);
		this.Opacity = 0.75;
		this.AllowDrop = false;
		this.ShowActivated = false;

		this.Resources = StudioFourteen.Resources.Load();

		DropShadowEffect shadow = new();
		shadow.BlurRadius = 10;
		shadow.Color = Colors.Black;
		shadow.Direction = 0;
		shadow.ShadowDepth = 0;

		ContentPresenter contentPresenter = new();
		contentPresenter.Content = content;
		contentPresenter.Margin = new Thickness(12);
		contentPresenter.Effect = shadow;
		this.AddChild(contentPresenter);

		this.Width = 100;
		this.Height = 100;

		this.Loaded += this.OnLoaded;
	}

	private void OnLoaded(object sender, RoutedEventArgs e)
	{
		WindowInteropHelper wndInterop = new(this);

		PInvoke.SetWindowLong(
			(HWND)wndInterop.Handle,
			WINDOW_LONG_PTR_INDEX.GWL_EXSTYLE,
			(int)WINDOW_EX_STYLE.WS_EX_TRANSPARENT | (int)WINDOW_EX_STYLE.WS_EX_LAYERED | (int)WINDOW_EX_STYLE.WS_EX_APPWINDOW | (int)WINDOW_EX_STYLE.WS_EX_TOPMOST);
	}
}