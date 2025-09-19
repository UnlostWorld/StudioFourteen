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

namespace StudioFourteen.Behaviors;

using DependencyPropertyGenerator;
using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

[AttachedDependencyProperty<bool, ScrollViewer>("DragScroll")]
public static partial class ScrollViewerExtensions
{
	static partial void OnDragScrollChanged(ScrollViewer scrollViewer, bool newValue)
	{
		if (newValue != true)
			return;

		new ScrollDragBehavior(scrollViewer);
	}
}

public class ScrollDragBehavior : IManipulator
{
	private const double MinDragDistance = 10;

	private readonly ScrollViewer scrollViewer;
	private readonly FieldInfo? panningInfoField;
	private Point startDragPoint;
	private bool isDown = false;
	private bool isManipulating = false;

	public ScrollDragBehavior(ScrollViewer scrollViewer)
	{
		this.scrollViewer = scrollViewer;
		this.scrollViewer.IsManipulationEnabled = true;

		this.scrollViewer.PreviewMouseDown += this.OnPreviewMouseDown;
		this.scrollViewer.MouseMove += this.OnMouseMove;
		this.scrollViewer.PreviewMouseUp += this.OnPreviewMouseUp;
		this.scrollViewer.ManipulationBoundaryFeedback += this.OnManipulationBoundaryFeedback;

		this.panningInfoField = typeof(ScrollViewer).GetField("_panningInfo", BindingFlags.Instance | BindingFlags.NonPublic);
	}

	public event EventHandler? Updated;
	public int Id => this.scrollViewer.GetHashCode();

	public Point GetPosition(IInputElement relativeTo) => Mouse.GetPosition(relativeTo);

	public void ManipulationEnded(bool cancel)
	{
	}

	private void OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
	{
		if (e.Handled)
			return;

		if (Mouse.Captured != null)
			return;

		if (Mouse.DirectlyOver is FrameworkElement fe && fe.FindParent<ScrollBar>() != null)
			return;

		this.isDown = true;
		this.startDragPoint = e.GetPosition(this.scrollViewer);

		if (this.panningInfoField?.GetValue(this.scrollViewer) != null)
		{
			this.isManipulating = true;
			Manipulation.AddManipulator(this.scrollViewer, this);
			Mouse.Capture(this.scrollViewer);
			e.Handled = true;
		}
	}

	private void OnMouseMove(object sender, MouseEventArgs e)
	{
		if (e.Handled)
			return;

		if (!this.isDown)
			return;

		if (Mouse.Captured != null && Mouse.Captured != this.scrollViewer)
			return;

		Point dragPoint = e.GetPosition(this.scrollViewer);
		Vector totalDelta = dragPoint - this.startDragPoint;

		if (totalDelta.Length > MinDragDistance && !this.isManipulating)
		{
			this.isManipulating = true;
			Manipulation.AddManipulator(this.scrollViewer, this);
			Mouse.Capture(this.scrollViewer);
			e.Handled = true;
		}
		else if (this.isManipulating)
		{
			this.Updated?.Invoke(this, e);
			e.Handled = true;
		}
	}

	private void OnPreviewMouseUp(object sender, MouseButtonEventArgs e)
	{
		this.isDown = false;

		if (this.isManipulating)
		{
			try
			{
				Manipulation.RemoveManipulator(this.scrollViewer, this);
			}
			catch
			{
			}

			Mouse.Capture(null);
			this.isManipulating = false;
			e.Handled = true;
		}
	}

	private void OnManipulationBoundaryFeedback(object? sender, ManipulationBoundaryFeedbackEventArgs e)
	{
		// don't bounce the window
		e.Handled = true;
	}
}