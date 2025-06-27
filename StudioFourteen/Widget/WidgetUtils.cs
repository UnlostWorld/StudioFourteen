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

namespace StudioFourteen.Widget;

using System.Windows;
using DependencyPropertyGenerator;

[AttachedDependencyProperty<bool>("HideInsideWidget")]
[AttachedDependencyProperty<bool>("HideOutsideWidget")]
public partial class View
{
	static partial void OnHideInsideWidgetChanged(DependencyObject dependencyObject, bool newValue)
	{
		if (dependencyObject is FrameworkElement fe)
		{
			if (!fe.IsLoaded)
			{
				fe.Loaded += (s, e) => OnHideInsideWidgetChanged(dependencyObject, newValue);
				return;
			}

			SelectionWidget? widget = fe.FindParent<SelectionWidget>();
			if (widget != null && newValue)
			{
				fe.Visibility = Visibility.Collapsed;
			}
			else
			{
				fe.Visibility = Visibility.Visible;
			}
		}
	}

	static partial void OnHideOutsideWidgetChanged(DependencyObject dependencyObject, bool newValue)
	{
		if (dependencyObject is FrameworkElement fe)
		{
			if (!fe.IsLoaded)
			{
				fe.Loaded += (s, e) => OnHideOutsideWidgetChanged(dependencyObject, newValue);
				return;
			}

			SelectionWidget? widget = fe.FindParent<SelectionWidget>();
			if (widget != null || !newValue)
			{
				fe.Visibility = Visibility.Visible;
			}
			else
			{
				fe.Visibility = Visibility.Collapsed;
			}
		}
	}
}