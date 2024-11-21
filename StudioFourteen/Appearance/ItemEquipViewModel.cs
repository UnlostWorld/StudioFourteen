// .                      @@             _____ _______ _    _ _____ _____ ____
//            @       @@@@@             / ____|__   __| |  | |  __ \_   _/ __ \
//           @@@  @@@@                 | (___    | |  | |  | | |  | || || |  | |
//           @@@@@@@@@  @    @          \___ \   | |  | |  | | |  | || || |  | |
//          @@@@       @@@@@@@          ____) |  | |  | |__| | |__| || || |__| |
//      @@@@@             @@@          |_____/   |_|   \____/|_____/_____\____/
//       @@@      @@@      @@        ___     _    _   _  __   _____  ___  ___  _  _
//        @@    @@@@@@@    @@       |  _|  / _ \ | | | || _ \|_   _|| __|| __|| \| |
//        @@    @@@@@@@    @   @    | __| | (_) || |_| ||   /  | |  | _| | _| | .` |
//      @@@@      @@@      @@@@     |_|    \___/  \___/ |_|_\  |_|  |___||___||_|\_|
//       @@@@             @@@
//         @@@@@      @@@@@               This software is licensed under the
//          @@@@@@@@@@@@@@                 GNU AFFERO GENERAL PUBLIC LICENSE
//              @@@@  @                       Version 3, 19 November 2007

namespace StudioFourteen.Appearance;

using FFXIVClientStructs.FFXIV.Client.Game.Character;
using Lumina.Excel.Sheets;
using StudioFourteen.GameData.Library;
using StudioFourteen.Mvm;
using StudioFourteen.Tags;
using StudioFourteen.Utilities;

public class ItemEquipViewModel(DrawDataContainer.EquipmentSlot slot)
	: ItemViewModelBase
{
	public DrawDataContainer.EquipmentSlot Slot { get; private set; } = slot;

	public ushort Id
	{
		get => this.HasValidTarget ? this.ItemEquip.Id : (ushort)0;
		set
		{
			this.BackupCharacter();
			this.ItemEquip.Id = value;
			this.ApplyChangeItem();
		}
	}

	public ushort Variant
	{
		get => this.HasValidTarget ? this.ItemEquip.Variant : (ushort)0;
		set
		{
			this.BackupCharacter();
			this.ItemEquip.Variant = (byte)value;
			this.ApplyChangeItem();
		}
	}

	public override byte Stain0Id
	{
		get => this.HasValidTarget ? this.ItemEquip.Stain0 : (byte)0;
		set
		{
			this.BackupCharacter();
			this.ItemEquip.Stain0 = value;
			this.ApplyChangeItem();
		}
	}

	public override byte Stain1Id
	{
		get => this.HasValidTarget ? this.ItemEquip.Stain1 : (byte)0;
		set
		{
			this.BackupCharacter();
			this.ItemEquip.Stain1 = value;
			this.ApplyChangeItem();
		}
	}

	[AutoNotify]
	public override ItemLibraryEntry? Item
	{
		get
		{
			if (!this.HasValidTarget)
				return null;

			if (this.item == null)
				this.item = this.Services.GameData.Items?.Find(this.Slot, this.ItemEquip);

			return this.item;
		}

		set
		{
			this.item = value;
			this.BackupCharacter();

			if (this.item != null)
			{
				EquipmentModelId modelId = this.item.GetModelId(this.Slot);
				this.Id = modelId.Id;
				this.Variant = modelId.Variant;
			}
			else
			{
				this.Id = 0;
				this.Variant = 0;
			}

			this.ApplyChangeItem();
		}
	}

	protected unsafe ref EquipmentModelId ItemEquip => ref this.Target->DrawData.Equipment(this.Slot);

	public unsafe void ApplyChangeItem()
	{
		Threads.RunOnFrameworkThread(() =>
		{
			this.Services.CharacterAppearance.SetEquipment(this.TargetObjectIndex, this.Slot, this.ItemEquip, CharacterExtensions.UpdateSource.Interface);
		});
	}

	protected override string GetSearchTitle() => Resources.Format("LOC_SearchEquipmentSlot", this.CharacterName, this.Slot.GetDisplayName());

	protected unsafe override void GetDefaultTags(TagCollection tags)
	{
		base.GetDefaultTags(tags);

		tags.Add(this.Slot.ToTag());

		// Filter by the current race.
		if (this.Target != null)
		{
			Race? race = this.Target->DrawData.CustomizeData.GetRace();
			tags.Add(race?.ToTags());
		}
	}
}