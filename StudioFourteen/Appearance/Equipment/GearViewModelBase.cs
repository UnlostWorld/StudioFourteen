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

namespace StudioFourteen.Appearance.Equipment;

using FFXIVClientStructs.FFXIV.Client.Game.Character;
using StudioFourteen.GameData.Library;
using StudioFourteen.Library;
using StudioFourteen.Mvm;
using StudioFourteen.Tags;
using System;
using System.Windows;

using static FFXIVClientStructs.FFXIV.Client.Game.Character.DrawDataContainer;

public abstract class GearViewModelBase : ViewModel
{
	public virtual unsafe void OnFrameworkUpdate(Character* pCharacter)
	{
	}
}

public abstract class GearViewModelBase<TLibraryType> : GearViewModelBase
	where TLibraryType : ExcelLibraryEntry
{
	protected byte? nextWriteStain0;
	protected byte lastReadStain0;
	protected byte? nextWriteStain1;
	protected byte lastReadStain1;

	protected StainLibraryEntry? stain0;
	protected StainLibraryEntry? stain1;
	protected TLibraryType? item;

	public TLibraryType? Item
	{
		get => this.item;
		set
		{
			this.item = value;
			this.RaisePropertyChanged(nameof(this.Item));
			this.OnItemChanged(value);
		}
	}

	public Type LibraryType => typeof(TLibraryType);

	public StainLibraryEntry? Stain0
	{
		get => this.stain0;
		set
		{
			this.stain0 = value;

			if (value == null)
			{
				this.nextWriteStain0 = 0;
			}
			else
			{
				this.nextWriteStain0 = (byte)value.RowId;
			}
		}
	}

	public StainLibraryEntry? Stain1
	{
		get => this.stain1;
		set
		{
			this.stain1 = value;

			if (value == null)
			{
				this.nextWriteStain1 = 0;
			}
			else
			{
				this.nextWriteStain1 = (byte)value.RowId;
			}
		}
	}

	public byte Stain0Id
	{
		get => this.nextWriteStain0 ?? this.lastReadStain0;
		set
		{
			if (value == this.Stain0Id)
				return;

			this.nextWriteStain0 = value;
			this.RaisePropertyChanged(nameof(this.Stain0Id));

			this.stain0 = this.Services.GameData.GetLibraryEntry<StainLibraryEntry>(this.Stain0Id);
			this.RaisePropertyChanged(nameof(this.Stain0));
		}
	}

	public byte Stain1Id
	{
		get => this.nextWriteStain1 ?? this.lastReadStain1;
		set
		{
			if (value == this.Stain1Id)
				return;

			this.nextWriteStain1 = value;
			this.RaisePropertyChanged(nameof(this.Stain1Id));

			this.stain1 = this.Services.GameData.GetLibraryEntry<StainLibraryEntry>(this.Stain1Id);
			this.RaisePropertyChanged(nameof(this.Stain1));
		}
	}

	public void Clear()
	{
		this.Item = null;
		this.Stain0 = null;
		this.Stain1 = null;
	}

	protected abstract void OnItemChanged(TLibraryType? item);
}

public class EquipmentSlotViewModel(EquipmentSlot equipmentSlot)
	: GearViewModelBase<ItemLibraryEntry>
{
	private ushort? nextWriteId;
	private ushort lastReadId;
	private byte? nextWriteVariant;
	private byte lastReadVariant;

	public ushort Id
	{
		get => this.nextWriteId ?? this.lastReadId;
		set
		{
			if (value == this.Id)
				return;

			this.nextWriteId = value;
			this.RaisePropertyChanged(nameof(this.Id));
		}
	}

	public byte Variant
	{
		get => this.nextWriteVariant ?? this.lastReadVariant;
		set
		{
			if (value == this.Variant)
				return;

			this.nextWriteVariant = value;
			this.RaisePropertyChanged(nameof(this.Variant));
		}
	}

	public override unsafe void OnFrameworkUpdate(Character* pCharacter)
	{
		base.OnFrameworkUpdate(pCharacter);

		EquipmentModelId modelId = pCharacter->DrawData.Equipment(equipmentSlot);
		bool changed = false;
		if (this.nextWriteId != null && modelId.Id != this.nextWriteId.Value)
		{
			modelId.Id = this.nextWriteId.Value;
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
			this.Services.CharacterAppearance.SetEquipment(pCharacter->ObjectIndex, equipmentSlot, modelId, CharacterExtensions.UpdateSource.Interface);

		if (this.lastReadId != modelId.Id || this.lastReadId != modelId.Id)
		{
			this.Item = this.Services.GameData.Items?.Find(equipmentSlot, modelId);
			this.RaisePropertyChanged(nameof(this.Item));
		}

		this.lastReadId = modelId.Id;
		this.lastReadVariant = modelId.Variant;

		if (this.lastReadStain0 != modelId.Stain0)
		{
			this.lastReadStain0 = modelId.Stain0;
			this.stain0 = this.Services.GameData.GetLibraryEntry<StainLibraryEntry>(this.Stain0Id);
		}

		if (this.lastReadStain1 != modelId.Stain1)
		{
			this.lastReadStain1 = modelId.Stain1;
			this.stain1 = this.Services.GameData.GetLibraryEntry<StainLibraryEntry>(this.Stain1Id);
		}

		this.nextWriteId = null;
		this.nextWriteVariant = null;
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

		EquipmentModelId modelId = item.GetModelId(equipmentSlot);
		this.Id = modelId.Id;
		this.Variant = modelId.Variant;
	}
}