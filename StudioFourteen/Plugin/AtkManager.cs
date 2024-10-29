namespace StudioFourteen.Plugin;

using FFXIVClientStructs.FFXIV.Component.GUI;
using FFXIVClientStructs.FFXIV.Client.UI;

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

	public static unsafe AtkUnitList? GetWindows()
	{
		AtkStage* atkStage = AtkStage.Instance();
		if (atkStage == null)
			return null;

		RaptureAtkUnitManager* unitManager = atkStage->RaptureAtkUnitManager;
		if (unitManager == null)
			return null;

		return unitManager->AllLoadedUnitsList;
	}
}
