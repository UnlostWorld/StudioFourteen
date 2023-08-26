// © XivTools.
// Licensed under the MIT license.

namespace ScreenshotStudio.GameData.Excel;

using Lumina.Data;
using Lumina.Excel;
using ScreenshotStudio.Tags;

public class EquipSlotCategory : Lumina.Excel.GeneratedSheets.EquipSlotCategory
{
	private readonly bool[] slots = new bool[(int)ItemSlots.Count];

	public TagCollection Tags { get; init; } = new();

	public override void PopulateData(RowParser parser, Lumina.GameData gameData, Language language)
	{
		base.PopulateData(parser, gameData, language);

		for (var i = 0; i < (int)ItemSlots.Count; i++)
		{
			this.slots[i] = parser.ReadColumn<sbyte>(i) != 0;
		}
	}

	public bool Contains(ItemSlots slot)
	{
		if (slot == ItemSlots.MainHand && this.slots[(int)ItemSlots.OffHand])
			return true;

		if (slot == ItemSlots.OffHand && this.slots[(int)ItemSlots.MainHand])
			return true;

		// >=(
		if (slot == ItemSlots.Count)
			return false;

		return this.slots[(int)slot];
	}

	public TagCollection ToTags()
	{
		TagCollection tags = new();

		for (var i = 0; i < (int)ItemSlots.Count; i++)
		{
			ItemSlots slot = (ItemSlots)i;
			if (this.Contains(slot))
			{
				tags.Add(slot.ToTag());
			}
		}

		return tags;
	}
}