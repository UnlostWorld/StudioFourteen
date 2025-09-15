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

namespace StudioFourteen.Scene.GameObjects.Characters.DrawData;

using FFXIVClientStructs.FFXIV.Client.Game.Character;
using StudioFourteen.GameData.Library;
using StudioFourteen.Tags;
using System.Windows;

using static FFXIVClientStructs.FFXIV.Client.Game.Character.DrawDataContainer;
using Character = StudioFourteen.Scene.GameObjects.Characters.Character;
using XivCharacter = FFXIVClientStructs.FFXIV.Client.Game.Character.Character;

public class Weapon
	: GearViewModelBase<ItemLibraryEntry>
{
	protected readonly WeaponSlot Slot;

	private ushort? nextWriteId;
	private ushort lastReadId;
	private ushort? nextWriteVariant;
	private ushort lastReadVariant;
	private ushort? nextWriteTypeId;
	private ushort lastReadTypeId;

	public Weapon(WeaponSlot weaponSlot)
	{
		this.Slot = weaponSlot;
	}

	public ushort Id
	{
		get => this.nextWriteId ?? this.lastReadId;
		set
		{
			if (value == this.Id)
				return;

			this.nextWriteId = value;
			this.NotifyPropertyChanged(nameof(this.Id));
		}
	}

	public ushort TypeId
	{
		get => this.nextWriteTypeId ?? this.lastReadTypeId;
		set
		{
			if (value == this.TypeId)
				return;

			this.nextWriteTypeId = value;
			this.NotifyPropertyChanged(nameof(this.TypeId));
		}
	}

	public ushort Variant
	{
		get => this.nextWriteVariant ?? this.lastReadVariant;
		set
		{
			if (value == this.Variant)
				return;

			this.nextWriteVariant = value;
			this.NotifyPropertyChanged(nameof(this.Variant));
		}
	}

	public override Rect SlotBackgroundRect
	{
		get
		{
			switch (this.Slot)
			{
				case WeaponSlot.MainHand:
				case WeaponSlot.OffHand: return new(0, 144, 64, 64);
			}

			return default;
		}
	}

	public override string SearchTitle => $"Select an item to equip to {this.CharacterName}'s {this.Slot.GetDisplayName()}:";
	public override string DyeSearchTitle => $"Select a dye to apply to {this.CharacterName}'s {this.Slot.GetDisplayName()}:";

	public override void OnGameTick(Character character)
	{
		base.OnGameTick(character);

		DrawObjectData weapon =	character.GetWeapon(this.Slot);
		WeaponModelId modelId = weapon.ModelId;
		bool changed = false;
		if (this.nextWriteId != null && modelId.Id != this.nextWriteId.Value)
		{
			modelId.Id = this.nextWriteId.Value;
			changed = true;
		}

		if (this.nextWriteTypeId != null && modelId.Type != this.nextWriteTypeId.Value)
		{
			modelId.Type = this.nextWriteTypeId.Value;
			changed = true;
		}

		if (this.nextWriteVariant != null && modelId.Variant != this.nextWriteVariant.Value)
		{
			modelId.Variant = this.nextWriteVariant.Value;
			changed = true;
		}

		if (this.nextWriteStain0 != null && modelId.Variant != this.nextWriteStain0.Value)
		{
			modelId.Stain0 = this.nextWriteStain0.Value;
			changed = true;
		}

		if (this.nextWriteStain1 != null && modelId.Variant != this.nextWriteStain1.Value)
		{
			modelId.Stain1 = this.nextWriteStain1.Value;
			changed = true;
		}

		if (changed)
			character.SetWeapon(this.Slot, modelId, UpdateSource.Interface);

		if (this.lastReadId != modelId.Id || this.lastReadId != modelId.Id)
		{
			this.Item = this.Services.GameData.Items?.Find(this.Slot, modelId);
			this.NotifyPropertyChanged(nameof(this.Item));
		}

		this.lastReadId = modelId.Id;
		this.lastReadVariant = modelId.Variant;
		this.lastReadTypeId = modelId.Type;

		if (this.lastReadStain0 != modelId.Stain0)
		{
			this.lastReadStain0 = modelId.Stain0;
			this.stain0 = this.Services.GameData.GetLibraryEntry<StainLibraryEntry>(this.Stain0Id);
			this.NotifyPropertyChanged(nameof(this.Stain0));
		}

		if (this.lastReadStain1 != modelId.Stain1)
		{
			this.lastReadStain1 = modelId.Stain1;
			this.stain1 = this.Services.GameData.GetLibraryEntry<StainLibraryEntry>(this.Stain1Id);
			this.NotifyPropertyChanged(nameof(this.Stain1));
		}

		this.nextWriteId = null;
		this.nextWriteVariant = null;
		this.nextWriteTypeId = null;
		this.nextWriteStain0 = null;
		this.nextWriteStain1 = null;
	}

	protected override void OnItemChanged(ItemLibraryEntry? item)
	{
		if (item == null)
		{
			this.Id = 0;
			this.Variant = 0;
			return;
		}

		WeaponModelId modelId = item.GetModelId(this.Slot);
		this.Id = modelId.Id;
		this.TypeId = modelId.Type;
		this.Variant = modelId.Variant;
	}

	protected unsafe override void GetSearchTags(ref TagCollection tags, Character character)
	{
		base.GetSearchTags(ref tags, character);
		tags.Add(this.Slot.ToTag());
	}
}