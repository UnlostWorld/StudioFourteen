namespace StudioFourteen.GameData.Library;

using FFXIVClientStructs.FFXIV.Client.Game.Character;
using Lumina.Excel.Sheets;
using Lumina.Text.ReadOnly;
using StudioFourteen.GameData.Extensions;
using StudioFourteen.Library.Sources;

using static FFXIVClientStructs.FFXIV.Client.Game.Character.DrawDataContainer;

public class ItemLibraryEntry(SourceBase source, Item item)
	: ExcelLibraryEntry(source, item.RowId)
{
	public Item Item => item;

	public override string? Name => item.Name.GetString();
	public string? Description => item.Description.ToString();
	public ImageReference? Icon => new ImageReference(item.Icon);

	public int EquipLevel => item.LevelEquip;
	public ulong ModelMain => item.ModelMain;
	public ulong ModelSub => item.ModelSub;

	public ItemUICategory UICategory => item.ItemUICategory.Value;
	public ClassJobCategory ClassJobs => item.ClassJobCategory.Value;
}

public class ItemLibrarySource : ExcelSheetLibrarySource<Item, ItemLibraryEntry>
{
	public ItemLibraryEntry? Find(EquipmentSlot slot, EquipmentModelId modelId)
	{
		if (this.Sheet == null)
			return null;

		if (modelId.Id == 0 && modelId.Variant == 0)
			return null;

		foreach(Item item in this.Sheet)
		{
			if (!item.EquipSlotCategory.Value.Contains(slot))
				continue;

			if (modelId.Id == (ushort)item.ModelMain
				&& modelId.Variant == (ushort)(item.ModelMain >> 16))
			{
				return this.GetRow(item);
			}

			if (modelId.Id == (ushort)item.ModelSub
				&& modelId.Variant == (ushort)(item.ModelSub >> 16))
			{
				return this.GetRow(item);
			}
		}

		return null;
	}

	public ItemLibraryEntry? Find(WeaponSlot slot, WeaponModelId modelId)
	{
		if (this.Sheet == null)
			return null;

		if (modelId.Id == 0 && modelId.Type == 0 && modelId.Variant == 0)
			return null;

		foreach (Item item in this.Sheet)
		{
			if (!item.EquipSlotCategory.Value.Contains(slot))
				continue;

			if (modelId.Id == (ushort)item.ModelMain
				&& modelId.Type == (ushort)(item.ModelMain >> 16)
				&& modelId.Variant == (ushort)(item.ModelMain >> 32))
			{
				return this.GetRow(item);
			}

			if (modelId.Id == (ushort)item.ModelSub
				&& modelId.Type == (ushort)(item.ModelSub >> 16)
				&& modelId.Variant == (ushort)(item.ModelSub >> 32))
			{
				return this.GetRow(item);
			}
		}

		return null;
	}
}