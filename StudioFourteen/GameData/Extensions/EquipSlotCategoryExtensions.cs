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

using FFXIVClientStructs.FFXIV.Client.Game.Character;
using Lumina.Excel.Sheets;
using StudioFourteen.Tags;
using System;

using static FFXIVClientStructs.FFXIV.Client.Game.Character.DrawDataContainer;

public static class EquipSlotCategoryExtensions
{
	public static TagCollection ToTags(this EquipSlotCategory self)
	{
		TagCollection tags = new();

		foreach (EquipmentSlot slot in Enum.GetValues<EquipmentSlot>())
		{
			if (self.Contains(slot))
			{
				tags.Add(slot.ToTag());
			}
		}

		foreach (WeaponSlot slot in Enum.GetValues<WeaponSlot>())
		{
			if (self.Contains(slot))
			{
				tags.Add(slot.ToTag());
			}
		}

		return tags;
	}

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
