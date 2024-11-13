namespace StudioFourteen.GameData.Sheets;

using Lumina.Excel;
using Lumina.Excel.Sheets;
using Serilog;

using static FFXIVClientStructs.FFXIV.Client.Game.Character.DrawDataContainer;

public class ItemUtility
{
	public ExcelSheet<Item>? Sheet => ServiceManager.Instance.GameData.GetSheet<Item>();
	public ILogger Log => Logging.ForContext<ItemUtility>();

	public Item? Find(EquipmentSlot slot, ushort modelBase, ushort modelVariant)
	{
		if (this.Sheet == null)
			return null;

		if (modelBase == 0 && modelVariant == 0)
			return null;

		/*foreach(Item item in this.Sheet)
		{
			if (!item.FitsInSlot(slot))
				continue;

			if (item.ModelSet == modelSet
				&& item.ModelBase == modelBase
				&& item.ModelVariant == modelVariant)
			{
				return item;
			}
		}*/

		return null;
	}

	public Item? Find(EquipmentSlot slot, ulong val)
	{
		if (val == 0)
			return null;

		if (val == uint.MaxValue || val == long.MaxValue || val == ulong.MaxValue)
			return null;

		short modelBase = (short)val;
		short modelVariant = (short)(val >> 16);

		if (modelBase < 0 || modelVariant < 0)
		{
			this.Log.Warning($"Invalid item value: {val}");
			return null;
		}

		return this.Find(slot, (ushort)modelBase, (ushort)modelVariant);
	}

	public Item? Find(WeaponSlot slot, ushort modelSet, ushort modelBase, ushort modelVariant)
	{
		if (this.Sheet == null)
			return null;

		if (modelSet == 0 && modelBase == 0 && modelVariant == 0)
			return null;

		string lookupKey = slot + "_" + modelSet + "_" + modelBase + "_" + modelVariant;
		/*foreach (Item item in this.Sheet)
		{
			if (!item.FitsInSlot(slot))
				continue;

			if (item.ModelSet == modelSet
				&& item.ModelBase == modelBase
				&& item.ModelVariant == modelVariant)
			{
				return item;
			}
		}*/

		return null;
	}

	public Item? Find(WeaponSlot slot, ulong val)
	{
		if (val == 0)
			return null;

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