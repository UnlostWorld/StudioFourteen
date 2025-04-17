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

using System.Windows;
using StudioFourteen.GameData.Library;
using StudioFourteen.Library;
using StudioFourteen.Panels;
using StudioFourteen.Tags;
using WpfUtils.Extensions;

public partial class EnvironmentPanel : Panel
{
	public TagCollection WeatherTags { get; init; } = new();

	protected override void OnOpened()
	{
		this.Services.Territory.TerritoryChanged += this.OnTerritoryChanged;
		this.OnTerritoryChanged(this.Services.Territory.CurrentTerritory);
		base.OnOpened();
	}

	protected override void OnClosed()
	{
		this.Services.Territory.TerritoryChanged -= this.OnTerritoryChanged;
		base.OnClosed();
	}

	private void OnTerritoryChanged(TerritoryTypeLibraryEntry? territory)
	{
		this.WeatherTags.Clear();

		if (territory != null)
		{
			this.WeatherTags.Add(territory.Tag);
		}
	}

	private void OnImportClicked(object sender, RoutedEventArgs e)
	{
		LibraryWindow.Open(this.GetContext());
	}

	private void OnExportClicked(object sender, RoutedEventArgs e)
	{
		this.Services.Environment.Export().Run();
	}
}
