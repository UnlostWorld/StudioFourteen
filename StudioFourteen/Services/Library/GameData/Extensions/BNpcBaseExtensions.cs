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

using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using StudioFourteen;
using System.Text;

using StudioBNpcCustomize = StudioFourteen.Services.Library.GameData.Sheets.BNpcCustomize;

public static class BNpcBaseExtensions
{
	public static string GetAppearanceHash(this BNpcBase npc)
	{
		StringBuilder sb = new();
		StudioBNpcCustomize? customize = Studio.DataManager.GetRow<StudioBNpcCustomize>(npc.BNpcCustomize.RowId);

		if (customize != null)
			customize.Value.Data.GetHash(ref sb);

		if (npc.NpcEquip.IsValid)
			npc.NpcEquip.Value.GetHash(ref sb);

		return sb.ToString();
	}
}
