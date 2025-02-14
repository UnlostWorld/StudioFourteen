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

namespace StudioFourteen.Photos;

using StudioFourteen.Mvm;
using DependencyPropertyGenerator;
using System.ComponentModel;
using System.Windows;
using System;
using System.Windows.Media.Animation;

[DependencyProperty<double>("BorderHalfWidth")]
[DependencyProperty<double>("BorderHalfHeight")]
[DependencyProperty<double>("AspectBoxHeight")]
[DependencyProperty<double>("AspectBoxWidth")]
[DependencyProperty<double>("CaptureOpacity")]
public partial class PhotoGuides : View
{
	private readonly Storyboard changeAspectStoryboard;
	private readonly DoubleAnimation borderWidthAnimation;
	private readonly DoubleAnimation borderHeightAnimation;
	private readonly DoubleAnimation aspectBoxHeightAnimation;
	private readonly DoubleAnimation aspectBoxWidthAnimation;
	private readonly DoubleAnimation captureOpacityAnimation;

	public PhotoGuides()
	{
		this.changeAspectStoryboard = new();

		// BorderHalfWidth
		this.borderWidthAnimation = new();
		this.borderWidthAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(100));
		this.borderWidthAnimation.EasingFunction = new SineEase()
		{
			EasingMode = EasingMode.EaseOut,
		};

		Storyboard.SetTarget(this.borderWidthAnimation, this);
		Storyboard.SetTargetProperty(this.borderWidthAnimation, new(nameof(this.BorderHalfWidth)));
		this.changeAspectStoryboard.Children.Add(this.borderWidthAnimation);

		// BorderHalfHeight
		this.borderHeightAnimation = new();
		this.borderHeightAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(100));
		this.borderHeightAnimation.EasingFunction = new SineEase()
		{
			EasingMode = EasingMode.EaseOut,
		};

		Storyboard.SetTarget(this.borderHeightAnimation, this);
		Storyboard.SetTargetProperty(this.borderHeightAnimation, new(nameof(this.BorderHalfHeight)));
		this.changeAspectStoryboard.Children.Add(this.borderHeightAnimation);

		// AspectBoxHeight
		this.aspectBoxHeightAnimation = new();
		this.aspectBoxHeightAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(100));
		this.aspectBoxHeightAnimation.EasingFunction = new SineEase()
		{
			EasingMode = EasingMode.EaseOut,
		};

		Storyboard.SetTarget(this.aspectBoxHeightAnimation, this);
		Storyboard.SetTargetProperty(this.aspectBoxHeightAnimation, new(nameof(this.AspectBoxHeight)));
		this.changeAspectStoryboard.Children.Add(this.aspectBoxHeightAnimation);

		// AspectBoxWidth
		this.aspectBoxWidthAnimation = new();
		this.aspectBoxWidthAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(100));
		this.aspectBoxWidthAnimation.EasingFunction = new SineEase()
		{
			EasingMode = EasingMode.EaseOut,
		};

		Storyboard.SetTarget(this.aspectBoxWidthAnimation, this);
		Storyboard.SetTargetProperty(this.aspectBoxWidthAnimation, new(nameof(this.AspectBoxWidth)));
		this.changeAspectStoryboard.Children.Add(this.aspectBoxWidthAnimation);

		// CaptureOpacity
		this.captureOpacityAnimation = new();
		this.captureOpacityAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(100));
		this.captureOpacityAnimation.EasingFunction = new SineEase()
		{
			EasingMode = EasingMode.EaseOut,
		};

		Storyboard.SetTarget(this.captureOpacityAnimation, this);
		Storyboard.SetTargetProperty(this.captureOpacityAnimation, new(nameof(this.CaptureOpacity)));
		this.changeAspectStoryboard.Children.Add(this.captureOpacityAnimation);

		this.Services.Photos.PropertyChanged += this.OnPhotosPropertyChanged;
		this.CalculateAspectBox();
	}

	protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
	{
		base.OnRenderSizeChanged(sizeInfo);
		this.CalculateAspectBox();
	}

	private void OnPhotosPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName == nameof(PhotosService.AspectRatio)
			|| e.PropertyName == nameof(PhotosService.IsPortrait))
		{
			try
			{
				this.CalculateAspectBox();
			}
			catch (Exception ex)
			{
				this.Log.Error(ex, "Error in photo guides view");
			}
		}
	}

	private void CalculateAspectBox()
	{
		double aspect = this.Services.Photos.AspectRatio;

		double height = this.ActualHeight;
		double width = this.ActualWidth;

		if (this.Services.Photos.IsPortrait)
		{
			double realAspect = this.ActualHeight / this.ActualWidth;

			height = this.ActualHeight;
			width = this.ActualHeight * realAspect;

			if (aspect <= 0)
			{
				aspect = realAspect;
			}
			else
			{
				aspect = 1 / aspect;
			}
		}

		double boxHeight = height;
		double boxWidth = width;

		if (aspect > 0)
		{
			boxHeight = height;
			boxWidth = aspect * height;
		}

		if (boxWidth > width)
		{
			double scale = width / boxWidth;

			boxHeight *= scale;
			boxWidth *= scale;
		}

		this.Dispatcher.Invoke(() =>
		{
			this.aspectBoxHeightAnimation.To = boxHeight;
			this.aspectBoxWidthAnimation.To = boxWidth;
			this.borderWidthAnimation.To = (this.ActualWidth - boxWidth) / 2;
			this.borderHeightAnimation.To = (this.ActualHeight - boxHeight) / 2;
			this.captureOpacityAnimation.To = this.Services.Photos.IsPortrait ? 1.0 : 0.0;
			this.BeginStoryboard(this.changeAspectStoryboard);
		});
	}
}
