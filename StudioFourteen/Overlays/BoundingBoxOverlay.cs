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

namespace StudioFourteen.Overlays;

using FFXIVClientStructs.FFXIV.Client.Game.Character;
using System.Numerics;

public class BoundingBoxOverlay(string group, string name)
	: BoxOverlay(group, name)
{
	public unsafe void Update(Character* pCharacter)
	{
		Vector3 position = pCharacter->DrawObject->Position;
		position.Y += pCharacter->Height;
		this.Position = position;

		this.Rotation = pCharacter->DrawObject->Rotation;

		Vector3 scale = this.Scale;
		scale.Y = pCharacter->Height * 2;
		scale.X = pCharacter->HitboxRadius * 2;
		scale.Z = pCharacter->HitboxRadius * 2;
		this.Scale = scale;
	}
}
