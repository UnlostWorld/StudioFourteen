// © XivTools.
// Licensed under the MIT license.

namespace ScreenshotStudio.Studio;

using ScreenshotStudio.Windows;
using System;
using System.Windows;
using System.Windows.Input;

public partial class StudioButtonWindow : Panel
{
	private Point dragPoint;

	private void OnMouseDown(object sender, MouseButtonEventArgs e)
	{
		if (e.LeftButton == MouseButtonState.Pressed)
		{
			this.dragPoint = e.GetPosition(this);
		}
	}

	private void OnMouseUp(object sender, MouseButtonEventArgs e)
	{
		this.Services.Studio.OpenStudio();
	}

	private void OnMouseMove(object sender, MouseEventArgs e)
	{
		if (e.LeftButton == MouseButtonState.Pressed)
		{
			Point currentPoint = e.GetPosition(this);
			if (Math.Abs(currentPoint.X - this.dragPoint.X) > 5 || Math.Abs(currentPoint.Y - this.dragPoint.Y) > 5)
			{
				this.DragMove();
			}
		}
	}
}