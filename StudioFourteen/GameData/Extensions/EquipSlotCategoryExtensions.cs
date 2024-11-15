namespace StudioFourteen.GameData.Extensions;

using Lumina.Excel.Sheets;

using static FFXIVClientStructs.FFXIV.Client.Game.Character.DrawDataContainer;

public static class EquipSlotCategoryExtensions
{
	public static bool Contains(this EquipSlotCategory self, WeaponSlot slot)
	{
		switch (slot)
		{
			case WeaponSlot.MainHand: return self.MainHand == 1;
			case WeaponSlot.OffHand: return self.OffHand == 1;
		}

		return false;
	}

	public static bool Contains(this EquipSlotCategory self, EquipmentSlot slot)
	{
		switch (slot)
		{
			case EquipmentSlot.Head: return self.Head == 1;
			case EquipmentSlot.Body: return self.Body == 1;
			case EquipmentSlot.Hands: return self.Gloves == 1;
			case EquipmentSlot.Legs: return self.Legs == 1;
			case EquipmentSlot.Feet: return self.Feet == 1;
			case EquipmentSlot.Ears: return self.Ears == 1;
			case EquipmentSlot.Neck: return self.Neck == 1;
			case EquipmentSlot.Wrists: return self.Wrists == 1;
			case EquipmentSlot.RFinger: return self.FingerR == 1;
			case EquipmentSlot.LFinger: return self.FingerL == 1;
		}

		return false;
	}
}
