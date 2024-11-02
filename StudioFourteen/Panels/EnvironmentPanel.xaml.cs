namespace StudioFourteen.Panels;

using StudioFourteen.GameData.Excel;
using StudioFourteen.Library;
using StudioFourteen.Plugin;
using StudioFourteen.Tags;
using System.Windows;

public partial class EnvironmentPanel : Panel
{
	private void OnChangeTerritoryClicked(object sender, RoutedEventArgs e)
	{
		TagCollection defaultTags = new();
		MiniLibraryPopOut.Show<Territory>(
			this,
			"Change Zone",
			defaultTags,
			null,
			(territory, isFinal) =>
			{
				if (!isFinal || territory == null || territory.Background == null)
					return;

				this.Services.Environment.ChangeTerritory(territory);
			});
	}

	private void OnChangeWeatherClicked(object sender, RoutedEventArgs e)
	{
		TagCollection defaultTags = new();
		MiniLibraryPopOut.Show<Weather>(
			this,
			"Change Weather",
			defaultTags,
			null,
			(weather, isFinal) =>
			{
				this.Services.Environment.ChangeWeather(weather);
			});
	}
}
