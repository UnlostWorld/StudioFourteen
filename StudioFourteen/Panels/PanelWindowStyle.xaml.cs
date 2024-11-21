// .                      @@             _____ _______ _    _ _____ _____ ____
//            @       @@@@@             / ____|__   __| |  | |  __ \_   _/ __ \
//           @@@  @@@@                 | (___    | |  | |  | | |  | || || |  | |
//           @@@@@@@@@  @    @          \___ \   | |  | |  | | |  | || || |  | |
//          @@@@       @@@@@@@          ____) |  | |  | |__| | |__| || || |__| |
//      @@@@@             @@@          |_____/   |_|   \____/|_____/_____\____/
//       @@@      @@@      @@        ___     _    _   _  __   _____  ___  ___  _  _
//        @@    @@@@@@@    @@       |  _|  / _ \ | | | || _ \|_   _|| __|| __|| \| |
//        @@    @@@@@@@    @   @    | __| | (_) || |_| ||   /  | |  | _| | _| | .` |
//      @@@@      @@@      @@@@     |_|    \___/  \___/ |_|_\  |_|  |___||___||_|\_|
//       @@@@             @@@
//         @@@@@      @@@@@               This software is licensed under the
//          @@@@@@@@@@@@@@                 GNU AFFERO GENERAL PUBLIC LICENSE
//              @@@@  @                       Version 3, 19 November 2007

namespace StudioFourteen.Panels;

using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

public partial class PanelWindowResources
{
	private static PanelWindow GetWindow(object sender)
	{
		PanelWindow? window = null;
		if (sender is FrameworkElement el)
		{
			window = el.FindParent<PanelWindow>();
		}

		if (window == null)
			throw new Exception($"Could not find window for element: {sender}");

		return window;
	}

	private void OnTitleMouseDown(object sender, MouseButtonEventArgs e)
	{
		if (e.LeftButton == MouseButtonState.Pressed)
		{
			GetWindow(sender).DragMove();
		}
	}

	private void OnCloseClicked(object sender, RoutedEventArgs e)
	{
		GetWindow(sender).Close();
	}

	private void OnResizeThumbDragDelta(object sender, DragDeltaEventArgs e)
	{
		GetWindow(sender).OnResizeDelta(e);
	}
}
