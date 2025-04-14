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

namespace StudioFourteen.GameData.Library;

using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FontAwesome.Sharp;
using Lumina.Excel.Sheets;
using Lumina.Text.ReadOnly;
using StudioFourteen.GameData.Extensions;
using StudioFourteen.Library;
using StudioFourteen.Library.LibraryMenu;
using StudioFourteen.Library.Sources;
using StudioFourteen.Services;
using StudioFourteen.Utilities;
using System;
using System.Threading.Tasks;
using static FFXIVClientStructs.FFXIV.Client.Game.Character.DrawDataContainer;

using ClassJobCategory = StudioFourteen.GameData.Sheets.ClassJobCategory;

public class ItemLibraryEntry : ExcelLibraryEntry
{
	public readonly Item Item;

	public ItemLibraryEntry(SourceBase source, Item item)
		: base(source, item.RowId)
	{
		this.Item = item;

		if (this.Item.EquipSlotCategory.Value.IsEquipable())
			this.Tags.Add("Equipable");

		this.Tags.Add(this.Item.EquipSlotCategory.Value.ToTags());
		this.Tags.Add(this.EquipRestriction?.ToTags());
		this.Tags.Add(this.ClassJobs?.ToTags());

		if (this.Name == null)
			this.Tags.Add("Unnamed");

		if (this.EquipLevel <= 1)
			this.Tags.Add("No Level");

		if (this.EquipSlot != null)
			this.Tags.Add(this.EquipSlot.Value.ToTags());

		if (this.Item.DyeCount == 1)
		{
			this.Tags.Add("Dyeable (One Channel)");
		}
		else if (this.Item.DyeCount == 2)
		{
			this.Tags.Add("Dyeable (Two Channel)");
		}
	}

	public override string? Name => this.Item.Name.GetString();
	public string? Description => this.Item.Description.GetString();
	public override object? Icon => new ImageReference(this.Item.Icon);

	public int EquipLevel => this.Item.LevelEquip;

	public ItemUICategory UICategory => this.Item.ItemUICategory.Value;
	public ClassJobCategory? ClassJobs => this.Services.GameData.GetRow<ClassJobCategory>(this.Item.ClassJobCategory.RowId);
	public EquipRaceCategory? EquipRestriction => this.Services.GameData.GetRow<EquipRaceCategory>(this.Item.EquipRestriction);
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

	public async Task EquipTo(int objectTableId)
	{
		if (this.EquipSlot == null)
			return;

		EquipSlotCategory equipSlot = this.EquipSlot.Value;

		foreach (EquipmentSlot slot in Enum.GetValues<EquipmentSlot>())
		{
			if (equipSlot.Contains(slot))
			{
				await this.EquipTo(objectTableId, slot);
				return;
			}
		}
	}

	public async Task EquipTo(int objectTableId, EquipmentSlot slot)
	{
		await TickService.GameTick();

		this.Services.CharacterAppearance.SetEquipment(
			this.Services.Target.TargetObjectIndex,
			slot,
			this.GetModelId(slot),
			UpdateSource.Library);
	}

	public Task<bool> CanEquipTo(int objectTableId)
	{
		if (!this.Item.EquipSlotCategory.IsValid || this.Item.EquipSlotCategory.RowId == 0)
			return Task.FromResult(false);

		return Task.FromResult(true);
	}

	/*public override Task GetLibraryMenus(ILibraryContextMenu menu)
	{
		MenuEntry webSearchMenu = menu.AddMenu(
			IconChar.Search,
			Resources.Find("LOC_SheetItem_WebSearch", "Search"));

		webSearchMenu.AddChild(
			IconChar.Globe,
			Resources.Find("LOC_SheetItem_EorzeaDatabase", "Lodestone"),
			() =>
			{
				UrlUtility.Open($"https://na.finalfantasyxiv.com/lodestone/playguide/db/search/?patch=&db_search_category=&q={this.Name}");
			});

		webSearchMenu.AddChild(
			IconChar.Globe,
			Resources.Find("LOC_SheetItem_GarlandData", "Garland Data"),
			() =>
			{
				UrlUtility.Open($"https://garlandtools.org/db/#item/{this.RowId}");
			});

		webSearchMenu.AddChild(
			IconChar.Globe,
			Resources.Find("LOC_SheetItem_GamerEscape", "Gamer Escape"),
			() =>
			{
				UrlUtility.Open($"https://ffxiv.gamerescape.com/?search={this.Name}");
			});

		return base.GetLibraryMenus(menu);
	}*/

	public override LibraryPreviewBase? GetPreview()
	{
		return new ItemLibraryPreview(this);
	}
}

public class ItemLibraryPreview(ItemLibraryEntry item)
	: LibraryPreviewBase
{
	private WeaponSlot backupWeaponSlot;
	private WeaponModelId? backupWeapon;
	private EquipmentSlot backupEquipmentSlot;
	private EquipmentModelId? backupEquipment;

	protected override async Task Start(LibraryPreviewBase? other)
	{
		if (other != null)
		{
			await other.StopPreviewAsync();
		}

		if (this.Services.Target.TargetObjectIndex == -1)
			return;

		if (item.EquipSlot == null)
			return;

		EquipSlotCategory equipSlot = item.EquipSlot.Value;
		foreach (EquipmentSlot slot in Enum.GetValues<EquipmentSlot>())
		{
			if (equipSlot.Contains(slot))
			{
				await this.Start(slot);
				return;
			}
		}

		foreach (WeaponSlot slot in Enum.GetValues<WeaponSlot>())
		{
			if (equipSlot.Contains(slot))
			{
				await this.Start(slot);
				return;
			}
		}
	}

	protected virtual async Task Start(EquipmentSlot slot)
	{
		await TickService.GameTick();

		this.backupEquipmentSlot = slot;

		unsafe
		{
			Character* pCharacter = this.Services.Target.GetTarget();
			this.backupEquipment = pCharacter->DrawData.Equipment(slot);
		}

		this.Services.CharacterAppearance.SetEquipment(
			this.Services.Target.TargetObjectIndex,
			slot,
			item.GetModelId(slot),
			UpdateSource.Preview);
	}

	protected virtual async Task Start(WeaponSlot slot)
	{
		await TickService.GameTick();

		this.backupWeaponSlot = slot;

		unsafe
		{
			Character* pCharacter = this.Services.Target.GetTarget();
			this.backupWeapon = pCharacter->DrawData.Weapon(slot).ModelId;
		}

		this.Services.CharacterAppearance.SetWeapon(
			this.Services.Target.TargetObjectIndex,
			slot,
			item.GetModelId(slot),
			UpdateSource.Preview);
	}

	protected override async Task Stop()
	{
		await TickService.GameTick();

		if (this.backupEquipment != null)
		{
			this.Services.CharacterAppearance.SetEquipment(
				this.Services.Target.TargetObjectIndex,
				this.backupEquipmentSlot,
				this.backupEquipment.Value,
				UpdateSource.Preview);
		}

		if (this.backupWeapon != null)
		{
			this.Services.CharacterAppearance.SetWeapon(
				this.Services.Target.TargetObjectIndex,
				this.backupWeaponSlot,
				this.backupWeapon.Value,
				UpdateSource.Preview);
		}
	}
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