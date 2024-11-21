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

namespace StudioFourteen.Panels;

using StudioFourteen.GameData.Library;
using StudioFourteen.Library;
using StudioFourteen.Tags;
using System.Windows;

public partial class EnvironmentPanel : Panel
{
	private void OnChangeTerritoryClicked(object sender, RoutedEventArgs e)
	{
		TagCollection defaultTags = new();
		MiniLibraryPopOut.Show<TerritoryTypeLibraryEntry>(
			this,
			"Change Zone",
			defaultTags,
			null,
			(territory, isFinal) =>
			{
				if (!isFinal)
					return;

				this.Services.Environment.ChangeTerritory(territory.Excel);
			});
	}

	private void OnChangeWeatherClicked(object sender, RoutedEventArgs e)
	{
		TagCollection defaultTags = new();
		MiniLibraryPopOut.Show<WeatherLibraryEntry>(
			this,
			"Change Weather",
			defaultTags,
			null,
			(weather, isFinal) =>
			{
				this.Services.Environment.ChangeWeather(weather.Excel);
			});
	}
}
