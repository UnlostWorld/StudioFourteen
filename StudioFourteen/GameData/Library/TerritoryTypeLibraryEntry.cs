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

namespace StudioFourteen.GameData.Library;

using Lumina.Excel;
using Lumina.Excel.Sheets;
using Lumina.Text.ReadOnly;
using StudioFourteen.Library.Sources;
using StudioFourteen.Tags;
using System.Text;

public class TerritoryTypeLibraryEntry : ExcelLibraryEntry
{
	public readonly TerritoryType Territory;

	private string? thumbnailPath;
	private bool hasGeneratedThumbnail;

	public TerritoryTypeLibraryEntry(SourceBase source, TerritoryType territory)
		: base(source, territory.RowId)
	{
		this.Territory = territory;

		// Find all weathers that can naturally spawn here and tag them.
		WeatherRate? rate = this.Services.GameData.GetRow<WeatherRate>(this.Territory.WeatherRate);
		if (rate != null)
		{
			foreach(RowRef<Weather> weather in rate.Value.Weather)
			{
				if (!weather.IsValid)
					continue;

				WeatherLibraryEntry? weatherLibraryEntry = this.Services.GameData.GetLibraryEntry<WeatherLibraryEntry>(weather.RowId);
				if (weatherLibraryEntry == null)
					continue;

				weatherLibraryEntry.Tags.Add(this.Tag);
			}
		}
	}

	public override string? SubTitle => this.Territory.Name.GetString();

	public string? Place => this.Territory.PlaceName.Value.Name.GetString();
	public string? Region => this.Territory.PlaceNameRegion.Value.Name.GetString();
	public string? Zone => this.Territory.PlaceNameZone.Value.Name.GetString();

	public Tag Tag
	{
		get
		{
			string? name = this.Territory.Name.GetString();
			if (name == null)
				return Tag.Get("Unknown");

			return Tag.Get(name);
		}
	}

	public override string? Name
	{
		get
		{
			if (this.Territory.PlaceName.IsValid)
			{
				string? placeName = this.Territory.PlaceName.Value.Name.GetString();
				string? regionName = this.Territory.PlaceNameRegion.Value.Name.GetString();
				string? zoneName = this.Territory.PlaceNameZone.Value.Name.GetString();

				StringBuilder builder = new();
				builder.Append(placeName);

				if (regionName != null)
				{
					builder.Append(", ");
					builder.Append(regionName);
				}

				if (regionName != zoneName && zoneName != null)
				{
					builder.Append(", ");
					builder.Append(zoneName);
				}

				string name = builder.ToString();

				if (string.IsNullOrEmpty(name))
					return null;

				return name;
			}

			return null;
		}
	}

	public string? ThumbnailPath
	{
		get
		{
			if (!this.hasGeneratedThumbnail)
			{
				this.hasGeneratedThumbnail = true;

				if (!this.Territory.LoadingImage.IsValid)
					return null;

				LoadingImage loadingImage = this.Territory.LoadingImage.Value;

				string? fileName = loadingImage.Unknown0.GetString();
				if (fileName == null)
					return null;

				string texturePath = $"ui/loadingimage/{fileName}_hr1.tex";
				ServiceManager.Instance.Thumbnails.GetThumbnailFromTexture(texturePath, this.OnThumbnailGenerated);
			}

			return this.thumbnailPath;
		}
	}

	private void OnThumbnailGenerated(string path)
	{
		this.thumbnailPath = path;
		this.NotifyPropertyChanged(nameof(this.ThumbnailPath));
	}
}
