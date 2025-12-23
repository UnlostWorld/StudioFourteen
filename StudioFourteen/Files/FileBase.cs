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

namespace StudioFourteen.Files;

using StudioFourteen.Library;
using StudioFourteen.Tags;
using System;
using System.IO;
using System.Threading.Tasks;

[Serializable]
public abstract class FileBase
{
	public string? Title { get; set; }
	public string? Author { get; set; }
	public string? Description { get; set; }
	public string? Version { get; set; }
	public string? Base64Image { get; set; }
	public TagCollection? Tags { get; set; }

	public virtual void GetAutoTags(TagCollection tags)
	{
		tags.Add("Named");

		if (this.Author != null)
		{
			tags.Add(this.Author);
		}
	}

	public object? GetImage()
	{
		throw new NotImplementedException();
	}

	public void SetImage(byte[] binaryData)
	{
		this.Base64Image = Convert.ToBase64String(binaryData);
	}

	public virtual LibraryPreviewBase? GetPreview()
	{
		return null;
	}

	public abstract Task Execute();
}
