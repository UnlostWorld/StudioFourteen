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
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using DependencyPropertyGenerator;
using PropertyChanged.SourceGenerator;
using StudioFourteen.Panels;
using WpfUtils;

using static StudioFourteen.Photos.PhotosService;

[DependencyProperty<Color>("FillColor")]
[DependencyProperty<double>("SpinnerOpacity", DefaultValue = 0)]
[DependencyProperty<double>("PhotoOpacity", DefaultValue = 0)]
[DependencyProperty<double>("PhotoScale", DefaultValue = 1)]
[DependencyProperty<double>("PhotoAngle", DefaultValue = 0)]
[DependencyProperty<double>("PhotoOffset", DefaultValue = 0)]
public partial class CaptureAnimationWindow : PanelWindow
{
	private readonly Stopwatch photoFadeDelayTimer = new();
	private readonly Storyboard captureStoryboard;
	private readonly ColorAnimation fillColorAnimation;
	private readonly DoubleAnimation spinnerOpacityAnimation;
	private readonly DoubleAnimation photoOpacityAnimation;
	private readonly DoubleAnimation photoScaleAnimation;
	private readonly DoubleAnimation photoAngleAnimation;
	private readonly DoubleAnimation photoOffsetAnimation;

	private readonly MemoryStream previewImageStream = new();

	[Notify] private int captureAngle = 0;
	[Notify] private bool showCapture = false;
	[Notify] private BitmapImage? lastImage;

	public CaptureAnimationWindow()
	{
		// Capture
		this.captureStoryboard = new();
		this.fillColorAnimation = this.captureStoryboard.CreateAnimation<ColorAnimation>(this, FillColorProperty, 100, Easing.SineOut);
		this.spinnerOpacityAnimation = this.captureStoryboard.CreateAnimation<DoubleAnimation>(this, SpinnerOpacityProperty, 250, Easing.SineOut);
		this.photoOpacityAnimation = this.captureStoryboard.CreateAnimation<DoubleAnimation>(this, PhotoOpacityProperty, 100, Easing.SineOut);
		this.photoScaleAnimation = this.captureStoryboard.CreateAnimation<DoubleAnimation>(this, PhotoScaleProperty, 1000, Easing.SineInOut);
		this.photoAngleAnimation = this.captureStoryboard.CreateAnimation<DoubleAnimation>(this, PhotoAngleProperty, 1000, Easing.SineOut);
		this.photoOffsetAnimation = this.captureStoryboard.CreateAnimation<DoubleAnimation>(this, PhotoOffsetProperty, 1000, Easing.SineIn);

		if (DesignerProperties.GetIsInDesignMode(this))
			return;

		this.Services.Photos.PhaseChanged += this.OnPhaseChanged;
	}

	public override bool CanNavigate => false;

	protected override void OnOpened()
	{
		base.OnOpened();
		this.UpdatePosition();
	}

	private void UpdatePosition()
	{
		if (this.Services.Windows.XivProcess == null)
			return;

		Rect xivWindowSize = this.Services.Windows.GetXivWindowClientSize();

		this.Width = xivWindowSize.Width;
		this.Height = xivWindowSize.Height;

		this.Services.Windows.SetPosition(this, new(0, 0), false);
		this.Services.Windows.SendToBack(this);
	}

	private async Task OnPhaseChanged(CapturePhases fromPhase, CapturePhases toPhase, CancellationToken cancellationToken, bool animate)
	{
		if (!animate)
			return;

		await this.MainThread();

		this.StopStoryboard(this.captureStoryboard);

		switch (toPhase)
		{
			case CapturePhases.Starting:
			{
				this.fillColorAnimation.From = Colors.Transparent;
				this.fillColorAnimation.To = Colors.Transparent;
				this.fillColorAnimation.Duration = new(TimeSpan.FromMilliseconds(1));

				this.photoOpacityAnimation.Duration = new(TimeSpan.FromMilliseconds(1));
				this.photoOpacityAnimation.From = 0;
				this.photoOpacityAnimation.To = 0;
				this.photoScaleAnimation.Duration = new(TimeSpan.FromMilliseconds(1));
				this.photoScaleAnimation.To = 1;
				this.photoAngleAnimation.Duration = new(TimeSpan.FromMilliseconds(1));
				this.photoAngleAnimation.To = 0;
				this.photoOffsetAnimation.Duration = new(TimeSpan.FromMilliseconds(1));
				this.photoOffsetAnimation.To = 0;

				this.BeginStoryboard(this.captureStoryboard);
				await Task.Delay(1);

				break;
			}

			case CapturePhases.ChangingResolution:
			case CapturePhases.WaitingForReshade:
			{
				this.fillColorAnimation.From = null;
				this.fillColorAnimation.To = Colors.Black;
				this.fillColorAnimation.Duration = new(TimeSpan.FromMilliseconds(100));

				this.spinnerOpacityAnimation.To = 1;

				this.photoOpacityAnimation.Duration = new(TimeSpan.FromMilliseconds(1));
				this.photoOpacityAnimation.To = 0;

				this.BeginStoryboard(this.captureStoryboard);
				await Task.Delay(100, cancellationToken);
				break;
			}

			case CapturePhases.Capturing:
			{
				if (this.Services.Settings.Current.PhotoAnimationFlash)
				{
					this.fillColorAnimation.From = null;
					this.fillColorAnimation.To = Colors.White;
					this.fillColorAnimation.Duration = new(TimeSpan.FromMilliseconds(50));
				}

				this.spinnerOpacityAnimation.To = 0;

				this.BeginStoryboard(this.captureStoryboard);
				await Task.Delay(75, cancellationToken);
				break;
			}

			case CapturePhases.Saving:
			{
				break;
			}

			case CapturePhases.Saved:
			{
				if (this.Services.Settings.Current.PhotoAnimationFlash)
				{
					this.fillColorAnimation.From = null;
					this.fillColorAnimation.To = Colors.White;
					this.fillColorAnimation.Duration = new(TimeSpan.FromMilliseconds(100));
				}
				else
				{
					this.fillColorAnimation.To = Colors.Transparent;
				}

				if (this.Services.Settings.Current.PhotoAnimationPreview && this.Services.Photos.LastSavedImagePath != null)
				{
					using FileStream fs = new FileStream(this.Services.Photos.LastSavedImagePath, FileMode.Open, FileAccess.Read);

					this.previewImageStream.Position = 0;
					fs.CopyTo(this.previewImageStream);
					this.previewImageStream.Position = 0;

					BitmapImage imageSource = new();
					imageSource.BeginInit();
					imageSource.StreamSource = this.previewImageStream;
					imageSource.EndInit();
					this.LastImage = imageSource;

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

				if (this.Services.Settings.Current.PhotoAnimationFlash || this.Services.Settings.Current.PhotoAnimationPreview)
				{
					this.BeginStoryboard(this.captureStoryboard);
					await Task.Delay(150, cancellationToken);
				}

				break;
			}

			case CapturePhases.RestoreResolution:
			{
				this.fillColorAnimation.From = null;
				this.fillColorAnimation.To = Colors.Black;
				this.fillColorAnimation.Duration = new(TimeSpan.FromMilliseconds(250));

				this.spinnerOpacityAnimation.To = 1;

				if (this.Services.Settings.Current.PhotoAnimationPreview)
				{
					this.photoOpacityAnimation.From = 1;
					this.photoOpacityAnimation.To = 1;
				}

				this.BeginStoryboard(this.captureStoryboard);
				await Task.Delay(250, cancellationToken);
				break;
			}

			case CapturePhases.WaitingForReshadeReset:
			{
				break;
			}

			case CapturePhases.Done:
			{
				this.fillColorAnimation.From = null;
				this.fillColorAnimation.To = Colors.Transparent;
				this.fillColorAnimation.Duration = new(TimeSpan.FromMilliseconds(500));
				this.spinnerOpacityAnimation.To = 0;

				if (this.Services.Settings.Current.PhotoAnimationPreview)
				{
					this.photoOpacityAnimation.From = 1;
					this.photoOpacityAnimation.To = 1;
				}

				this.BeginStoryboard(this.captureStoryboard);

				if (this.Services.Settings.Current.PhotoAnimationPreview)
				{
					this.photoFadeDelayTimer.Start();

					if (this.photoFadeDelayTimer.ElapsedMilliseconds < 1500)
						await Task.Delay((int)(1500 - this.photoFadeDelayTimer.ElapsedMilliseconds), cancellationToken);

					this.photoFadeDelayTimer.Stop();
					await this.MainThread();

					this.fillColorAnimation.From = Colors.Transparent;
					this.fillColorAnimation.To = Colors.Transparent;
					this.fillColorAnimation.Duration = new(TimeSpan.FromMilliseconds(500));

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

				break;
			}
		}
	}
}