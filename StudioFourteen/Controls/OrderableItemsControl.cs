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

using System;
using System.Collections;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using DependencyPropertyGenerator;
using WpfUtils;

public class OrderableItemsControl : ItemsControl
{
	private AdornerLayer? adornerLayer;
	private AdornerContentPresenter? dragAdorner;
	private OrderableItemControl? draggingContainer;
	private OrderableItemControl? dropBeforeContainer;
	private double draggingContainerHeight;
	private int fromIndex = -1;
	private int toIndex = -1;

	public override void OnApplyTemplate()
	{
		base.OnApplyTemplate();
	}

	internal void StartDrag(OrderableItemControl container, MouseEventArgs args)
	{
		this.draggingContainer = container;
		object item = container.DataContext;
		this.draggingContainerHeight = container.ActualHeight;
		this.fromIndex = this.Items.IndexOf(item);

		this.adornerLayer = AdornerLayer.GetAdornerLayer(this);
		if (this.adornerLayer != null)
		{
			OrderableItemControl dragChild = new();
			dragChild.Style = this.ItemContainerStyle;
			dragChild.Content = container.DataContext;
			dragChild.ContentTemplate = this.ItemTemplate;

			this.dragAdorner = new(this, dragChild);
			this.adornerLayer?.Add(this.dragAdorner);

			this.dragAdorner.Loaded += (s, e) => this.UpdateDragPosition(args.GetPosition(this));
		}

		container.Opacity = 0;
		container.Height = 0;

		if (this.fromIndex + 1 < this.Items.Count)
		{
			this.dropBeforeContainer = this.ItemContainerGenerator.ContainerFromIndex(this.fromIndex + 1) as OrderableItemControl;
			this.dropBeforeContainer?.SetTopGhost(this.draggingContainerHeight, false);
		}
	}

	internal void UpdateDrag(OrderableItemControl container, MouseEventArgs args)
	{
		if (container != this.draggingContainer)
			return;

		this.UpdateDragPosition(args.GetPosition(this));

		this.toIndex = -1;
		for (int i = 0; i < this.Items.Count; i++)
		{
			OrderableItemControl? otherContainer = this.ItemContainerGenerator.ContainerFromIndex(i) as OrderableItemControl;
			if (otherContainer == null)
				continue;

			Point p = args.GetPosition(otherContainer);

			double posY = p.Y + otherContainer.Margin.Top;
			double otherContainerTotalHeight = otherContainer.ActualHeight + otherContainer.Margin.Bottom + otherContainer.Margin.Top;

			// Before this container
			if (posY > 0
				&& posY <= otherContainerTotalHeight / 2
				&& posY <= otherContainerTotalHeight)
			{
				this.toIndex = i;
			}

			// After this container
			if (posY > 0
				&& posY > otherContainerTotalHeight / 2
				&& posY <= otherContainerTotalHeight)
			{
				this.toIndex = i + 1;
			}
		}

		// Check drag to top
		if (this.toIndex == -1)
		{
			OrderableItemControl? otherContainer = this.ItemContainerGenerator.ContainerFromIndex(0) as OrderableItemControl;
			if (otherContainer != null)
			{
				Point p = args.GetPosition(otherContainer);
				double posY = p.Y + otherContainer.Margin.Top;
				if (posY < 0)
				{
					this.toIndex = 0;
				}
			}
		}

		// Check drag to end
		if (this.toIndex == -1)
		{
			OrderableItemControl? otherContainer = this.ItemContainerGenerator.ContainerFromIndex(this.Items.Count - 1) as OrderableItemControl;
			if (otherContainer != null)
			{
				Point p = args.GetPosition(otherContainer);
				double posY = p.Y + otherContainer.Margin.Top;
				double otherContainerTotalHeight = otherContainer.ActualHeight + otherContainer.Margin.Bottom + otherContainer.Margin.Top;

				if (posY > otherContainerTotalHeight)
				{
					this.toIndex = this.Items.Count;
				}
			}
		}

		OrderableItemControl? newDropBeforeContainer = null;

		if (this.toIndex >= 0 && this.toIndex < this.Items.Count)
			newDropBeforeContainer = this.ItemContainerGenerator.ContainerFromIndex(this.toIndex) as OrderableItemControl;

		if (this.dropBeforeContainer != newDropBeforeContainer)
		{
			this.dropBeforeContainer?.SetTopGhost(0);
			this.dropBeforeContainer = newDropBeforeContainer;
			this.dropBeforeContainer?.SetTopGhost(this.draggingContainerHeight);
		}
	}

	internal void StopDrag(OrderableItemControl container, MouseEventArgs args)
	{
		this.dropBeforeContainer?.SetTopGhost(0, false);
		this.dropBeforeContainer = null;

		if (this.draggingContainer != null)
		{
			this.draggingContainer.Opacity = 1;
			this.draggingContainer.Height = double.NaN;
			this.draggingContainer = null;
		}

		this.adornerLayer?.Remove(this.dragAdorner);

		if (this.toIndex > this.fromIndex)
			this.toIndex--;

		if (this.toIndex < 0)
			this.toIndex = 0;

		if (this.toIndex > this.Items.Count - 1)
			this.toIndex = this.Items.Count - 1;

		object item = container.DataContext;

		if (this.ItemsSource != null)
		{
			if (this.ItemsSource is IList list)
			{
				list.RemoveAt(this.fromIndex);
				list.Insert(this.toIndex, item);
				this.ItemsSource = list;
			}
		}
		else
		{
			this.Items.RemoveAt(this.fromIndex);
			this.Items.Insert(this.toIndex, item);
		}
	}

	protected override bool IsItemItsOwnContainerOverride(object item) => item is OrderableItemControl;
	protected override DependencyObject GetContainerForItemOverride() => new OrderableItemControl();

	private void UpdateDragPosition(Point position)
	{
		if (this.dragAdorner == null)
			return;

		double offset = 22; // ??

		double y = position.Y;
		double min = (this.draggingContainerHeight / 2) - 6;
		double max = this.ActualHeight - (this.draggingContainerHeight / 2) - 6;
		y = double.Clamp(y, min, max);

		TranslateTransform moveTransform = new(offset, y + this.draggingContainerHeight + offset);
		this.dragAdorner.RenderTransform = moveTransform;
	}
}

[DependencyProperty<double>("TopGhostSize")]
public partial class OrderableItemControl : ContentControl
{
	private readonly Storyboard ghostStoryboard;
	private readonly DoubleAnimation topGhostAnimation;
	private OrderableItemsControl? itemsControl;
	private Grip? dragGrip;

	public OrderableItemControl()
	{
		this.ghostStoryboard = new();

		this.topGhostAnimation = new();
		this.topGhostAnimation.Duration = new(TimeSpan.FromMilliseconds(250));
		this.ghostStoryboard.Children.Add(this.topGhostAnimation);
		CubicEase ease = new();
		ease.EasingMode = EasingMode.EaseInOut;
		this.topGhostAnimation.EasingFunction = ease;
		Storyboard.SetTarget(this.topGhostAnimation, this);
		Storyboard.SetTargetProperty(this.topGhostAnimation, new PropertyPath(TopGhostSizeProperty.Name));
	}

	public override void OnApplyTemplate()
	{
		base.OnApplyTemplate();

		this.itemsControl = this.FindParent<OrderableItemsControl>();

		if (this.dragGrip != null)
		{
			this.dragGrip.DragStart -= this.OnDragStart;
			this.dragGrip.DragMove -= this.OnDragMove;
			this.dragGrip.DragEnd -= this.OnDragEnd;
		}

		this.dragGrip = this.GetTemplateChild("PART_Grip") as Grip;

		if (this.dragGrip != null)
		{
			this.dragGrip.DragStart += this.OnDragStart;
			this.dragGrip.DragMove += this.OnDragMove;
			this.dragGrip.DragEnd += this.OnDragEnd;
		}
	}

	internal void SetTopGhost(double size, bool animate = true)
	{
		if (!animate)
		{
			this.TopGhostSize = size;
			this.ghostStoryboard.Stop();
		}
		else
		{
			this.topGhostAnimation.To = size;
			this.ghostStoryboard.Begin();
		}
	}

	private void OnDragStart(Grip sender, MouseEventArgs args)
	{
		this.itemsControl?.StartDrag(this, args);
	}

	private void OnDragMove(Grip sender, MouseEventArgs args)
	{
		this.itemsControl?.UpdateDrag(this, args);
	}

	private void OnDragEnd(Grip sender, MouseEventArgs args)
	{
		this.itemsControl?.StopDrag(this, args);
	}

	partial void OnTopGhostSizeChanged(double newValue)
	{
		var m = this.Margin;
		m.Top = newValue;
		this.Margin = m;
	}
}