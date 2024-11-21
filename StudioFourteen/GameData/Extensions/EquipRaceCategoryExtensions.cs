// .                      @@             _____ _______ _    _ _____ _____ ____
//            @       @@@@@             / ____|__   __| |  | |  __ \_   _/ __ \
//           @@@  @@@@                 | (___    | |  | |  | | |  | || || |  | |
//           @@@@@@@@@  @    @          \___ \   | |  | |  | | |  | || || |  | |
//          @@@@       @@@@@@@          ____) |  | |  | |__| | |__| || || |__| |
//      @@@@@             @@@          |_____/   |_|   \____/|_____/_____\____/
//       @@@      @@@      @@        ___     _    _   _  __   _____  ___  ___  _  _
//        @@    @@@@@@@    @@       |  _|  / _ \ | | | || _ \|_   _|| __|| __|| \| |
//        @@    @@@@@@@    @   @    | __| | (_) || |_| ||   /  | |  | _| | _| | .` |
//      @@@@      @@@      @@@@     |_|    \___/  \___/ |_|_\  |_|  |___||___||_|\_|
//       @@@@             @@@
//         @@@@@      @@@@@               This software is licensed under the
//          @@@@@@@@@@@@@@                 GNU AFFERO GENERAL PUBLIC LICENSE
//              @@@@  @                       Version 3, 19 November 2007

namespace StudioFourteen.GameData.Extensions;

using Lumina.Excel;
using Lumina.Excel.Sheets;
using StudioFourteen.Tags;

public static class EquipRaceCategoryExtensions
{
	public static TagCollection ToTags(this EquipRaceCategory self)
	{
		TagCollection tags = new();

		if (self.Male)
			tags.Add("Masculine");

		if (self.Female)
			tags.Add("Feminine");

		ExcelSheet<Race>? raceSheet = ServiceManager.Instance.GameData.GetSheet<Race>();
		if (raceSheet == null)
			return tags;

		if (self.Hyur)
			tags.Add(raceSheet.GetRow(RaceRows.Hyur).GetName() ?? "Hyur");

		if (self.Elezen)
			tags.Add(raceSheet.GetRow(RaceRows.Elezen).GetName() ?? "Elezen");

		if (self.Lalafell)
			tags.Add(raceSheet.GetRow(RaceRows.Lalafell).GetName() ?? "Lalafell");

		if (self.Miqote)
			tags.Add(raceSheet.GetRow(RaceRows.Miqote).GetName() ?? "Miqote");

		if (self.Roegadyn)
			tags.Add(raceSheet.GetRow(RaceRows.Roegadyn).GetName() ?? "Roegadyn");

		if (self.AuRa)
			tags.Add(raceSheet.GetRow(RaceRows.AuRa).GetName() ?? "AuRa");

		if (self.Unknown0)
			tags.Add(raceSheet.GetRow(RaceRows.Hrothgar).GetName() ?? "Hrothgar");

		if (self.Unknown1)
			tags.Add(raceSheet.GetRow(RaceRows.Viera).GetName() ?? "Viera");

		return tags;
	}
}
