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

using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Effects;
using Windows.Win32;
using Windows.Win32.Foundation;

public class DragObjectVisual : Window
{
	public DragObjectVisual(object content)
	{
		this.WindowStyle = WindowStyle.None;
		this.AllowsTransparency = true;
		this.Topmost = true;
		this.IsHitTestVisible = false;
		this.Background = new SolidColorBrush(Colors.Transparent);

		this.Resources = StudioFourteen.Resources.Load();

		WindowInteropHelper wndInterop = new(this);
		PInvoke.EnableWindow((HWND)wndInterop.Handle, false);

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
	}
}