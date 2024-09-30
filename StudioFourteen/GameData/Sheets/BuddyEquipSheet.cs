namespace StudioFourteen.GameData.Sheets;

using StudioFourteen.GameData.Excel;
using static FFXIVClientStructs.FFXIV.Client.Game.Character.DrawDataContainer;

public class BuddyEquipsSheet : DataSheet<BuddyEquip>
{
	public static readonly DummyBuddyItem None = new(string.Empty, 0, 0, 0);

	public BuddyItem? Find(EquipmentSlot slot, ushort modelSet, ushort modelBase, ushort modelVariant)
	{
		if (this.Sheet == null)
			return null;

		foreach(BuddyEquip equip in this)
		{
			if (slot == EquipmentSlot.Head
				&& equip.Head != null
				&& equip.Head.ModelSet == modelSet
				&& equip.Head.ModelBase == modelBase
				&& equip.Head.ModelVariant == modelVariant)
			{
				return equip.Head;
			}

			if (slot == EquipmentSlot.Body
				&& equip.Body != null
				&& equip.Body.ModelSet == modelSet
				&& equip.Body.ModelBase == modelBase
				&& equip.Body.ModelVariant == modelVariant)
			{
				return equip.Body;
			}

			if (slot == EquipmentSlot.Feet
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

	public BuddyItem? Find(WeaponSlot slot, int val)
	{
		return null;
	}

	public BuddyItem? Find(EquipmentSlot slot, int val)
	{
		if (val == 0)
			return None;

		short modelSet = 0;
		short modelBase = (short)val;
		short modelVariant = (short)(val >> 16);

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

	public BuddyItem(string name, EquipmentSlot slot, ushort modelBase, ushort modelVariant, ushort icon)
	{
		this.Name = name;
		this.Slot = slot;
		this.ModelBase = modelBase;
		this.ModelVariant = modelVariant;
		this.Icon = new(icon);

		this.Tags.Add("Chocobo");
		this.Tags.Add(slot.ToString());
	}

	public EquipmentSlot Slot { get; private set; }

	public override bool FitsInSlot(EquipmentSlot slot)
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

	public override bool FitsInSlot(EquipmentSlot slot)
	{
		return true;
	}

	public override bool FitsInSlot(WeaponSlot slot)
	{
		return false;
	}
}