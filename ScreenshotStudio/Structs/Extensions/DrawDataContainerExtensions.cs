namespace FFXIVClientStructs.FFXIV.Client.Game.Character;

using global::System;
using ScreenshotStudio.Tags;

using static FFXIVClientStructs.FFXIV.Client.Game.Character.DrawDataContainer;

public static class DrawDataContainerExtensions
{
	// Equipment Slots
	public static string GetDisplayName(this DrawDataContainer.EquipmentSlot self)
	{
		// TODO
		return self.ToString();
	}

	public static Tag ToTag(this DrawDataContainer.EquipmentSlot self)
	{
		switch (self)
		{
			case EquipmentSlot.Head: return Tag.Get("Head");
			case EquipmentSlot.Body: return Tag.Get("Body").WithAlias("Chest");
			case EquipmentSlot.Hands: return Tag.Get("Hands");
			case EquipmentSlot.Legs: return Tag.Get("Legs");
			case EquipmentSlot.Feet: return Tag.Get("Feet");
			case EquipmentSlot.Ears: return Tag.Get("Ears").WithAlias("Earring");
			case EquipmentSlot.Neck: return Tag.Get("Neck").WithAlias("Necklace");
			case EquipmentSlot.Wrists: return Tag.Get("Wrists").WithAlias("Bracelets");
			case EquipmentSlot.RFinger: return Tag.Get("Right Finger").WithAlias("Right Ring");
			case EquipmentSlot.LFinger: return Tag.Get("Left Finger").WithAlias("Left Ring");
		}

		throw new Exception($"Invalid Equipment Slot: {self}");
	}

	// Weapon Slots
	public static string GetDisplayName(this DrawDataContainer.WeaponSlot self)
	{
		// TODO
		return self.ToString();
	}

	public static Tag ToTag(this DrawDataContainer.WeaponSlot self)
	{
		switch (self)
		{
			case WeaponSlot.MainHand: return Tag.Get("Main Hand");
			case WeaponSlot.OffHand: return Tag.Get("Off Hand");
			case WeaponSlot.Unk: return Tag.Get("Prop");
		}

		throw new Exception($"Invalid Equipment Slot: {self}");
	}
}