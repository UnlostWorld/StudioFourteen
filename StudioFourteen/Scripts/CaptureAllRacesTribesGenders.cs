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

namespace StudioFourteen.Scripts;

using System;
using System.Threading.Tasks;
using Dalamud.Game.ClientState.Objects.Enums;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using StudioFourteen.GameData;
using StudioFourteen.Utilities;

public class CaptureAllRacesTribesGenders : ScriptBase
{
	protected override async Task Run()
	{
		int objectIndex = this.Services.Target.TargetObjectIndex;

		this.SetStatus($"Starting...");

		await Threads.FrameworkThread();

		ExcelSheet<Race>? raceSheet = this.Services.GameData.GetSheet<Race>();
		if (raceSheet == null)
			return;

		string dirName = DateTime.Now.ToString("yyyy-MM-dd HH-mm");

		foreach(Race race in raceSheet)
		{
			if (race.RowId == 0)
				continue;

			this.Services.CharacterAppearance.SetCustomizeValue(objectIndex, CustomizeIndex.Race, (byte)race.RowId, CharacterExtensions.UpdateSource.Interface);

			foreach(Tribe tribe in race.GetTribes())
			{
				if (tribe.RowId == 0)
					continue;

				this.Services.CharacterAppearance.SetCustomizeValue(objectIndex, CustomizeIndex.Tribe, (byte)tribe.RowId, CharacterExtensions.UpdateSource.Interface);

				foreach(Genders gender in Enum.GetValues<Genders>())
				{
					this.Services.CharacterAppearance.SetCustomizeValue(objectIndex, CustomizeIndex.Gender, (byte)gender, CharacterExtensions.UpdateSource.Interface);

					this.SetStatus($"{race.GetName()}, {tribe.GetName()}, {gender}");

					await this.Services.Redraw.RedrawAsync(objectIndex, false);

					await Task.Delay(100);

					await this.Services.Photos.CaptureAsync($"{dirName}/{race.GetName()}-{tribe.GetName()}-{gender}", false);

					await Threads.FrameworkThread();
				}
			}
		}

		this.SetStatus($"Completed");
		await this.Services.CharacterAppearance.Restore(objectIndex);
	}
}