// © XivTools.
// Licensed under the MIT license.

namespace ScreenshotStudio.Windows;

using ScreenshotStudio.Utilities;
using System.Windows;

public class DockPanel : Panel
{
	public static readonly DependencyProperty DockPositionProperty = DependencyProperty.Register(
		nameof(DockPanel.DockPosition),
		typeof(Point),
		typeof(DockPanel),
		new(new Point(0.5, 0.5), OnDockPositionChanged));

	public Point DockPosition
	{
		get => (Point)this.GetValue(DockPositionProperty);
		set => this.SetValue(DockPositionProperty, value);
	}

	protected override Style GetDefaultStyle() => (Style)this.FindResource("DockPanelStyle");

	protected override void OnLoaded(object sender, RoutedEventArgs e)
	{
		base.OnLoaded(sender, e);
		this.UpdatePosition();
	}

	private static void OnDockPositionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is DockPanel dock)
		{
			dock.UpdatePosition();
		}
	}

	private void UpdatePosition()
	{
		this.MaxWidth = XivWindow.Size.Width;
		this.MaxHeight = XivWindow.Size.Height;

		XivWindow.SetPosition(this, this.DockPosition);
	}
}
