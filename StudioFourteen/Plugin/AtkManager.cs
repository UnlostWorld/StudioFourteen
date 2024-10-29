namespace StudioFourteen.Plugin;

using FFXIVClientStructs.FFXIV.Component.GUI;

public static class AtkManager
{
	public static unsafe bool HasActiveWindow()
	{
		var atkStage = AtkStage.Instance();
		if (atkStage == null)
			return false;

		var unitMgr = atkStage->RaptureAtkUnitManager;
		if (unitMgr == null)
			return false;

		ushort focusedUnits = unitMgr->FocusedUnitsList.Count;
		return focusedUnits != 0;
	}
}
