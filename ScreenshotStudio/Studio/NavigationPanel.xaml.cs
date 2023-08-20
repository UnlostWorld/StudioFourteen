// © XivTools.
// Licensed under the MIT license.

namespace ScreenshotStudio.Studio;

using ScreenshotStudio.Windows;
using System.Windows;

public partial class NavigationPanel : DockPanel
{
	private void OnGearClicked(object sender, RoutedEventArgs e) => Panel.Show<GearWindow>();
}
