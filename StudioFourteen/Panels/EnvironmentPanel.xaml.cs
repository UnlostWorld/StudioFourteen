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
