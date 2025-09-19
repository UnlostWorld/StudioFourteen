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

namespace StudioFourteen.Controls;

using DependencyPropertyGenerator;
using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Threading;

[DependencyProperty<ControlTemplate>("Template")]
[DependencyProperty<object>("Content")]
[DependencyProperty<DataTemplate>("ContentTemplate")]
[AttachedDependencyProperty<object>("Header")]
[AttachedDependencyProperty<DataTemplate>("HeaderTemplate")]
[DependencyProperty<Thickness>("Padding")]
[DependencyProperty<Brush>("Background")]
[ContentProperty("Content")]
public partial class PopOut : Popup, IAddChild
{
	private readonly PopOutContentControl contentPresenter;

	public PopOut()
	{
		this.contentPresenter = new(this);
		this.contentPresenter.Padding = this.Padding;
		this.contentPresenter.Background = this.Background;
		this.Child = this.contentPresenter;

		this.IsHitTestVisibleChanged += this.OnIsHitTestVisibleChanged;
	}

	public static PopOut Show(UIElement placementTarget, UIElement content)
	{
		PopOut host = new();
		host.Content = content;
		host.PlacementTarget = placementTarget;
		host.AllowsTransparency = true;
		host.IsHitTestVisible = false;
		host.IsOpen = true;
		return host;
	}

	void IAddChild.AddChild(object value)
	{
		this.contentPresenter.Content = value;
	}

	protected override void OnOpened(EventArgs e)
	{
		base.OnOpened(e);

		this.SetHitTestable(this.IsHitTestVisible);

		this.UpdatePlacement();

		if (this.PlacementTarget is FrameworkElement fe)
		{
			fe.Unloaded += this.OnTargetUnloaded;
		}
	}

	protected new DependencyObject? GetTemplateChild(string childName)
	{
		return this.contentPresenter.GetTemplateChildWrapper(childName);
	}

	private void OnTargetUnloaded(object sender, RoutedEventArgs e)
	{
		this.IsOpen = false;
	}

	partial void OnTemplateChanged(ControlTemplate? newValue)
	{
		this.contentPresenter.Template = newValue;
	}

	partial void OnContentChanged(object? newValue)
	{
		this.contentPresenter.Content = newValue;
	}

	partial void OnPaddingChanged(Thickness newValue)
	{
		this.contentPresenter.Padding = newValue;
	}

	partial void OnBackgroundChanged(Brush? newValue)
	{
		if (newValue == null)
			newValue = new SolidColorBrush(Colors.Transparent);

		this.contentPresenter.Background = newValue;
	}

	private void OnIsHitTestVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		this.SetHitTestable(this.IsHitTestVisible);
	}

	private void SetHitTestable(bool hitTestable)
	{
		try
		{
			MethodInfo? method = typeof(Popup).GetMethod("SetHitTestable", BindingFlags.NonPublic | BindingFlags.Instance);
			method?.Invoke(this, [hitTestable]);
		}
		catch (Exception)
		{
		}
	}

	private void UpdatePlacement()
	{
		if (this.PlacementTarget is FrameworkElement el)
		{
			double offsetX = 0;
			double offsetY = 0;
			PlacementMode placement = this.Placement;
			UIElement placementTarget = this.PlacementTarget;
			Rect placementRect = this.PlacementRectangle;

			// Try to get the popup size. If its size is empty, use the size
			// of its child, if any child exists.
			Size popupSize = this.GetElementSize(this);
			UIElement child = this.Child;
			if ((popupSize.IsEmpty || popupSize.Width <= 0 || popupSize.Height <= 0)
				&& child != null)
			{
				popupSize = this.GetElementSize(child);
			}

			// Get the placement rectangle size. If it's empty, get the
			// placement target's size, if a target is set.
			Size targetSize;
			if (placementRect.Width > 0 && placementRect.Height > 0)
			{
				targetSize = placementRect.Size;
			}
			else if (placementTarget != null)
			{
				targetSize = this.GetElementSize(placementTarget);
			}
			else
			{
				targetSize = Size.Empty;
			}

			// If we have a valid popup size and a valid target size, determine
			// the offset needed to align the popup to the target rectangle.
			if (!popupSize.IsEmpty && popupSize.Width > 0 && popupSize.Height > 0
				&& !targetSize.IsEmpty && targetSize.Width > 0 && targetSize.Height > 0)
			{
				switch (placement)
				{
					// Horizontal alignment offset.
					case PlacementMode.Top:
					case PlacementMode.Bottom:
					{
						offsetX = (popupSize.Width - targetSize.Width) / 2;
						offsetY = -8;
						break;
					}

					// Vertical alignment offset.
					case PlacementMode.Left:
					case PlacementMode.Right:
					{
						offsetY = -24;
						offsetX = -8;
						break;
					}
				}
			}

			// Apply the final computed offsets to the popup.
			this.HorizontalOffset = offsetX;
			this.VerticalOffset = offsetY;
		}
	}

	private Size GetElementSize(UIElement element)
	{
		if (element is null)
		{
			return new Size(0d, 0d);
		}
		else if (element is FrameworkElement frameworkElement)
		{
			return new Size(frameworkElement.ActualWidth, frameworkElement.ActualHeight);
		}
		else
		{
			return element.RenderSize;
		}
	}
}

public class PopOutContentControl(PopOut owner)
	: ContentControl
{
	public override void OnApplyTemplate()
	{
		base.OnApplyTemplate();
		owner.OnApplyTemplate();
	}

	public DependencyObject? GetTemplateChildWrapper(string childName)
	{
		return this.GetTemplateChild(childName);
	}
}