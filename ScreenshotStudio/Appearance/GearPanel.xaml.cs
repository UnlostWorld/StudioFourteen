namespace ScreenshotStudio.Appearance;

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

public partial class GearPanel : CharacterPanelBase
{
	public GearPanel()
	{
		this.MainHand = new(DrawDataContainer.WeaponSlot.MainHand, this);
		this.OffHand = new(DrawDataContainer.WeaponSlot.OffHand, this);

		this.Head = new(DrawDataContainer.EquipmentSlot.Head, this);
		this.Chest = new(DrawDataContainer.EquipmentSlot.Body, this);
		this.Hands = new(DrawDataContainer.EquipmentSlot.Hands, this);
		this.Legs = new(DrawDataContainer.EquipmentSlot.Legs, this);
		this.Feet = new(DrawDataContainer.EquipmentSlot.Feet, this);
		this.Earring = new(DrawDataContainer.EquipmentSlot.Ears, this);
		this.Necklace = new(DrawDataContainer.EquipmentSlot.Neck, this);
		this.Bracelet = new(DrawDataContainer.EquipmentSlot.Wrists, this);
		this.RingRight = new(DrawDataContainer.EquipmentSlot.RFinger, this);
		this.RingLeft = new(DrawDataContainer.EquipmentSlot.LFinger, this);

		this.Glasses = new(AccessorySlots.Glasses, this);
		this.Ornament = new(this);
	}

	public WeaponViewModel MainHand { get; init; }
	public WeaponViewModel OffHand { get; init; }

	public ItemEquipViewModel Head { get; init; }
	public ItemEquipViewModel Chest { get; init; }
	public ItemEquipViewModel Hands { get; init; }
	public ItemEquipViewModel Legs { get; init; }
	public ItemEquipViewModel Feet { get; init; }
	public ItemEquipViewModel Earring { get; init; }
	public ItemEquipViewModel Necklace { get; init; }
	public ItemEquipViewModel Bracelet { get; init; }
	public ItemEquipViewModel RingRight { get; init; }
	public ItemEquipViewModel RingLeft { get; init; }

	[AutoNotify] public unsafe bool CanRevert => this.Services.CharacterAppearance.CanRestore(this.Target);

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

		LibraryModal.Show<Stain>(
			sender,
			searchTitle,
			defaultTags,
			equip.Stain1,
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

	private unsafe void OnRevertClicked(object sender, RoutedEventArgs e)
	{
		Task.Run(() => this.Services.CharacterAppearance.Restore(this.TargetObjectIndex));
	}
}

public abstract class GearViewModelBase : ViewModel
{
	public abstract void Clear();
	public abstract void Change(object placementTarget);
}

public abstract class GearViewModelBase<T> : GearViewModelBase
	where T : ILibraryEntry
{
	private readonly GearPanel window;

	public GearViewModelBase(GearPanel window)
	{
		this.window = window;
	}

	[AutoNotify] public virtual bool HasValidTarget => this.window.HasValidTarget;
	[AutoNotify] public abstract T? Item { get; set; }

	protected unsafe Character* Target => this.window.Target;

	public override bool ShouldTickAutoProperties() => this.window.ShouldTickAutoProperties() && this.HasValidTarget;

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

	public ItemViewModelBase(GearPanel window)
		: base(window)
	{
	}

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
	public WeaponViewModel(DrawDataContainer.WeaponSlot slot, GearPanel window)
		: base(window)
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
		Race? race = this.Target->DrawData.CustomizeData.GetRace();
		if (race != null && race.Name != null)
		{
			tags.Add(race.Name);
		}
	}
}

public class ItemEquipViewModel : ItemViewModelBase
{
	public ItemEquipViewModel(DrawDataContainer.EquipmentSlot slot, GearPanel window)
		: base(window)
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
		Race? race = this.Target->DrawData.CustomizeData.GetRace();
		if (race != null && race.Name != null)
		{
			tags.Add(race.Name);
		}
	}
}

public abstract class TableRowItemViewModel<T> : GearViewModelBase<T>
	where T : LibraryExcelRow
{
	private T? item;

	public TableRowItemViewModel(GearPanel window)
		: base(window)
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
	public AccessoryViewModel(AccessorySlots slot, GearPanel window)
		: base(window)
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
	public OrnamentViewModel(GearPanel window)
		: base(window)
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