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

namespace StudioFourteen.Scripting.Instance;

using System.Threading.Tasks;
using Dalamud.Game.ClientState.Objects.Enums;
using StudioFourteen.GameData;
using StudioFourteen.GameData.Library;
using StudioFourteen.Utilities;

using static FFXIVClientStructs.FFXIV.Client.Game.Character.CharacterExtensions;

public class CharacterInterface : ScriptServiceBase
{
	public Genders[] Genders => [GameData.Genders.Feminine, GameData.Genders.Masculine];

	public CharacterReference GetCurrentTarget() => new(this.Services.Target.TargetObjectIndex);
}

public class CharacterReference(int objectTableIndex)
{
	public readonly int ObjectTableIndex = objectTableIndex;

	private ServiceManager Services => ServiceManager.Instance;

	public async Task RedrawAsync(RaceLibraryEntry? race = null, TribeLibraryEntry? tribe = null, Genders? gender = null)
	{
		await Threads.FrameworkThread();

		if (race != null)
			this.Services.CharacterAppearance.SetCustomizeValue(this.ObjectTableIndex, CustomizeIndex.Race, (byte)race.RowId, UpdateSource.Script);

		if (tribe != null)
			this.Services.CharacterAppearance.SetCustomizeValue(this.ObjectTableIndex, CustomizeIndex.Tribe, (byte)tribe.RowId, UpdateSource.Script);

		if (gender != null)
			this.Services.CharacterAppearance.SetCustomizeValue(this.ObjectTableIndex, CustomizeIndex.Gender, (byte)gender, UpdateSource.Script);

		await this.Services.Redraw.RedrawAsync(this.ObjectTableIndex, true);
	}

	public Task RestoreAsync() => this.Services.CharacterAppearance.Restore(this.ObjectTableIndex);
}