// © XivTools.
// Licensed under the MIT license.

namespace ScreenshotStudio.GameData.Sheets;

using ScreenshotStudio.GameData.Excel;

public class BuddyEquipsSheet : DataSheet<BuddyEquip>
{
	public static readonly DummyBuddyItem None = new(string.Empty, 0, 0, 0);

	public BuddyItem? Find(ItemSlots slot, ushort modelSet, ushort modelBase, ushort modelVariant)
	{
		if (this.Sheet == null)
			return null;

		foreach(BuddyEquip equip in this)
		{
			if (slot == ItemSlots.Head
				&& equip.Head != null
				&& equip.Head.ModelSet == modelSet
				&& equip.Head.ModelBase == modelBase
				&& equip.Head.ModelVariant == modelVariant)
			{
				return equip.Head;
			}

			if (slot == ItemSlots.Chest
				&& equip.Body != null
				&& equip.Body.ModelSet == modelSet
				&& equip.Body.ModelBase == modelBase
				&& equip.Body.ModelVariant == modelVariant)
			{
				return equip.Body;
			}

			if (slot == ItemSlots.Feet
				&& equip.Feet != null
				&& equip.Feet.ModelSet == modelSet
				&& equip.Feet.ModelBase == modelBase
				&& equip.Feet.ModelVariant == modelVariant)
			{
				return equip.Feet;
			}
		}

		return null;
	}

	public BuddyItem? Find(ItemSlots slot, int val)
	{
		if (val == 0)
			return None;

		bool isWeapon = slot == ItemSlots.MainHand || slot == ItemSlots.OffHand;
		short modelSet;
		short modelBase;
		short modelVariant;

		if (isWeapon)
		{
			modelSet = (short)val;
			modelBase = (short)(val >> 16);
			modelVariant = (short)(val >> 32);
		}
		else
		{
			modelSet = 0;
			modelBase = (short)val;
			modelVariant = (short)(val >> 16);
		}

		if (modelSet < 0 || modelBase < 0 || modelVariant < 0)
		{
			this.Log.Warning($"Invalid item value: {val}");
			return null;
		}

		return this.Find(slot, (ushort)modelSet, (ushort)modelBase, (ushort)modelVariant);
	}
}

public class BuddyItem : Item
{
	public BuddyItem()
	{
	}

	public BuddyItem(string name, ItemSlots slot, ushort modelBase, ushort modelVariant, ushort icon)
	{
		this.Name = name;
		this.Slot = slot;
		this.ModelBase = modelBase;
		this.ModelVariant = modelVariant;
		this.Icon = new(icon);

		this.Tags.Add("Chocobo");
		this.Tags.Add(slot.ToString());
	}

	public ItemSlots Slot { get; private set; }

	public override bool FitsInSlot(ItemSlots slot)
	{
		return slot == this.Slot;
	}
}

public class DummyBuddyItem : BuddyItem
{
	public DummyBuddyItem(string name, ushort modelBase, ushort modelVariant, ushort icon)
	{
		this.Name = name;
		this.ModelBase = modelBase;
		this.ModelVariant = modelVariant;
		this.Icon = new(icon);

		this.Tags.Add("Chocobo");
	}

	public override bool FitsInSlot(ItemSlots slot)
	{
		return true;
	}
}