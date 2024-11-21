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
using StudioFourteen.Library;
using StudioFourteen.Library.LibraryMenu;
using StudioFourteen.Library.Sources;
using StudioFourteen.Plugin;
using StudioFourteen.Utilities;
using System.Threading.Tasks;
using static FFXIVClientStructs.FFXIV.Client.Game.Character.CharacterExtensions;
using BNpcCustomize = StudioFourteen.GameData.Sheets.BNpcCustomize;

public class BNpcBaseLibraryEntry(SourceBase source, BNpcBase npc)
	: ExcelLibraryEntry(source, npc.RowId), ICharacterAppearance
{
	public override string Name
	{
		get
		{
			if (GameDataService.BattleNpcNameIndex.TryGetValue($"{npc.RowId}", out int nameRowId))
			{
				BNpcName? name = ServiceManager.Instance.GameData.GetRow<BNpcName>(nameRowId);
				if (name != null)
				{
					return name.Value.Singular.ExtractText();
				}
			}

			return $"#{npc.RowId}";
		}
	}

	public ImageReference? Icon
	{
		get
		{
			CustomizeData? customize = this.Customize;
			if (customize == null)
				return null;

			CustomizeData d = customize.Value;
			return d.GetIcon();
		}
	}

	public CustomizeData? Customize
	{
		get
		{
			BNpcCustomize? customize = ServiceManager.Instance.GameData.GetRow<BNpcCustomize>(npc.BNpcCustomize.RowId);
			return customize?.Data;
		}
	}

	[LibraryMenu(IconChar.Plus, "LOC_AppearanceCreateCharacter")]
	public Task Spawn()
	{
		return ServiceManager.Instance.CharacterLifecycle.CreateAsync(this);
	}

	[LibraryMenuTarget(IconChar.UserShield, "LOC_AppearanceApplyTo")]
	public async Task Apply(int objectTableIndex)
	{
		await Threads.FrameworkThread();

		this.Services.CharacterAppearance.SetModelCharaId(objectTableIndex, npc.ModelChara.Value, UpdateSource.Library);

		if (this.Customize != null)
			this.Services.CharacterAppearance.SetCustomize(objectTableIndex, this.Customize.Value, UpdateSource.Library);

		if (npc.NpcEquip.IsValid)
		{
			// TODO: apply equip.
		}
	}
}
