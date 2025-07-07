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

namespace StudioFourteen.Selection;

using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using DependencyPropertyGenerator;

[AttachedDependencyProperty<bool>("HideInsideWidget")]
[AttachedDependencyProperty<bool>("HideOutsideWidget")]
public partial class WidgetUtils
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

			Widget? widget = fe.FindParent<Widget>();
			SetItemVisibility(fe, (widget == null || !newValue));
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

			Widget? widget = fe.FindParent<Widget>();
			SetItemVisibility(fe, (widget != null || !newValue));
		}
	}

	private static void SetItemVisibility(FrameworkElement el, bool visible)
	{
		el.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
		el.IsEnabled = visible;

		if (!visible && el is TabItem ti && ti.IsSelected)
		{
			ti.IsSelected = false;

			// Ensure the tab control has a tab selected
			TabControl? tc = ti.FindParent<TabControl>();
			List<TabItem>? tabItems = tc?.FindChildren<TabItem>();
			if (tabItems != null)
			{
				foreach (TabItem item in tabItems)
				{
					if (item == ti)
						continue;

					item.IsSelected = true;
					break;
				}
			}
		}
	}
}