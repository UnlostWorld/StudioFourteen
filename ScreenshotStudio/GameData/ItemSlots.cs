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

	public static Tag ToTag(this ItemSlots self)
	{
		return Tag.Get(self.ToString());
	}
}