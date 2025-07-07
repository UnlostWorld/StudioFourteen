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

using System.Threading.Tasks;
using StudioFourteen.Context;
using System.Collections.Generic;
using StudioFourteen.Scene.GameObjects.Characters;

public class CharacterAppearanceContextMenuProvider : ContextProvider<ICharacterAppearance>
{
	protected override Task GetMenus(ICharacterAppearance target, List<MenuEntry> menus)
	{
		menus.Add(new("ICON_AddCharacter", "LOC_Context_Spawn", () => this.Spawn(target)));
		return Task.CompletedTask;
	}

	private Task<int> Spawn(ICharacterAppearance target)
	{
		return this.Services.CharacterLifecycle.CreateAsync(target, UpdateSource.Interface);
	}

	private Task Apply(ICharacterAppearance target, Character character)
	{
		return character.ImportAppearance(target, UpdateSource.Interface);
	}
}