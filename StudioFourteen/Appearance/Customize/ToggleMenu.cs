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

namespace StudioFourteen.Appearance.Customize;

using Dalamud.Game.ClientState.Objects.Enums;
using FFXIVClientStructs.FFXIV.Client.Game.Character;

public class ToggleMenu : MenuViewModel
{
	public ToggleMenu(CustomizeIndex index, MenuViewModel? innerMenu)
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

	public MenuViewModel? InnerMenu { get; private set; }

	public override unsafe void OnGameTick(Character* pCharacter)
	{
		base.OnGameTick(pCharacter);
		this.InnerMenu?.OnGameTick(pCharacter);
	}

	protected override void OnValueChanged(byte oldValue, byte newValue)
	{
		base.OnValueChanged(oldValue, newValue);
		this.RaisePropertyChanged(nameof(this.IsChecked));
	}
}
