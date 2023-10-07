namespace ScreenshotStudio.Windows;

using ScreenshotStudio.Utilities;
using System.Windows;

public class DockPanel : PersistentPanel
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

	protected override void OnOpened()
	{
		base.OnOpened();
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
