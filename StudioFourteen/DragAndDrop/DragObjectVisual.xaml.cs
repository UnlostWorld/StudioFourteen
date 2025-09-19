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
using System.Windows.Interop;
using global::Windows.Win32;
using global::Windows.Win32.Foundation;
using global::Windows.Win32.UI.WindowsAndMessaging;
using StudioFourteen.Xaml;

public partial class DragObjectVisual : Window
{
	private readonly object content;

	public DragObjectVisual(object content)
	{
		this.content = content;

		this.InitializeComponent();

		this.Resources = XamlResources.Load();
		this.Loaded += this.OnLoaded;
	}

	public void SetOperation(object? op)
	{
		this.Dispatcher.Invoke(() =>
		{
			this.OperationPresenterFg.Icon = op;
			this.OperationPresenterBg.Icon = op;
		});
	}

	private void OnLoaded(object sender, RoutedEventArgs e)
	{
		this.ContentPresenter.Content = this.content;

		WindowInteropHelper wndInterop = new(this);

		PInvoke.SetWindowLong(
			(HWND)wndInterop.Handle,
			WINDOW_LONG_PTR_INDEX.GWL_EXSTYLE,
			(int)WINDOW_EX_STYLE.WS_EX_TRANSPARENT | (int)WINDOW_EX_STYLE.WS_EX_LAYERED | (int)WINDOW_EX_STYLE.WS_EX_APPWINDOW | (int)WINDOW_EX_STYLE.WS_EX_TOPMOST);
	}
}