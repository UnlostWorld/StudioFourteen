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

namespace StudioFourteen.Plugin;

using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;

public static class AtkManager
{
	public static unsafe bool HasActiveWindow()
	{
		AtkStage* atkStage = AtkStage.Instance();
		if (atkStage == null)
			return false;

		RaptureAtkUnitManager* unitManager = atkStage->RaptureAtkUnitManager;
		if (unitManager == null)
			return false;

		ushort focusedUnits = unitManager->FocusedUnitsList.Count;
		return focusedUnits != 0;
	}

	public static unsafe AtkUnitList? GetAllLoadedUnits()
	{
		AtkStage* atkStage = AtkStage.Instance();
		if (atkStage == null)
			return null;

		RaptureAtkUnitManager* unitManager = atkStage->RaptureAtkUnitManager;
		if (unitManager == null)
			return null;

		return unitManager->AllLoadedUnitsList;
	}

	public static unsafe void SetUnitVisibility(string name, bool visible)
	{
		if (DalamudServices.GameGui == null)
			return;

		AtkUnitBase* addon = (AtkUnitBase*)DalamudServices.GameGui.GetAddonByName(name).Address;
		if (addon == null)
			return;

		if (visible)
		{
			addon->Show(true, 0);
		}
		else
		{
			addon->Hide(true, true, 0);
		}
	}
}
