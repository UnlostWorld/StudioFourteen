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
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using StudioFourteen.Extensions;

[AttachedDependencyProperty<bool>("UseGroupHighlight")]
[DependencyProperty<double>("Left")]
[DependencyProperty<double>("Right")]
[DependencyProperty<double>("Top")]
[DependencyProperty<double>("Bottom")]
[DependencyProperty<Orientation>("Orientation")]
public partial class RadioButtonHighlighter : ContentControl
{
	private readonly List<RadioButton> radioButtons = new();
	private readonly Storyboard storyboard;
	private readonly DoubleAnimation leftAnimation;
	private readonly DoubleAnimation rightAnimation;
	private readonly DoubleAnimation topAnimation;
	private readonly DoubleAnimation bottomAnimation;
	private FrameworkElement? highlight;
	private int currentIndex = 0;
	private bool isLoading = true;

	public RadioButtonHighlighter()
	{
		this.storyboard = new Storyboard();

		this.leftAnimation = this.CreateAnimation(LeftProperty);
		this.rightAnimation = this.CreateAnimation(RightProperty);
		this.topAnimation = this.CreateAnimation(TopProperty);
		this.bottomAnimation = this.CreateAnimation(BottomProperty);

		this.Loaded += this.OnLoaded;
		this.Unloaded += this.OnUnloaded;
	}

	public override void OnApplyTemplate()
	{
		base.OnApplyTemplate();

		this.highlight = this.GetTemplateChild("PART_Highlight") as FrameworkElement;
		this.Update();
	}

	protected void AddButton(RadioButton button)
	{
		this.radioButtons.Add(button);
		button.Checked += this.OnButtonChecked;
		button.IsVisibleChanged += this.OnButtonVisibilityChanged;
		this.Update();
	}

	protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
	{
		base.OnRenderSizeChanged(sizeInfo);
		this.Update();
	}

	static partial void OnUseGroupHighlightChanged(DependencyObject dependencyObject, bool newValue)
	{
		if (newValue == false)
			return;

		if (dependencyObject is RadioButton btn)
		{
			RadioButtonHighlighter? highlighter = btn.FindParent<RadioButtonHighlighter>();
			if (highlighter == null)
				throw new Exception("No Radio Button highlighter in hierarchy for UseGroupHighlight");

			highlighter.AddButton(btn);
		}
	}

	private DoubleAnimation CreateAnimation(DependencyProperty property)
	{
		DoubleAnimation animation = new();
		animation.Duration = new(TimeSpan.FromMilliseconds(500));
		this.storyboard.Children.Add(animation);
		CubicEase ease = new();
		ease.EasingMode = EasingMode.EaseInOut;
		animation.EasingFunction = ease;
		Storyboard.SetTarget(animation, this);
		Storyboard.SetTargetProperty(animation, new PropertyPath(property.Name));
		return animation;
	}

	private void OnLoaded(object sender, RoutedEventArgs e)
	{
		this.Dispatcher.Invoke(
			() =>
			{
				this.isLoading = false;
				this.Update();
			},
			DispatcherPriority.Background);
	}

	private void OnUnloaded(object sender, RoutedEventArgs e)
	{
		this.isLoading = true;
		this.radioButtons.Clear();
	}

	private void OnButtonVisibilityChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		if (this.isLoading)
			return;

		this.Update();
	}

	private void OnButtonChecked(object sender, RoutedEventArgs e)
	{
		if (this.isLoading)
			return;

		this.Update();
	}

	private void Update()
	{
		this.storyboard.Stop();

		RadioButton? button = null;
		foreach (RadioButton btn in this.radioButtons)
		{
			if (btn.IsChecked != true)
				continue;

			button = btn;
			break;
		}

		if (button == null)
			return;

		int fromIndex = this.currentIndex;
		this.currentIndex = this.radioButtons.IndexOf(button);

		if (!this.IsAncestorOf(button))
			return;

		GeneralTransform transform = button.TransformToAncestor(this);
		Rect bounds = transform.TransformBounds(new(0, 0, button.ActualWidth, button.ActualHeight));

		bool animate = this.IsVisible && !this.isLoading;
		if (animate)
		{
			if (fromIndex > this.currentIndex)
			{
				this.leftAnimation.AccelerationRatio = 0.1;
				this.leftAnimation.DecelerationRatio = 0.5;
				this.rightAnimation.AccelerationRatio = 0.5;
				this.rightAnimation.DecelerationRatio = 0.1;

				this.topAnimation.AccelerationRatio = 0.1;
				this.topAnimation.DecelerationRatio = 0.5;
				this.bottomAnimation.AccelerationRatio = 0.5;
				this.bottomAnimation.DecelerationRatio = 0.1;
			}
			else
			{
				this.leftAnimation.AccelerationRatio = 0.5;
				this.leftAnimation.DecelerationRatio = 0.1;
				this.rightAnimation.AccelerationRatio = 0.1;
				this.rightAnimation.DecelerationRatio = 0.5;

				this.topAnimation.AccelerationRatio = 0.5;
				this.topAnimation.DecelerationRatio = 0.1;
				this.bottomAnimation.AccelerationRatio = 0.1;
				this.bottomAnimation.DecelerationRatio = 0.5;
			}

			this.leftAnimation.To = bounds.Left;
			this.rightAnimation.To = this.ActualWidth - bounds.Right;
			this.topAnimation.To = bounds.Top;
			this.bottomAnimation.To = this.ActualHeight - bounds.Bottom;
			this.storyboard.Begin();
		}
		else
		{
			this.Left = bounds.Left;
			this.Right = this.ActualWidth - bounds.Right;
			this.Top = bounds.Top;
			this.Bottom = this.ActualHeight - bounds.Bottom;
		}
	}

	partial void OnLeftChanged(double newValue)
	{
		if (this.highlight == null)
			return;

		Thickness margin = this.highlight.Margin;
		margin.Left = newValue;
		this.highlight.Margin = margin;
	}

	partial void OnRightChanged(double newValue)
	{
		if (this.highlight == null)
			return;

		Thickness margin = this.highlight.Margin;
		margin.Right = newValue;
		this.highlight.Margin = margin;
	}

	partial void OnTopChanged(double newValue)
	{
		if (this.highlight == null)
			return;

		Thickness margin = this.highlight.Margin;
		margin.Top = newValue;
		this.highlight.Margin = margin;
	}

	partial void OnBottomChanged(double newValue)
	{
		if (this.highlight == null)
			return;

		Thickness margin = this.highlight.Margin;
		margin.Bottom = newValue;
		this.highlight.Margin = margin;
	}
}
