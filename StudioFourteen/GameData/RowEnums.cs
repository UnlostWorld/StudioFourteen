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

namespace StudioFourteen.GameData;

using StudioFourteen.Tags;

public enum RaceRows : byte
{
	Hyur = 1,
	Elezen = 2,
	Lalafell = 3,
	Miqote = 4,
	Roegadyn = 5,
	AuRa = 6,
	Hrothgar = 7,
	Viera = 8,

	Count,
}

public enum Genders : sbyte
{
	Masculine,
	Feminine,
}

public enum ModelTypes : byte
{
	Normal = 1,
	Old = 3,
	Young = 4,
}

public enum TribeRows : byte
{
	Midlander = 1,
	Highlander = 2,
	Wildwood = 3,
	Duskwight = 4,
	Plainsfolk = 5,
	Dunesfolk = 6,
	SeekerOfTheSun = 7,
	KeeperOfTheMoon = 8,
	SeaWolf = 9,
	Hellsguard = 10,
	Raen = 11,
	Xaela = 12,
	Helions = 13,
	TheLost = 14,
	Rava = 15,
	Veena = 16,
}

public static class RowEnumExtensions
{
	public static TagCollection ToTags(this Genders self)
	{
		TagCollection tags = new();
		tags.Add(self.ToString());
		return tags;
	}
}