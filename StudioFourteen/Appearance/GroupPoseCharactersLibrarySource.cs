// Brio
// https://github.com/Etheirys/Brio/tree/main/Brio/Game/Actor/ActorAppearanceService.cs

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