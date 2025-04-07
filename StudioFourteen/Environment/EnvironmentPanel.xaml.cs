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

namespace StudioFourteen.Environment;

using Lumina.Excel.Sheets;
using StudioFourteen.GameData.Library;
using StudioFourteen.Panels;
using StudioFourteen.Services;
using StudioFourteen.Tags;
using System.ComponentModel;

public partial class EnvironmentPanel : Panel
{
	public EnvironmentPanel()
	{
		this.Services.Environment.PropertyChanged += this.OnEnvironmentServicePropertyChanged;
		this.OnTerritoryChanged();
	}

	public TerritoryTypeLibraryEntry? Territory
	{
		get
		{
			TerritoryType? current = this.Services.Environment.CurrentTerritory;

			if (current == null)
				return null;

			return this.Services.GameData.GetLibraryEntry<TerritoryTypeLibraryEntry>(current.Value.RowId);
		}

		set
		{
			if (value == null)
				return;

			this.Services.Environment.ChangeTerritory(value.Territory);
		}
	}

	public WeatherLibraryEntry? Weather
	{
		get
		{
			Weather? current = this.Services.Environment.CurrentWeather;

			if (current == null)
				return null;

			return this.Services.GameData.GetLibraryEntry<WeatherLibraryEntry>(current.Value.RowId);
		}

		set
		{
			if (value == null)
				return;

			this.Services.Environment.ChangeWeather(value.Excel);
		}
	}

	public TagCollection WeatherTags { get; init; } = new();

	private void OnEnvironmentServicePropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName == nameof(EnvironmentService.CurrentTerritory))
		{
			this.NotifyPropertyChanged(nameof(this.Territory));
			this.OnTerritoryChanged();
		}

		if (e.PropertyName == nameof(EnvironmentService.CurrentWeather))
			this.NotifyPropertyChanged(nameof(this.Weather));
	}

	private void OnTerritoryChanged()
	{
		this.WeatherTags.Clear();

		if (this.Territory != null)
		{
			this.WeatherTags.Add(this.Territory.Tag);
		}
	}
}
