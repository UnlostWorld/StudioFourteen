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
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using WpfUtils.Extensions;

using static StudioFourteen.Photos.PhotosService;
using Panel = StudioFourteen.Panels.Panel;

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

	public int AspectRatioIndex
	{
		get => this.Persistence.GetPersistence<int>();
		set => this.Persistence.SetPersistence(value);
	}

	public AspectRatioEntry SelectedAspectRatio
	{
		get => this.Services.Photos.AspectRatios[this.AspectRatioIndex];

		set
		{
			this.AspectRatioIndex = this.Services.Photos.AspectRatios.IndexOf(value);
			this.Services.Photos.AspectRatio = value.Aspect;
			this.Services.Photos.Width = this.SelectedAspectRatio.Width;
			this.Services.Photos.Height = this.SelectedAspectRatio.Height;

			this.NotifyPropertyChanged(nameof(this.SelectedAspectRatio));
			this.ResolutionToggle.IsChecked = false;
		}
	}

	protected override void OnOpened()
	{
		base.OnOpened();

		this.Services.Photos.IsPhotoMode = this.HideUI;
		this.Services.Photos.AspectRatio = this.SelectedAspectRatio.Aspect;
		this.Services.Photos.Width = this.SelectedAspectRatio.Width;
		this.Services.Photos.Height = this.SelectedAspectRatio.Height;
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

	private void OnAspectsExpanderExpanded(object sender, RoutedEventArgs e)
	{
		if (sender is Expander expander)
		{
			List<Expander> expanders = this.AspectsList.FindChildren<Expander>();
			foreach (Expander otherExpander in expanders)
			{
				if (otherExpander == expander)
					continue;

				otherExpander.IsExpanded = false;
			}
		}
	}
}
