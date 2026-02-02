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

namespace StudioFourteen.Scene;

using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using Lumina.Excel.Sheets;
using StudioFourteen.Services.Tick;
using XivCharacter = FFXIVClientStructs.FFXIV.Client.Game.Character.Character;

public partial class CharacterCustomize(int objectIndex)
	: CharacterBase(objectIndex)
{
	// ----------------------------------------------------------------------
	// Customize Value
	// ----------------------------------------------------------------------
	public Race? GetRace() => Studio.DataManager.GetRow<Race>(this.GetCustomizeValue(CustomizeIndex.Race));
	public Tribe? GetTribe() => Studio.DataManager.GetRow<Tribe>(this.GetCustomizeValue(CustomizeIndex.Tribe));

	public unsafe byte GetCustomizeValue(CustomizeIndex index)
	{
		TickService.VerifyGameTickThread();
		XivCharacter* pCharacter = this.GetXivCharacter();
		return pCharacter->DrawData.CustomizeData.GetValue(index);
	}

	public unsafe void SetCustomizeValue(CustomizeIndex index, byte value, UpdateSource source)
	{
		TickService.VerifyGameTickThread();

		XivCharacter* pCharacter = this.GetXivCharacter();

		byte oldValue = pCharacter->DrawData.CustomizeData.GetValue(index);
		if (oldValue == value)
			return;

		if (source != UpdateSource.Restore && source != UpdateSource.Preview)
			this.BackupAppearance();

		pCharacter->DrawData.CustomizeData.SetValue(index, value);

		if (index == CustomizeIndex.Race
			|| index == CustomizeIndex.Tribe
			|| index == CustomizeIndex.ModelType
			|| index == CustomizeIndex.Gender)
		{
			Studio.Redraw.Redraw(this);
		}

		this.UpdateCustomize(null, source);
	}

	// ----------------------------------------------------------------------
	// Customize
	// ----------------------------------------------------------------------
	public unsafe void SetCustomize(CustomizeData customize, UpdateSource source)
	{
		XivCharacter* pCharacter = this.GetXivCharacter();

		if (pCharacter->DrawData.CustomizeData[(int)CustomizeIndex.Race] != customize[(int)CustomizeIndex.Race]
			|| pCharacter->DrawData.CustomizeData[(int)CustomizeIndex.Tribe] != customize[(int)CustomizeIndex.Tribe]
			|| pCharacter->DrawData.CustomizeData[(int)CustomizeIndex.ModelType] != customize[(int)CustomizeIndex.ModelType])
		{
			Studio.Redraw.Redraw(this);
		}

		this.UpdateCustomize(customize, source);
	}

	private unsafe void UpdateCustomize(CustomizeData? customize, UpdateSource source)
	{
		TickService.VerifyGameTickThread();

		XivCharacter* pCharacter = this.GetXivCharacter();

		if (source != UpdateSource.Restore)
			this.BackupAppearance();

		CustomizeData* custom = &pCharacter->DrawData.CustomizeData;

		if (customize != null)
			custom->Import(customize.Value);

		bool didLoad = ((Human*)pCharacter->DrawObject)->UpdateDrawData((byte*)custom, true);
		if (!didLoad)
		{
			Studio.Redraw.Redraw(this);
		}
	}
}
