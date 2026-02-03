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

namespace StudioFourteen.Scene.DrawData;

using CommunityToolkit.Mvvm.Input;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using StudioFourteen.Scene.DrawData;
using StudioFourteen.Services.Library.GameData.Library;
using static FFXIVClientStructs.FFXIV.Client.Game.Character.DrawDataContainer;
using XivCharacter = FFXIVClientStructs.FFXIV.Client.Game.Character.Character;

public class Equipment : DrawItemBase<ItemLibraryEntry>
{
	private ushort? nextWriteId;
	private ushort lastReadId;
	private byte? nextWriteVariant;
	private byte lastReadVariant;

	public Equipment(EquipmentSlot equipmentSlot)
	{
		this.Slot = equipmentSlot;
	}

	public EquipmentSlot Slot { get; init; }

	public ushort Id
	{
		get => this.nextWriteId ?? this.lastReadId;
		set
		{
			if (value == this.Id)
				return;

			this.nextWriteId = value;
			this.OnPropertyChanged(nameof(this.Id));
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
			this.OnPropertyChanged(nameof(this.Variant));
		}
	}

	public unsafe void OnGameTick(CharacterEquipment character)
	{
		XivCharacter* pCharacter = character.GetXivCharacter();
		if (pCharacter == null)
			return;

		EquipmentModelId modelId = pCharacter->DrawData.Equipment(this.Slot);
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
			character.SetEquipment(this.Slot, modelId, UpdateSource.Interface);

		if (this.lastReadId != modelId.Id || this.lastReadId != modelId.Id)
		{
			this.Item = Studio.Library.GameData.Items?.Find(this.Slot, modelId);
			this.OnPropertyChanged(nameof(this.Item));
		}

		this.lastReadId = modelId.Id;
		this.lastReadVariant = modelId.Variant;

		if (this.lastReadStain0 != modelId.Stain0)
		{
			this.lastReadStain0 = modelId.Stain0;
			this.stain0 = Studio.Library.GameData.GetLibraryEntry<StainLibraryEntry>(this.Stain0Id);
			this.OnPropertyChanged(nameof(this.Stain0));
		}

		if (this.lastReadStain1 != modelId.Stain1)
		{
			this.lastReadStain1 = modelId.Stain1;
			this.stain1 = Studio.Library.GameData.GetLibraryEntry<StainLibraryEntry>(this.Stain1Id);
			this.OnPropertyChanged(nameof(this.Stain1));
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

		EquipmentModelId modelId = item.GetModelId(this.Slot);
		this.Id = modelId.Id;
		this.Variant = modelId.Variant;
	}
}
