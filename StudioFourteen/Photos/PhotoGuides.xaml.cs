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

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using DependencyPropertyGenerator;
using PropertyChanged.SourceGenerator;
using StudioFourteen.Mvm;
using WpfUtils;

using static StudioFourteen.Photos.PhotosService;

[DependencyProperty<double>("PhotoOpacity", DefaultValue = 0)]
[DependencyProperty<double>("PhotoScale", DefaultValue = 1)]
[DependencyProperty<double>("PhotoAngle", DefaultValue = 0)]
[DependencyProperty<double>("PhotoOffset", DefaultValue = 0)]
public partial class PhotoGuides : View
{
	private readonly Stopwatch photoFadeDelayTimer = new();

	private readonly Storyboard captureStoryboard;
	private readonly DoubleAnimation photoOpacityAnimation;
	private readonly DoubleAnimation photoScaleAnimation;
	private readonly DoubleAnimation photoAngleAnimation;
	private readonly DoubleAnimation photoOffsetAnimation;

	[Notify] private int captureAngle = 0;
	[Notify] private bool showCapture = false;
	[Notify] private WriteableBitmap? lastSavedImage;

	public PhotoGuides()
	{
		// Capture
		this.captureStoryboard = new();
		this.photoOpacityAnimation = this.captureStoryboard.CreateAnimation<DoubleAnimation>(this, PhotoOpacityProperty, 100, Easing.SineOut);
		this.photoScaleAnimation = this.captureStoryboard.CreateAnimation<DoubleAnimation>(this, PhotoScaleProperty, 1000, Easing.SineInOut);
		this.photoAngleAnimation = this.captureStoryboard.CreateAnimation<DoubleAnimation>(this, PhotoAngleProperty, 1000, Easing.SineOut);
		this.photoOffsetAnimation = this.captureStoryboard.CreateAnimation<DoubleAnimation>(this, PhotoOffsetProperty, 1000, Easing.SineIn);

		if (DesignerProperties.GetIsInDesignMode(this))
			return;

		this.Services.Photos.PhaseChanged += this.OnPhaseChanged;
	}

	private async Task OnPhaseChanged(CapturePhases fromPhase, CapturePhases toPhase, CancellationToken cancellationToken, bool animate)
	{
		if (!animate)
			return;

		await this.MainThread();

		this.StopStoryboard(this.captureStoryboard);

		if (toPhase == CapturePhases.Saved)
		{
			if (this.Settings.PhotoAnimationPreview)
			{
				this.photoOpacityAnimation.Duration = new(TimeSpan.FromMilliseconds(150));
				this.photoOpacityAnimation.To = 0;
				this.photoOpacityAnimation.To = 1;
				this.photoScaleAnimation.Duration = new(TimeSpan.FromMilliseconds(150));
				this.photoScaleAnimation.To = 0.75;
				this.photoAngleAnimation.Duration = new(TimeSpan.FromMilliseconds(150));
				this.photoAngleAnimation.To = (Random.Shared.NextDouble() * 10) - 5;
				this.photoOffsetAnimation.Duration = new(TimeSpan.FromMilliseconds(150));
				this.photoOffsetAnimation.To = 0;

				this.photoFadeDelayTimer.Restart();
			}

			if (this.Settings.PhotoAnimationFlash || this.Settings.PhotoAnimationPreview)
			{
				this.BeginStoryboard(this.captureStoryboard);
				await Task.Delay(150, cancellationToken);
			}
		}
		else if (toPhase == CapturePhases.Done)
		{
			if (this.Settings.PhotoAnimationPreview)
			{
				this.photoOpacityAnimation.From = 1;
				this.photoOpacityAnimation.To = 1;
			}

			this.BeginStoryboard(this.captureStoryboard);

			if (this.Settings.PhotoAnimationPreview)
			{
				this.photoFadeDelayTimer.Start();

				if (this.photoFadeDelayTimer.ElapsedMilliseconds < 1500)
					await Task.Delay((int)(1500 - this.photoFadeDelayTimer.ElapsedMilliseconds), cancellationToken);

				this.photoFadeDelayTimer.Stop();
				await this.MainThread();

				this.photoOpacityAnimation.Duration = new(TimeSpan.FromMilliseconds(500));
				this.photoOpacityAnimation.From = 1;
				this.photoOpacityAnimation.To = 0;
				this.photoScaleAnimation.Duration = new(TimeSpan.FromMilliseconds(500));
				this.photoScaleAnimation.To = 0.25;
				this.photoAngleAnimation.Duration = new(TimeSpan.FromMilliseconds(500));
				this.photoAngleAnimation.To = 0;
				this.photoOffsetAnimation.Duration = new(TimeSpan.FromMilliseconds(500));
				this.photoOffsetAnimation.To = 400;

				this.BeginStoryboard(this.captureStoryboard);
				await Task.Delay(500, cancellationToken);
			}
		}
	}
}
