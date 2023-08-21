// © XivTools.
// Licensed under the MIT license.

namespace ScreenshotStudio.GameData;

using Lumina.Data;
using Lumina.Excel;

public class EquipSlotCategorySheet : DataSheet<EquipSlotCategory>
{
}

public enum EquipSlots
{
	MainHand,
	OffHand,

	Head,
	Chest,
	Hands,
	Waist,
	Legs,
	Feet,

	Earring,
	Necklace,
	Bracelet,
	RingLeft,
	RingRight,

	SoulCrystal,

	Count,
}

public class EquipSlotCategory : Lumina.Excel.GeneratedSheets.EquipSlotCategory
{
	private readonly bool[] slots = new bool[(int)EquipSlots.Count];

	public override void PopulateData(RowParser parser, Lumina.GameData gameData, Language language)
	{
		base.PopulateData(parser, gameData, language);

		for (var i = 0; i < (int)EquipSlots.Count; i++)
		{
			this.slots[i] = parser.ReadColumn<sbyte>(i) != 0;
		}
	}

	public bool IsEquippable(EquipSlots slot)
	{
		if (slot == EquipSlots.MainHand && this.slots[(int)EquipSlots.OffHand])
			return true;

		if (slot == EquipSlots.OffHand && this.slots[(int)EquipSlots.MainHand])
			return true;

		return this.slots[(int)slot];
	}
}