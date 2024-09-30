namespace ScreenshotStudio.Appearance;

using DependencyPropertyGenerator;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using ScreenshotStudio.GameData;
using ScreenshotStudio.GameData.Excel;
using ScreenshotStudio.Library;
using ScreenshotStudio.Mvm;
using ScreenshotStudio.Panels;
using ScreenshotStudio.Services;
using ScreenshotStudio.Tags;
using ScreenshotStudio.Utilities;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using Ornament = ScreenshotStudio.GameData.Excel.Ornament;

public enum AccessorySlots
{
	Glasses,
}

[DependencyProperty<CharacterPanelBase>("Panel")]
public partial class GearView : View
{
	public GearView()
	{
		this.MainHand = new(DrawDataContainer.WeaponSlot.MainHand);
		this.OffHand = new(DrawDataContainer.WeaponSlot.OffHand);

		this.Head = new(DrawDataContainer.EquipmentSlot.Head);
		this.Chest = new(DrawDataContainer.EquipmentSlot.Body);
		this.Hands = new(DrawDataContainer.EquipmentSlot.Hands);
		this.Legs = new(DrawDataContainer.EquipmentSlot.Legs);
		this.Feet = new(DrawDataContainer.EquipmentSlot.Feet);
		this.Earring = new(DrawDataContainer.EquipmentSlot.Ears);
		this.Necklace = new(DrawDataContainer.EquipmentSlot.Neck);
		this.Bracelet = new(DrawDataContainer.EquipmentSlot.Wrists);
		this.RingRight = new(DrawDataContainer.EquipmentSlot.RFinger);
		this.RingLeft = new(DrawDataContainer.EquipmentSlot.LFinger);

		this.Glasses = new(AccessorySlots.Glasses);
		this.Ornament = new();
	}

	public unsafe Character* Target => this.Services.Target.Target;

	[AutoNotify] public WeaponViewModel MainHand { get; init; }
	[AutoNotify] public WeaponViewModel OffHand { get; init; }

	[AutoNotify] public ItemEquipViewModel Head { get; init; }
	[AutoNotify] public ItemEquipViewModel Chest { get; init; }
	[AutoNotify] public ItemEquipViewModel Hands { get; init; }
	[AutoNotify] public ItemEquipViewModel Legs { get; init; }
	[AutoNotify] public ItemEquipViewModel Feet { get; init; }
	[AutoNotify] public ItemEquipViewModel Earring { get; init; }
	[AutoNotify] public ItemEquipViewModel Necklace { get; init; }
	[AutoNotify] public ItemEquipViewModel Bracelet { get; init; }
	[AutoNotify] public ItemEquipViewModel RingRight { get; init; }
	[AutoNotify] public ItemEquipViewModel RingLeft { get; init; }

	public AccessoryViewModel Glasses { get; init; }
	public OrnamentViewModel Ornament { get; init; }

	private unsafe void OnChangeClicked(object sender, RoutedEventArgs e)
	{
		if (sender is Button btn)
		{
			if (btn.DataContext is GearViewModelBase gear)
			{
				gear.Change(btn);
			}
		}
	}

	private void OnMouseUp(object sender, MouseButtonEventArgs e)
	{
		if (e.ChangedButton != MouseButton.Middle && e.ChangedButton != MouseButton.Right)
			return;

		if (sender is Button btn)
		{
			if (btn.DataContext is GearViewModelBase gear)
			{
				gear.Clear();
			}
		}
	}

	private void OnChangeDye1Clicked(object sender, RoutedEventArgs e)
	{
		if (sender is Button btn
			&& btn.DataContext is ItemEquipViewModel equip)
		{
			this.OnChangeDye(sender, equip, 0);
		}
	}

	private void OnChangeDye2Clicked(object sender, RoutedEventArgs e)
	{
		if (sender is Button btn
			&& btn.DataContext is ItemEquipViewModel equip)
		{
			this.OnChangeDye(sender, equip, 1);
		}
	}

	private void OnChangeDye(object sender, ItemEquipViewModel equip, int dyeChanel)
	{
		TagCollection defaultTags = new();
		defaultTags.Add("Named");

		string searchTitle = $"{equip.Slot} {ScreenshotStudio.Resources.Find("Dye", "Dye")}";

		Stain? currentStain = equip.Stain0;
		if (dyeChanel == 1)
			currentStain = equip.Stain1;

		LibraryModal.Show<Stain>(
			sender,
			searchTitle,
			defaultTags,
			currentStain,
			(stain, isFinal) =>
			{
				if (dyeChanel == 0)
				{
					equip.Stain0 = stain;
				}
				else
				{
					equip.Stain1 = stain;
				}
			});
	}
}

public abstract class GearViewModelBase : ViewModel
{
	public unsafe Character* Target => this.Services.Target.Target;
	[AlwaysNotify] public string? CharacterName => this.Services.Target.CharacterName;
	[AlwaysNotify] public virtual bool HasValidTarget => this.Services.Target.HasValidTarget;
	[AlwaysNotify] public int TargetObjectIndex => this.Services.Target.TargetObjectIndex;

	public abstract void Clear();
	public abstract void Change(object placementTarget);

	public override bool ShouldTickAutoProperties() => this.HasValidTarget;
}

public abstract class GearViewModelBase<T> : GearViewModelBase
	where T : ILibraryEntry
{
	[AutoNotify] public abstract T? Item { get; set; }

	public override void Clear()
	{
		this.Item = default;
	}

	public sealed override void Change(object placementTarget)
	{
		TagCollection defaultTags = new();
		this.GetDefaultTags(defaultTags);

		string searchTitle = this.GetSearchTitle();

		LibraryModal.Show<T>(
			placementTarget,
			searchTitle,
			defaultTags,
			this.Item,
			(item, isFinal) =>
			{
				this.Item = item;
			});
	}

	protected virtual string GetSearchTitle()
	{
		return string.Empty;
	}

	protected virtual void GetDefaultTags(TagCollection tags)
	{
	}
}

public abstract class ItemViewModelBase : GearViewModelBase<Item>
{
	protected Item? item;
	private Stain? stain0;
	private Stain? stain1;

	[AutoNotify] public abstract ushort Set { get; set; }
	[AutoNotify] public abstract ushort Base { get; set; }
	[AutoNotify] public abstract ushort Variant { get; set; }
	[AutoNotify] public abstract byte Stain0Id { get; set; }
	[AutoNotify] public abstract byte Stain1Id { get; set; }

	[AutoNotify]
	public Stain? Stain0
	{
		get
		{
			if (!this.HasValidTarget)
				return null;

			if (this.stain0 == null || this.stain0.RowId != this.Stain0Id)
				this.stain0 = GameDataService.GetRow<Stain>(this.Stain0Id);

			return this.stain0;
		}

		set
		{
			this.stain0 = value;

			if (this.stain0 != null)
			{
				this.Stain0Id = (byte)this.stain0.RowId;
			}
		}
	}

	[AutoNotify]
	public Stain? Stain1
	{
		get
		{
			if (!this.HasValidTarget)
				return null;

			if (this.stain1 == null || this.stain1.RowId != this.Stain1Id)
				this.stain1 = GameDataService.GetRow<Stain>(this.Stain1Id);

			return this.stain1;
		}

		set
		{
			this.stain1 = value;

			if (this.stain1 != null)
			{
				this.Stain1Id = (byte)this.stain1.RowId;
			}
		}
	}

	public unsafe void BackupCharacter()
	{
		this.Services.CharacterAppearance.Backup(this.Target);
	}
}

public class WeaponViewModel : ItemViewModelBase
{
	public WeaponViewModel(DrawDataContainer.WeaponSlot slot)
	{
		this.Slot = slot;
	}

	public DrawDataContainer.WeaponSlot Slot { get; private set; }

	public override ushort Set
	{
		get => this.HasValidTarget ? this.Weapon.ModelId.Id : (ushort)0;
		set
		{
			this.BackupCharacter();
			this.Weapon.ModelId.Id = value;
			this.ApplyChangeItem();
		}
	}

	public override ushort Base
	{
		get => this.HasValidTarget ? this.Weapon.ModelId.Type : (ushort)0;
		set
		{
			this.BackupCharacter();
			this.Weapon.ModelId.Type = value;
			this.ApplyChangeItem();
		}
	}

	public override ushort Variant
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

			if (this.item != null)
			{
				// Submodels?
				this.BackupCharacter();
				this.Set = this.item.ModelSet;
				this.Base = this.item.ModelBase;
				this.Variant = (byte)this.item.ModelVariant;
				this.ApplyChangeItem();
			}
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

	protected override string GetSearchTitle() => $"{this.Slot.GetDisplayName()} {Resources.Find("LOC_Weapon", "Weapon")}";

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

public class ItemEquipViewModel : ItemViewModelBase
{
	public ItemEquipViewModel(DrawDataContainer.EquipmentSlot slot)
	{
		this.Slot = slot;
	}

	public DrawDataContainer.EquipmentSlot Slot { get; private set; }

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

			if (this.item != null)
			{
				// Submodels?
				this.BackupCharacter();
				this.ItemEquip.Id = this.item.ModelBase;
				this.ItemEquip.Variant = (byte)this.item.ModelVariant;
				this.ApplyChangeItem();
			}
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

public abstract class TableRowItemViewModel<T> : GearViewModelBase<T>
	where T : LibraryExcelRow
{
	private T? item;

	public TableRowItemViewModel()
	{
	}

	[AutoNotify]
	public sealed override T? Item
	{
		get
		{
			if (!this.HasValidTarget)
				return null;

			if (this.Value == 0)
				return null;

			if (this.item == null)
				this.item = GameDataService.GetRow<T>(this.Value);

			return this.item;
		}

		set
		{
			if (value == null || !value.IsValid)
			{
				this.Value = 0;
			}
			else
			{
				this.Value = (ushort)value.RowId;
			}
		}
	}

	[AutoNotify]
	public ushort Value
	{
		get
		{
			if (!this.HasValidTarget)
				return 0;

			return this.LiveValue;
		}
		set
		{
			this.item = GameDataService.GetRow<T>(value);

			if (!this.HasValidTarget)
				return;

			if (this.item == null || !this.item.IsValid)
			{
				this.LiveValue = 0;
			}
			else
			{
				Threads.RunOnFrameworkThread(() =>
				{
					this.LiveValue = value;
				});
			}
		}
	}

	protected unsafe abstract ushort LiveValue
	{
		get;
		set;
	}
}

/// <summary>
/// Accessory view model for glasses.
/// </summary>
public class AccessoryViewModel : TableRowItemViewModel<Glasses>
{
	public AccessoryViewModel(AccessorySlots slot)
	{
		this.Slot = slot;
	}

	public AccessorySlots Slot { get; init; }

	protected unsafe override ushort LiveValue
	{
		get => this.Target->DrawData.GlassesIds[(int)this.Slot];
		set => this.Target->DrawData.SetGlasses((int)this.Slot, value);
	}

	protected override string GetSearchTitle() => Resources.Find("LOC_Glasses", "Glasses");
}

/// <summary>
/// Ornament view model for wings and umbrellas.
/// </summary>
public class OrnamentViewModel : TableRowItemViewModel<Ornament>
{
	public OrnamentViewModel()
	{
	}

	[AutoNotify]
	public override unsafe bool HasValidTarget
	{
		get
		{
			if (!base.HasValidTarget)
				return false;

			return this.Target->OrnamentData.OrnamentObject != null;
		}
	}

	protected unsafe override ushort LiveValue
	{
		get => this.Target->OrnamentData.OrnamentId;
		set
		{
			// Can only set 0 (no ornament) while in gpose
			if (value == 0 && !this.Services.GroupPose.IsGroupPosing)
				return;

			this.Target->OrnamentData.SetupOrnament((short)value, 0);
		}
	}

	protected override string GetSearchTitle() => Resources.Find("LOC_Ornament", "Fashion Accessories");
}