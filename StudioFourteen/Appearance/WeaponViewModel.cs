namespace StudioFourteen.Appearance;

using FFXIVClientStructs.FFXIV.Client.Game.Character;
using Lumina.Excel.Sheets;
using StudioFourteen.GameData.Library;
using StudioFourteen.Mvm;
using StudioFourteen.Tags;
using StudioFourteen.Utilities;

public class WeaponViewModel(DrawDataContainer.WeaponSlot slot)
	: ItemViewModelBase
{
	public DrawDataContainer.WeaponSlot Slot { get; private set; } = slot;

	public ushort Id
	{
		get => this.HasValidTarget ? this.Weapon.ModelId.Id : (ushort)0;
		set
		{
			this.BackupCharacter();
			this.Weapon.ModelId.Id = value;
			this.ApplyChangeItem();
		}
	}

	public ushort Type
	{
		get => this.HasValidTarget ? this.Weapon.ModelId.Type : (ushort)0;
		set
		{
			this.BackupCharacter();
			this.Weapon.ModelId.Type = value;
			this.ApplyChangeItem();
		}
	}

	public ushort Variant
	{
		get => this.HasValidTarget ? this.Weapon.ModelId.Variant : (ushort)0;
		set
		{
			this.BackupCharacter();
			this.Weapon.ModelId.Variant = (byte)value;
			this.ApplyChangeItem();
		}
	}

	public override byte Stain0Id
	{
		get => this.HasValidTarget ? this.Weapon.ModelId.Stain0 : (byte)0;
		set
		{
			this.BackupCharacter();
			this.Weapon.ModelId.Stain0 = value;
			this.ApplyChangeItem();
		}
	}

	public override byte Stain1Id
	{
		get => this.HasValidTarget ? this.Weapon.ModelId.Stain1 : (byte)0;
		set
		{
			this.BackupCharacter();
			this.Weapon.ModelId.Stain1 = value;
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
				this.item = this.Services.GameData.Items?.Find(this.Slot, this.Weapon.ModelId);

			return this.item;
		}

		set
		{
			this.item = value;
			this.BackupCharacter();

			if (this.item != null)
			{
				WeaponModelId modelId = this.item.GetModelId(this.Slot);
				this.Id = modelId.Id;
				this.Type = modelId.Type;
				this.Variant = modelId.Variant;
			}
			else
			{
				this.Id = 0;
				this.Type = 0;
				this.Variant = 0;
			}

			this.ApplyChangeItem();
		}
	}

	protected unsafe ref DrawObjectData Weapon => ref this.Target->DrawData.Weapon(this.Slot);

	public unsafe void ApplyChangeItem()
	{
		Threads.RunOnFrameworkThread(() =>
		{
			this.Target->UpdateWeapon(this.Slot, this.Weapon.ModelId, CharacterExtensions.UpdateSource.Interface);
		});
	}

	protected override string GetSearchTitle() => Resources.Format("LOC_SearchWeaponSlot", this.CharacterName, this.Slot.GetDisplayName());

	protected unsafe override void GetDefaultTags(TagCollection tags)
	{
		base.GetDefaultTags(tags);

		tags.Add(this.Slot.ToTag());

		// Filter by the current race.
		if (this.Target != null)
		{
			Race? race = this.Target->DrawData.CustomizeData.GetRace();
			tags.Add(race?.GetTag());
		}
	}
}