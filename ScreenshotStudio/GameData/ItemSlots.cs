// © XivTools.
// Licensed under the MIT license.

namespace ScreenshotStudio.GameData;

using ScreenshotStudio.Tags;

public enum ItemSlots
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

public static class ItemSlotsExtensions
{
	public static string GetDisplayName(this ItemSlots self)
	{
		string? localized = Resources.Find($"ItemSlots_{self}") as string;
		return localized ?? self.ToString();
	}

	public static ItemSlotTag ToTag(this ItemSlots self) => new (self);
}

public class ItemSlotTag : Tag
{
	private readonly ItemSlots slot;

	public ItemSlotTag(ItemSlots slot)
		: base(slot.ToString())
	{
		this.slot = slot;
	}
}