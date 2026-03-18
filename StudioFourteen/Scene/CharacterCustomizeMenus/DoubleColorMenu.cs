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

using CharaMakeType = StudioFourteen.Services.Library.GameData.Sheets.CharaMakeType;

public class DoubleColorMenu : MakeMenuViewModel
{
	public DoubleColorMenu(CharaMakeType makeType, CharaMakeType.CharaMakeMenu makeMenu, CustomizeIndex customizeIndex, ToggleModes toggleMode)
		: base(makeMenu, customizeIndex, toggleMode)
	{
		CustomizeIndex rightIndex = customizeIndex;

		// surely this must came from the makeMenu, but I cant find how.
		if (customizeIndex == CustomizeIndex.EyeColor)
			rightIndex = CustomizeIndex.EyeColor2;

		if (customizeIndex == CustomizeIndex.HairColor)
			rightIndex = CustomizeIndex.HairColor2;

		this.Left = new(makeType, makeMenu, customizeIndex, toggleMode, true);
		this.Right = new(makeType, makeMenu, rightIndex, toggleMode, true);
	}

	public ColorMenu Left { get; set; }
	public ColorMenu Right { get; set; }

	public override unsafe void OnGameTick(CharacterCustomize character)
	{
		base.OnGameTick(character);

		this.Left.OnGameTick(character);
		this.Right.OnGameTick(character);
	}
}
