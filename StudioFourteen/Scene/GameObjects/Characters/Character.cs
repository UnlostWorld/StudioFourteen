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

namespace StudioFourteen.Scene.GameObjects.Characters;

using FFXIVClientStructs.FFXIV.Client.Game.Character;
using StudioFourteen.Services;

using CharaMakeType = StudioFourteen.GameData.Sheets.CharaMakeType;
using XivCharacter = FFXIVClientStructs.FFXIV.Client.Game.Character.Character;

public class Character : Skeleton
{
	public Character(int objectIndex)
		: base(objectIndex)
	{
	}

	public override object? Icon => Resources.Find("ICON_Type_Character");
	public override string TypeName => Resources.Find("LOC_Type_Character", "Character");

	public unsafe XivCharacter* GetXivCharacter()
	{
		return (XivCharacter*)this.Services.GameObjects.GetXivGameObject(this.ObjectIndex);
	}

	public unsafe CharaMakeType? GetCharaMakeType()
	{
		TickService.VerifyGameTickThread();
		XivCharacter* pCharacter = this.GetXivCharacter();
		return pCharacter->DrawData.CustomizeData.GetMakeType();
	}
}