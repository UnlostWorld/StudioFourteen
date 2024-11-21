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

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class SceneFileTypeInfo : JsonFileTypeInfoBase<SceneFile>
{
	public override string Extension => ".studio";
	public override string TypeName => "Studio Scene";
}

[Serializable]
public class SceneFile : FileBase
{
	public string Guid { get; set; } = System.Guid.NewGuid().ToString();

	// TODO
	////public string Location { get; set; }
	////public string TimeOfDay { get; set; }
	////public string Weather { get; set; }
	public List<Actor> Actors { get; set; } = new();

	public Task Apply() => ServiceManager.Instance.Save.OpenAsync(this);
	public Task Revert() => ServiceManager.Instance.Save.Revert(this);

	public class Actor
	{
		public string? Role { get; set; }
		public PoseFile? Pose { get; set; }
		public AppearanceFile? Appearance { get; set; }

		public async Task Apply(int objectTableIndex)
		{
			if (this.Pose != null)
			{
				await this.Pose.Apply(objectTableIndex);
			}

			if (this.Appearance != null)
			{
				await this.Appearance.Apply(objectTableIndex);
			}
		}
	}
}
