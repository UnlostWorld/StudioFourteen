// .                    @@             _____ _______ _    _ _____ _____ ____
//          @       @@@@@             / ____|__   __| |  | |  __ \_   _/ __ \
//         @@@  @@@@                 | (___    | |  | |  | | |  | || || |  | |
//         @@@@@@@@@  @    @          \___ \   | |  | |  | | |  | || || |  | |
//        @@@@       @@@@@@@          ____) |  | |  | |__| | |__| || || |__| |
//    @@@@@             @@@          |_____/   |_|   \____/|_____/_____\____/
//     @@@      @@@      @@        ___     _    _   _  __   _____  ___  ___  _  _
//      @@    @@@@@@@    @@       |  _|  / _ \ | | | || _ \|_   _|| __|| __|| \| |
//      @@    @@@@@@@    @   @    | __| | (_) || |_| ||   /  | |  | _| | _| | .` |
//    @@@@      @@@      @@@@     |_|    \___/  \___/ |_|_\  |_|  |___||___||_|\_|
//     @@@@             @@@        https://github.com/UnlostWorld/StudioFourteen
//       @@@@@      @@@@@
//        @@@@@@@@@@@@@@                This software is licensed under the
//            @@@@  @                  GNU AFFERO GENERAL PUBLIC LICENSE v3

namespace StudioFourteen.Services.Library.GameData.Library;

using System;
using System.Threading.Tasks;
using global::Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using Lumina.Excel.Sheets;
using Lumina.Text.ReadOnly;
using StudioFourteen.Services.Library.GameData.Extensions;

using StudioFourteen.Services.Library.GameData.Sheets;

using static FFXIVClientStructs.FFXIV.Client.Game.Character.DrawDataContainer;
using ClassJobCategory = StudioFourteen.Services.Library.GameData.Sheets.ClassJobCategory;

public class ItemLibraryEntry : ExcelLibraryEntry
{
	public ItemLibraryEntry(SourceBase source, Item item)
		: base(source, item.RowId)
	{
		this.Item = item;
	}

	public Item Item { get; init; }

	public override string? Name => this.Item.Name.GetString();
	public string? Description => this.Item.Description.GetString();
	public override object? Icon => new TextureIconReference(this.Item.Icon);

	public int EquipLevel => this.Item.LevelEquip;

	public ItemUICategory UICategory => this.Item.ItemUICategory.Value;
	public ClassJobCategory? ClassJobs => Studio.DataManager.GetRow<ClassJobCategory>(this.Item.ClassJobCategory.RowId);
	public EquipRaceCategory? EquipRestriction => Studio.DataManager.GetRow<EquipRaceCategory>(this.Item.EquipRestriction);
	public EquipSlotCategory? EquipSlot => this.Item.EquipSlotCategory.ValueNullable;

	public string? ClassJobsName => this.ClassJobs?.Name.GetString();
	public string? UICategoryName => this.UICategory.Name.GetString();

	public EquipmentModelId GetModelId(EquipmentSlot slot)
	{
		EquipmentModelId id = default;
		id.Id = (ushort)this.Item.ModelMain;
		id.Variant = (byte)(this.Item.ModelMain >> 16);
		return id;
	}

	public WeaponModelId GetModelId(WeaponSlot slot)
	{
		WeaponModelId id = default;

		if (slot == WeaponSlot.MainHand)
		{
			id.Id = (ushort)this.Item.ModelMain;
			id.Type = (ushort)(this.Item.ModelMain >> 16);
			id.Variant = (ushort)(this.Item.ModelMain >> 32);
		}
		else if (slot == WeaponSlot.OffHand)
		{
			id.Id = (ushort)this.Item.ModelSub;
			id.Type = (ushort)(this.Item.ModelSub >> 16);
			id.Variant = (ushort)(this.Item.ModelSub >> 32);
		}

		return id;
	}

	/*public async Task EquipTo(Character character)
	{
		if (this.EquipSlot == null)
			return;

		EquipSlotCategory equipSlot = this.EquipSlot.Value;

		foreach (EquipmentSlot slot in Enum.GetValues<EquipmentSlot>())
		{
			if (equipSlot.Contains(slot))
			{
				await this.EquipTo(character, slot);
				return;
			}
		}
	}

	public async Task EquipTo(Character character, EquipmentSlot slot)
	{
		await TickService.GameTick();

		character.SetEquipment(
			slot,
			this.GetModelId(slot),
			UpdateSource.Interface);
	}

	public Task<bool> CanEquipTo(int objectTableId)
	{
		if (!this.Item.EquipSlotCategory.IsValid || this.Item.EquipSlotCategory.RowId == 0)
			return Task.FromResult(false);

		return Task.FromResult(true);
	}

	public override Task GetLibraryMenus(ILibraryContextMenu menu)
	{
		MenuEntry webSearchMenu = menu.AddMenu(
			IconChar.Search,
			XamlResources.Find("LOC_SheetItem_WebSearch", "Search"));

		webSearchMenu.AddChild(
			IconChar.Globe,
			XamlResources.Find("LOC_SheetItem_EorzeaDatabase", "Lodestone"),
			() =>
			{
				UrlUtility.Open($"https://na.finalfantasyxiv.com/lodestone/playguide/db/search/?patch=&db_search_category=&q={this.Name}");
			});

		webSearchMenu.AddChild(
			IconChar.Globe,
			XamlResources.Find("LOC_SheetItem_GarlandData", "Garland Data"),
			() =>
			{
				UrlUtility.Open($"https://garlandtools.org/db/#item/{this.RowId}");
			});

		webSearchMenu.AddChild(
			IconChar.Globe,
			XamlResources.Find("LOC_SheetItem_GamerEscape", "Gamer Escape"),
			() =>
			{
				UrlUtility.Open($"https://ffxiv.gamerescape.com/?search={this.Name}");
			});

		return base.GetLibraryMenus(menu);
	}

	public override LibraryPreviewBase? GetPreview()
	{
		return new ItemLibraryPreview(this);
	}

	public override async Task Execute()
	{
		Character? character = this.Services.Selection.GetLast<Character>();

		if (character == null)
			return;

		await this.EquipTo(character);
	}*/
}

public class ItemLibrarySource : ExcelSheetLibrarySource<Item, ItemLibraryEntry>
{
	public ItemLibraryEntry? Find(EquipmentSlot slot, EquipmentModelId modelId)
	{
		if (this.Sheet == null)
			return null;

		if (modelId.Id == 0 && modelId.Variant == 0)
			return null;

		foreach (Item item in this.Sheet)
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

	protected override bool IncludeEntry(Item row)
	{
		// Only equippable items.
		if (row.EquipRestriction == 0)
			return false;

		return base.IncludeEntry(row);
	}
}