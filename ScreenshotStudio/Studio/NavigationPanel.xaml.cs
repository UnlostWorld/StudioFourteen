// © XivTools.
// Licensed under the MIT license.

namespace ScreenshotStudio.Studio;

using ScreenshotStudio.Windows;
using System.Windows;

public partial class NavigationPanel : DockPanel
{
	private void OnCloseClicked(object sender, RoutedEventArgs e) => this.Services.Studio.CloseStudio();
	private void OnGearClicked(object sender, RoutedEventArgs e) => Panel.Show<GearWindow>();
	private void OnCustomizeClicked(object sender, RoutedEventArgs e) => Panel.Show<CustomizeWindow>();
}
