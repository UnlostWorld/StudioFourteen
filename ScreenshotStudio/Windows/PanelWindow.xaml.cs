namespace ScreenshotStudio.Windows;

using System;
using System.Windows;
using System.Windows.Input;

public partial class PanelWindow
{
	private static PanelWindowBase GetWindow(object sender)
	{
		PanelWindowBase? window = null;
		if (sender is FrameworkElement el)
		{
			window = el.FindParent<PanelWindowBase>();
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
}
