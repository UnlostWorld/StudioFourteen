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

namespace StudioFourteen.Scene.CharacterCustomizeMenus;

using Dalamud.Game.ClientState.Objects.Enums;

public class ToggleMenu : CharacterCustomizeMenu
{
	public ToggleMenu(CustomizeIndex index, CharacterCustomizeMenu? innerMenu)
		: base(index, ToggleModes.IsToggle)
	{
		if (innerMenu != null)
		{
			this.InnerMenu = innerMenu;
			this.Name = innerMenu.Name;
			innerMenu.Name = null;
		}
	}

	public bool IsChecked
	{
		get => this.Value == 1;
		set => this.Value = (byte)(value ? 1 : 0);
	}

	public CharacterCustomizeMenu? InnerMenu { get; private set; }

	public override unsafe void OnGameTick(CharacterCustomize character)
	{
		base.OnGameTick(character);
		this.InnerMenu?.OnGameTick(character);
	}

	protected override void OnValueChanged(byte oldValue, byte newValue)
	{
		base.OnValueChanged(oldValue, newValue);
		this.OnPropertyChanged(nameof(this.IsChecked));
	}
}
