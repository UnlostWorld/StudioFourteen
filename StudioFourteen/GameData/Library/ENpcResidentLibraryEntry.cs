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

using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FontAwesome.Sharp;
using Lumina.Excel.Sheets;
using StudioFourteen.Appearance;
using StudioFourteen.Library.LibraryMenu;
using StudioFourteen.Library.Sources;
using System.Threading.Tasks;

using ENpcBase = StudioFourteen.GameData.Sheets.ENpcBase;

public class ENpcResidentLibraryEntry(SourceBase source, ENpcResident npc)
	: ExcelLibraryEntry(source, npc.RowId), ICharacterAppearance
{
	public override string Name => npc.Singular.ExtractText();

	public ImageReference? Icon
	{
		get
		{
			CustomizeData? customize = this.ENpcBase?.CustomizeData;
			if (customize == null)
				return null;

			CustomizeData d = customize.Value;
			return d.GetIcon();
		}
	}

	public ENpcBase? ENpcBase => ServiceManager.Instance.GameData.GetRow<ENpcBase>(npc.RowId);

	[LibraryMenu(IconChar.Plus, "LOC_AppearanceCreateCharacter")]
	public Task Spawn()
	{
		return ServiceManager.Instance.CharacterLifecycle.CreateAsync(this);
	}

	[LibraryMenuTarget(IconChar.UserShield, "LOC_AppearanceApplyTo")]
	public Task Apply(int objectTableIndex)
	{
		return Task.CompletedTask;
	}
}
