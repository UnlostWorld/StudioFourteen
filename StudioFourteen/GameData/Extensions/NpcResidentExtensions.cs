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

namespace Lumina.Excel.Sheets;

using FFXIVClientStructs.FFXIV.Client.Game.Character;
using StudioFourteen;
using System.Text;

using StudioENpcBase = StudioFourteen.GameData.Sheets.ENpcBase;

public static class NpcResidentExtensions
{
	public static string GetAppearanceHash(this ENpcResident npc)
	{
		StringBuilder sb = new();

		StudioENpcBase? eNpcBase = ServiceManager.Instance.GameData.GetRow<StudioENpcBase>(npc.RowId);
		CustomizeData? customizeData = eNpcBase?.CustomizeData;

		if (customizeData != null)
			customizeData.Value.GetHash(ref sb);

		if (eNpcBase != null && eNpcBase.Value.NpcEquip.IsValid)
			eNpcBase.Value.GetAppearanceHash(ref sb);

		return sb.ToString();
	}
}