namespace StudioFourteen.Panels;

using Lumina.Excel.Sheets;
using StudioFourteen.Library;
using StudioFourteen.Tags;
using System.Windows;

public partial class EnvironmentPanel : Panel
{
	private void OnChangeTerritoryClicked(object sender, RoutedEventArgs e)
	{
		TagCollection defaultTags = new();
		MiniLibraryPopOut.Show<TerritoryType>(
			this,
			"Change Zone",
			defaultTags,
			null,
			(territory, isFinal) =>
			{
				if (!isFinal)
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
