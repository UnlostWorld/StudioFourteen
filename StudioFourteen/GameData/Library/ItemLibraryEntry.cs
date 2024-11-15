namespace StudioFourteen.GameData.Library;

using FFXIVClientStructs.FFXIV.Client.Game.Character;
using Lumina.Excel.Sheets;
using Lumina.Text.ReadOnly;
using StudioFourteen.GameData.Extensions;
using StudioFourteen.Library.Sources;

using static FFXIVClientStructs.FFXIV.Client.Game.Character.DrawDataContainer;
using ClassJobCategory = StudioFourteen.GameData.Sheets.ClassJobCategory;

public class ItemLibraryEntry : ExcelLibraryEntry
{
	public readonly Item Item;

	public ItemLibraryEntry(SourceBase source, Item item)
		: base(source, item.RowId)
	{
		this.Item = item;

		this.Tags.Add(this.Item.EquipSlotCategory.Value.ToTags());
		this.Tags.Add(this.EquipRestriction?.ToTags());
		this.Tags.Add(this.ClassJobs?.ToTags());

		if (this.Name == null)
			this.Tags.Add("Unnamed");

		if (this.EquipLevel <= 1)
			this.Tags.Add("No Level");
	}

	public override string? Name => this.Item.Name.GetString();
	public string? Description => this.Item.Description.ToString();
	public ImageReference? Icon => new ImageReference(this.Item.Icon);

	public int EquipLevel => this.Item.LevelEquip;
	public ulong ModelMain => this.Item.ModelMain;
	public ulong ModelSub => this.Item.ModelSub;

	public ItemUICategory UICategory => this.Item.ItemUICategory.Value;
	public ClassJobCategory? ClassJobs => this.Services.GameData.GetRow<ClassJobCategory>(this.Item.ClassJobCategory.RowId);
	public EquipRaceCategory? EquipRestriction => this.Services.GameData.GetRow<EquipRaceCategory>(this.Item.EquipRestriction);
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