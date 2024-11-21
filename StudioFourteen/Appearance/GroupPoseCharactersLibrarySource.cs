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

namespace StudioFourteen.Appearance;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using StudioFourteen;
using StudioFourteen.Library.Sources;
using StudioFourteen.Plugin;
using StudioFourteen.Services;
using StudioFourteen.Utilities;
using System.Threading.Tasks;

public class GroupPoseCharactersLibrarySource : SourceBase
{
	public override string Name => Resources.Find("LOC_Library_GroupPoseCharactersLibrarySource", "GPose Characters");

	public void OnEnterGroupPose()
	{
		this.Clear();

		Task.Run(async () =>
		{
			await Task.Delay(1500);
			await Threads.RunOnFrameworkThread(() => this.BackupAll());
		});
	}

	protected override void Scan()
	{
	}

	protected override string GetInternalId() => "CurrentCharactersLibraryProvider";

	private unsafe void BackupAll()
	{
		// back up the appearance of every character in gpose
		for (int i = GroupPoseService.GPoseFirstCharacter; i < GroupPoseService.GPoseFirstCharacter + GroupPoseService.GPoseCharacterCount; ++i)
		{
			nint? address = DalamudServices.ObjectTable?.GetObjectAddress(i);
			if (address == null || address == nint.Zero)
				continue;

			Character* character = (Character*)address;

			CharacterBackupAppearance appearance = new(character);
			this.Add(appearance);
		}
	}
}