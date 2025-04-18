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

using StudioFourteen.Appearance;
using StudioFourteen.Cameras;
using StudioFourteen.Environment;
using StudioFourteen.Posing;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class SceneFileTypeInfo : JsonFileTypeInfoBase<SceneFile>
{
	public override string Extension => ".s14scene";
	public override string TypeName => "Scene";
}

[Serializable]
public class SceneFile : FileBase
{
	public string Guid { get; set; } = System.Guid.NewGuid().ToString();

	public EnvironmentFile Environment { get; set; } = new();
	public List<Actor> Actors { get; set; } = new();
	public List<StudioCameraBase> Cameras { get; set; } = new();

	////[LibraryMenu(IconChar.UsersBetweenLines, "LOC_OpenScene")]
	////public Task Apply() => ServiceManager.Instance.Save.OpenAsync(this);

	public override Task Execute()
	{
		throw new NotImplementedException();
	}

	public class Actor
	{
		public string? Role { get; set; }
		public PoseFile? Pose { get; set; }
		public AppearanceFile? Appearance { get; set; }

		public async Task Apply(int objectTableIndex, UpdateSource source)
		{
			if (this.Pose != null)
			{
				await this.Pose.Apply(objectTableIndex, source);
			}

			if (this.Appearance != null)
			{
				await this.Appearance.Apply(objectTableIndex, source);
			}
		}
	}
}
