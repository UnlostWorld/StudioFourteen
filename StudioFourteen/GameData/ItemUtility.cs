//// Ktisis
//// https://github.com/ktisis-tools/Ktisis/
//// https://github.com/ktisis-tools/Ktisis/blob/main/Ktisis/Data/Excel/Item.cs

//// Anamnesis
//// https://github.com/imchillin/Anamnesis/blob/master/Anamnesis/Actor/Utilities/ItemUtility.cs

namespace StudioFourteen.GameData.Sheets;

using Lumina.Excel;
using Serilog;
using StudioFourteen.GameData.Excel;
using static FFXIVClientStructs.FFXIV.Client.Game.Character.DrawDataContainer;

public class ItemUtility
{
	public static readonly DummyItem None = new DummyItem(0, 0, 0, string.Empty);

	public ExcelSheet<Item>? Sheet => ServiceManager.Instance.GameData.GetSheet<Item>();
	public ILogger Log => Logging.ForContext<ItemUtility>();

	public Item? Find(EquipmentSlot slot, ushort modelSet, ushort modelBase, ushort modelVariant)
	{
		if (this.Sheet == null)
			return null;

		if (modelSet == 0 && modelBase == 0 && modelVariant == 0)
			return None;

		foreach(Item item in this.Sheet)
		{
			if (!item.FitsInSlot(slot))
				continue;

			if (item.ModelSet == modelSet
				&& item.ModelBase == modelBase
				&& item.ModelVariant == modelVariant)
			{
				return item;
			}
		}

		return new DummyItem(modelSet, modelBase, modelVariant);
	}

	public Item? Find(EquipmentSlot slot, ulong val)
	{
		if (val == 0)
			return None;

		if (val == uint.MaxValue || val == long.MaxValue || val == ulong.MaxValue)
			return null;

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

	public Item? Find(WeaponSlot slot, ushort modelSet, ushort modelBase, ushort modelVariant)
	{
		if (this.Sheet == null)
			return null;

		if (modelSet == 0 && modelBase == 0 && modelVariant == 0)
			return None;

		string lookupKey = slot + "_" + modelSet + "_" + modelBase + "_" + modelVariant;
		foreach (Item item in this.Sheet)
		{
			if (!item.FitsInSlot(slot))
				continue;

			if (item.ModelSet == modelSet
				&& item.ModelBase == modelBase
				&& item.ModelVariant == modelVariant)
			{
				return item;
			}
		}

		return new DummyItem(modelSet, modelBase, modelVariant);
	}

	public Item? Find(WeaponSlot slot, ulong val)
	{
		if (val == 0)
			return None;

		if (val == uint.MaxValue || val == long.MaxValue || val == ulong.MaxValue)
			return null;

		short modelSet = (short)val;
		short modelBase = (short)(val >> 16);
		short modelVariant = (short)(val >> 32);

		if (modelSet < 0 || modelBase < 0 || modelVariant < 0)
		{
			this.Log.Warning($"Invalid item value: {val}");
			return null;
		}

		return this.Find(slot, (ushort)modelSet, (ushort)modelBase, (ushort)modelVariant);
	}
}

public class DummyItem : Item
{
	public DummyItem(ushort modelSet, ushort modelBase, ushort modelVariant, string name = "???")
	{
		this.ModelSet = modelSet;
		this.ModelBase = modelBase;
		this.ModelVariant = modelVariant;
		this.Name = name;
	}

	public DummyItem(string name = "???")
	{
		this.Name = name;
	}
}