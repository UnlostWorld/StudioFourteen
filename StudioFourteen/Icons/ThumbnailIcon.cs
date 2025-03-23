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

namespace StudioFourteen.Icons;

using System.ComponentModel;
using System.IO;

public partial class ThumbnailIcon(FileInfo fileInfo, object? fallback)
	: INotifyPropertyChanged
{
	private string? path;
	private bool hasGenerated;

	public event PropertyChangedEventHandler? PropertyChanged;

	public object? Fallback { get; set; } = fallback;
	public bool HasImage { get; private set; } = false;

	public string? Path
	{
		get
		{
			if (!this.hasGenerated)
			{
				this.hasGenerated = true;
				ServiceManager.Instance.Thumbnails.GetThumbnail(fileInfo, this.OnThumbnailGenerated);
			}

			return this.path;
		}
	}

	private void OnThumbnailGenerated(string path)
	{
		this.path = path;
		this.HasImage = true;
		this.PropertyChanged?.Invoke(this, new(nameof(ThumbnailIcon.Path)));
		this.PropertyChanged?.Invoke(this, new(nameof(ThumbnailIcon.HasImage)));
	}
}