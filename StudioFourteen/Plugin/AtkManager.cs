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

	public static unsafe void SetUnitVisibility(string name, bool visible)
	{
		if (DalamudServices.GameGui == null)
			return;

		AtkUnitBase* addon = (AtkUnitBase*)DalamudServices.GameGui.GetAddonByName(name);
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
