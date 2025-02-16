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

using PropertyChanged.SourceGenerator;
using StudioFourteen.Panels;
using StudioFourteen.Settings;
using System.Windows;

using static StudioFourteen.Photos.PhotosService;

public partial class PhotoWindow : Panel
{
	public bool HideUI
	{
		get => this.Persistence.GetPersistence<bool>();
		set
		{
			this.Persistence.SetPersistence(value);
			this.Services.Photos.IsPhotoMode = this.HideUI;
		}
	}

	public bool IsPortrait
	{
		get => this.Persistence.GetPersistence<bool>();
		set
		{
			this.Persistence.SetPersistence(value);
			this.Services.Photos.IsPortrait = this.IsPortrait;
		}
	}

	public PhotosService.Guides Guide
	{
		get => this.Persistence.GetPersistence<PhotosService.Guides>();
		set
		{
			this.Persistence.SetPersistence(value);
			this.Services.Photos.Guide = value;
		}
	}

	public int GuideIndex
	{
		get => (int)this.Guide;
		set => this.Guide = (Guides)value;
	}

	public double AspectRatio
	{
		get => this.Persistence.GetPersistence<double>();
		set
		{
			this.Persistence.SetPersistence(value);
			this.Services.Photos.AspectRatio = value;
		}
	}

	public AspectRatioEntry SelectedAspectRatio
	{
		get
		{
			foreach (AspectRatioEntry entry in this.Services.Photos.AspectRatios)
			{
				if (entry.Aspect == this.AspectRatio)
				{
					return entry;
				}
			}

			return new AspectRatioEntry("Unknown", this.AspectRatio);
		}

		set
		{
			this.AspectRatio = value.Aspect;
		}
	}

	protected override void OnOpened()
	{
		base.OnOpened();

		this.Services.Photos.IsPhotoMode = this.HideUI;
		this.Services.Photos.AspectRatio = this.AspectRatio;
		this.Services.Photos.Guide = this.Guide;
		this.Services.Photos.IsPortrait = this.IsPortrait;
	}

	protected override void OnClosed()
	{
		base.OnClosed();
		this.Services.Photos.IsPhotoMode = false;
		this.Services.Photos.AspectRatio = 0;
		this.Services.Photos.Guide = Guides.None;
		this.Services.Photos.IsPortrait = false;
	}

	private void OnSaveClicked(object sender, RoutedEventArgs e)
	{
		this.Services.Photos.Capture();
	}

	private void OnSettingsClicked(object sender, RoutedEventArgs e)
	{
		SettingsPanel.Show("PhotosSection");
	}

	private void OnCloseClicked(object sender, RoutedEventArgs e)
	{
		this.Close();
	}
}
