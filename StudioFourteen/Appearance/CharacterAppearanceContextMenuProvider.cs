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
using StudioFourteen.Services;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.Object;

public class CharacterAppearanceContextMenuProvider : ContextProvider<ICharacterAppearance>
{
	protected override async Task GetMenus(ICharacterAppearance target, List<MenuEntry> menus)
	{
		menus.Add(new("ICON_AddCharacter", "LOC_Context_Spawn", () => this.Spawn(target)));

		await TickService.GameTick();

		MenuEntry applyParent = new(null, "LOC_Context_ApplyTo");

		unsafe
		{
			Character*[] pCharacters = this.Services.CharacterLifecycle.GetAllCharacters();
			foreach(Character* pCharacter in pCharacters)
			{
				int index = pCharacter->ObjectIndex;
				MenuEntry subEntry = new(null, pCharacter->GetDisplayName(), () => this.Apply(target, index));
				applyParent.AddChild(subEntry);
			}
		}

		if (applyParent.Children.Count > 0)
		{
			menus.Add(applyParent);
		}
	}

	private Task<int> Spawn(ICharacterAppearance target)
	{
		return this.Services.CharacterLifecycle.CreateAsync(target, UpdateSource.Interface);
	}

	private Task Apply(ICharacterAppearance target, int index)
	{
		return target.Apply(index, UpdateSource.Interface);
	}
}