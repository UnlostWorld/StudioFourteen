namespace StudioFourteen.Appearance;

using FFXIVClientStructs.FFXIV.Client.Game.Character;
using StudioFourteen.GameData;
using StudioFourteen.GameData.Excel;
using StudioFourteen.Mvm;
using StudioFourteen.Tags;
using StudioFourteen.Utilities;

public class ItemEquipViewModel(DrawDataContainer.EquipmentSlot slot)
	: ItemViewModelBase
{
	public DrawDataContainer.EquipmentSlot Slot { get; private set; } = slot;

	public override ushort Set
	{
		get => 0;
		set { }
	}

	public override ushort Base
	{
		get => this.HasValidTarget ? this.ItemEquip.Id : (ushort)0;
		set
		{
			this.BackupCharacter();
			this.ItemEquip.Id = value;
			this.ApplyChangeItem();
		}
	}

	public override ushort Variant
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
	public override Item? Item
	{
		get
		{
			if (!this.HasValidTarget)
				return null;

			if (this.item == null)
				this.item = GameDataService.Items?.Find(this.Slot, this.Set, this.Base, this.Variant);

			return this.item;
		}

		set
		{
			this.item = value;

			this.BackupCharacter();

			if (this.item != null)
			{
				// Submodels?
				this.ItemEquip.Id = this.item.ModelBase;
				this.ItemEquip.Variant = (byte)this.item.ModelVariant;
			}
			else
			{
				this.ItemEquip.Id = 0;
				this.ItemEquip.Variant = 0;
			}

			this.ApplyChangeItem();
		}
	}

	protected unsafe ref EquipmentModelId ItemEquip => ref this.Target->DrawData.Equipment(this.Slot);

	public unsafe void ApplyChangeItem()
	{
		Threads.RunOnFrameworkThread(() =>
		{
			this.Target->UpdateEquipment(this.Slot, this.ItemEquip, CharacterExtensions.UpdateSource.Interface);
		});
	}

	protected override string GetSearchTitle() => $"{this.Slot.GetDisplayName()} {Resources.Find("LOC_Equipment", "Equipment")}";

	protected unsafe override void GetDefaultTags(TagCollection tags)
	{
		base.GetDefaultTags(tags);

		tags.Add(this.Slot.ToTag());

		// Filter by the current race.
		if (this.Target != null)
		{
			Race? race = this.Target->DrawData.CustomizeData.GetRace();
			if (race != null && race.Name != null)
			{
				tags.Add(race.Name);
			}
		}
	}
}