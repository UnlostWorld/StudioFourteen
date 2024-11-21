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

namespace FFXIVClientStructs.FFXIV.Client.Game.Character;

using global::System;
using global::System.Collections.Generic;
using StudioFourteen;
using StudioFourteen.Tags;

using static FFXIVClientStructs.FFXIV.Client.Game.Character.DrawDataContainer;

public static class DrawDataContainerExtensions
{
	private static readonly Dictionary<DrawDataContainer.EquipmentSlot, string> EquipmentSlotDisplayNameCache = new();
	private static readonly Dictionary<DrawDataContainer.WeaponSlot, string> WeaponSlotDisplayNameCache = new();

	// Equipment Slots
	public static string GetDisplayName(this DrawDataContainer.EquipmentSlot self)
	{
		string? name;
		if (!EquipmentSlotDisplayNameCache.TryGetValue(self, out name))
		{
			string id = $"LOC_EquipmentSlot_{self.ToString()}";
			name = Resources.Find(id, self.ToString());
			EquipmentSlotDisplayNameCache.Add(self, name);
		}

		return name ?? self.ToString();
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
			case EquipmentSlot.RFinger: return Tag.Get("RFinger").WithAlias("RRing");
			case EquipmentSlot.LFinger: return Tag.Get("LFinger").WithAlias("LRing");
		}

		throw new Exception($"Invalid Equipment Slot: {self}");
	}

	// Weapon Slots
	public static string GetDisplayName(this DrawDataContainer.WeaponSlot self)
	{
		string? name;
		if (!WeaponSlotDisplayNameCache.TryGetValue(self, out name))
		{
			string id = $"LOC_WeaponSlot_{self.ToString()}";
			name = Resources.Find(id, self.ToString());
			WeaponSlotDisplayNameCache.Add(self, name);
		}

		return name ?? self.ToString();
	}

	public static Tag ToTag(this DrawDataContainer.WeaponSlot self)
	{
		switch (self)
		{
			case WeaponSlot.MainHand: return Tag.Get("MainHand");
			case WeaponSlot.OffHand: return Tag.Get("OffHand");
			case WeaponSlot.Unk: return Tag.Get("Prop");
		}

		throw new Exception($"Invalid Equipment Slot: {self}");
	}
}