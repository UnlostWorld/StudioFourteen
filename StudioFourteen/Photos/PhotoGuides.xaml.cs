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
using PropertyChanged.SourceGenerator;
using System.ComponentModel;
using System.Windows;

public partial class PhotoGuides : View
{
	[Notify] private double aspectBoxHeight = 0;
	[Notify] private double aspectBoxWidth = 0;
	[Notify] private double borderHalfWidth = 0;
	[Notify] private double borderHalfHeight = 0;

	public PhotoGuides()
	{
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
		if (e.PropertyName == nameof(PhotosService.AspectRatio))
		{
			this.CalculateAspectBox();
		}
	}

	private void CalculateAspectBox()
	{
		double aspect = this.Services.Photos.AspectRatio;

		double boxHeight = this.ActualHeight;
		double boxWidth = this.ActualWidth;

		if (aspect > 0)
		{
			boxHeight = this.ActualHeight;
			boxWidth = aspect * this.ActualHeight;

			if (boxWidth > this.ActualWidth)
			{
				double scale = this.ActualWidth / boxWidth;

				boxHeight *= scale;
				boxWidth *= scale;
			}
		}

		this.AspectBoxHeight = boxHeight;
		this.AspectBoxWidth = boxWidth;
		this.BorderHalfWidth = (this.ActualWidth - boxWidth) / 2;
		this.BorderHalfHeight = (this.ActualHeight - boxHeight) / 2;
	}
}
