namespace ScreenshotStudio.Windows;

using System;
using System.Windows;
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
}
