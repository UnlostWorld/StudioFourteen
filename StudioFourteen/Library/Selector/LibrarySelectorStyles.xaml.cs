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

namespace StudioFourteen.Library.Selector;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

public partial class LibrarySelectorStyles
{
	private void OnResultToolTipOpening(object sender, ToolTipEventArgs e)
	{
		e.Handled = true;

		if (sender is FrameworkElement el && el.Tag is LibrarySelector selector)
		{
			selector.OnResultToolTipOpening(sender, e);
		}
	}

	private void OnResultPreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
	{
		e.Handled = true;
		if (sender is FrameworkElement el && el.Tag is LibrarySelector selector)
		{
			selector.OnResultMouseRightButtonUp(sender, e);
		}
	}

	private void OnResultPreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
	{
		e.Handled = true;
	}
}
