namespace ScreenshotStudio.Windows;

using ScreenshotStudio.Utilities;
using System;
using System.Windows;

public class DockPanel : PersistentPanel
{
	public static readonly DependencyProperty DockPositionProperty = DependencyProperty.Register(
		nameof(DockPanel.DockPosition),
		typeof(Point),
		typeof(DockPanel),
		new(new Point(0.5, 0.5), OnDockPositionChanged));

	public static readonly DependencyProperty CanActivateProperty = DependencyProperty.Register(
		nameof(DockPanel.CanActivate),
		typeof(bool),
		typeof(DockPanel),
		new(false));

	public Point DockPosition
	{
		get => (Point)this.GetValue(DockPositionProperty);
		set => this.SetValue(DockPositionProperty, value);
	}

	public bool CanActivate
	{
		get => (bool)this.GetValue(CanActivateProperty);
		set => this.SetValue(CanActivateProperty, value);
	}

	protected override Style GetDefaultStyle() => (Style)this.FindResource("DockPanelStyle");

	protected override void OnOpened()
	{
		base.OnOpened();
		this.UpdatePosition();

		if (!this.CanActivate)
		{
			XivWindow.Activate();
		}
	}

	protected override void OnActivated(EventArgs e)
	{
		base.OnActivated(e);

		if (!this.CanActivate)
		{
			// Don't want to be activated. Dock panel is Shy.
			XivWindow.Activate();
		}
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
