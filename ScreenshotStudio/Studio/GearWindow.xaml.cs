namespace ScreenshotStudio.Studio;

using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using ScreenshotStudio.GameData;
using ScreenshotStudio.GameData.Excel;
using ScreenshotStudio.GameData.Sheets;
using ScreenshotStudio.Library;
using ScreenshotStudio.Services;
using ScreenshotStudio.Structs;
using ScreenshotStudio.Tags;
using ScreenshotStudio.Utilities;
using ScreenshotStudio.Windows;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using static FFXIVClientStructs.FFXIV.Client.Game.Character.DrawDataContainer;

public partial class GearWindow : ActorWindow
{
	public GearWindow()
	{
		this.MainHand = new(WeaponSlot.MainHand, this);
		this.OffHand = new(WeaponSlot.OffHand, this);

		this.Head = new(EquipmentSlot.Head, this);
		this.Chest = new(EquipmentSlot.Body, this);
		this.Hands = new(EquipmentSlot.Hands, this);
		this.Legs = new(EquipmentSlot.Legs, this);
		this.Feet = new(EquipmentSlot.Feet, this);
		this.Earring = new(EquipmentSlot.Ears, this);
		this.Necklace = new(EquipmentSlot.Neck, this);
		this.Bracelet = new(EquipmentSlot.Wrists, this);
		this.RingRight = new(EquipmentSlot.RFinger, this);
		this.RingLeft = new(EquipmentSlot.LFinger, this);
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

	[AutoNotify] public unsafe bool CanRevert => this.Services.ActorAppearanceBackup.CanRestore(this.Actor);

	private void OnChangeClicked(object sender, RoutedEventArgs e)
	{
		if (sender is Button btn)
		{
			if (btn.DataContext is ItemEquipViewModel equip)
			{
				TagCollection defaultTags = new();
				defaultTags.Add(equip.Slot.ToTag());

				// Filter by the current race.
				Race? race = this.DrawData.CustomizeData.GetRace();
				if (race != null && race.Name != null)
					defaultTags.Add(race.Name);

				string searchTitle = $"{equip.Slot.GetDisplayName()} {ScreenshotStudio.Resources.Find("LOC_Equipment", "Equipment")}";

				QuickSearch.Show<Item>(
					btn,
					searchTitle,
					defaultTags,
					equip.Item,
					(item, isFinal) =>
				{
					equip.Item = item;
				});
			}
			else if (btn.DataContext is WeaponViewModel weapon)
			{
				TagCollection defaultTags = new();
				defaultTags.Add(weapon.Slot.ToTag());

				// Filter by the current race.
				Race? race = this.DrawData.CustomizeData.GetRace();
				if (race != null && race.Name != null)
					defaultTags.Add(race.Name);

				string searchTitle = $"{weapon.Slot.GetDisplayName()} {ScreenshotStudio.Resources.Find("LOC_Weapon", "Weapon")}";

				QuickSearch.Show<Item>(
					btn,
					searchTitle,
					defaultTags,
					weapon.Item,
					(item, isFinal) =>
					{
						weapon.Item = item;
					});
			}
		}
	}

	private void OnMouseUp(object sender, MouseButtonEventArgs e)
	{
		if (sender is Button btn
			&& btn.DataContext is ItemEquipViewModel equip)
		{
			if (e.ChangedButton == MouseButton.Middle || e.ChangedButton == MouseButton.Right)
			{
				equip.Item = ItemsSheet.None;
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

		QuickSearch.Show<Stain>(
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
		this.Services.ActorAppearanceBackup.Restore(this.Actor);
	}
}

public abstract class GearViewModelBase : ViewModel
{
	protected Item? item;
	private readonly GearWindow window;
	private Stain? stain0;
	private Stain? stain1;

	public GearViewModelBase(GearWindow window)
	{
		this.window = window;
	}

	[AutoNotify] public bool HasValidTarget => this.window.HasValidTarget;
	[AutoNotify] public abstract ushort Set { get; set; }
	[AutoNotify] public abstract ushort Base { get; set; }
	[AutoNotify] public abstract ushort Variant { get; set; }
	[AutoNotify] public abstract byte Stain0Id { get; set; }
	[AutoNotify] public abstract byte Stain1Id { get; set; }

	[AutoNotify] public abstract Item? Item { get; set; }

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

	protected unsafe ref DrawDataContainer DrawData => ref this.window.Actor->DrawData;

	public override bool ShouldTickAutoProperties() => this.window.ShouldTickAutoProperties();
}

public class WeaponViewModel : GearViewModelBase
{
	public readonly WeaponSlot Slot;

	public WeaponViewModel(WeaponSlot slot, GearWindow window)
		: base(window)
	{
		this.Slot = slot;
	}

	public override ushort Set
	{
		get => this.Weapon.ModelId.Id;
		set
		{
			this.Weapon.ModelId.Id = value;
			this.ApplyChangeItem();
		}
	}

	public override ushort Base
	{
		get => this.Weapon.ModelId.Type;
		set
		{
			this.Weapon.ModelId.Type = value;
			this.ApplyChangeItem();
		}
	}

	public override ushort Variant
	{
		get => this.Weapon.ModelId.Variant;
		set
		{
			this.Weapon.ModelId.Variant = (byte)value;
			this.ApplyChangeItem();
		}
	}

	public override byte Stain0Id
	{
		get => this.Weapon.ModelId.Stain0;
		set
		{
			this.Weapon.ModelId.Stain0 = value;
			this.ApplyChangeItem();
		}
	}

	public override byte Stain1Id
	{
		get => this.Weapon.ModelId.Stain1;
		set
		{
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
				this.Set = this.item.ModelSet;
				this.Base = this.item.ModelBase;
				this.Variant = (byte)this.item.ModelVariant;
				this.ApplyChangeItem();
			}
		}
	}

	protected ref DrawObjectData Weapon => ref this.DrawData.Weapon(this.Slot);

	public unsafe void ApplyChangeItem()
	{
		Threads.RunOnFrameworkThread(() =>
		{
			ActorWindow.GetTarget()->UpdateWeapon(this.Slot, this.Weapon.ModelId, Actor.UpdateSource.Interface);
		});
	}
}

public class ItemEquipViewModel : GearViewModelBase
{
	public readonly EquipmentSlot Slot;

	public ItemEquipViewModel(EquipmentSlot slot, GearWindow window)
		: base(window)
	{
		this.Slot = slot;
	}

	public override ushort Set
	{
		get => 0;
		set { }
	}

	public override ushort Base
	{
		get => this.ItemEquip.Id;
		set
		{
			this.ItemEquip.Id = value;
			this.ApplyChangeItem();
		}
	}

	public override ushort Variant
	{
		get => this.ItemEquip.Variant;
		set
		{
			this.ItemEquip.Variant = (byte)value;
			this.ApplyChangeItem();
		}
	}

	public override byte Stain0Id
	{
		get => this.ItemEquip.Stain0;
		set
		{
			this.ItemEquip.Stain0 = value;
			this.ApplyChangeItem();
		}
	}

	public override byte Stain1Id
	{
		get => this.ItemEquip.Stain1;
		set
		{
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
				this.ItemEquip.Id = this.item.ModelBase;
				this.ItemEquip.Variant = (byte)this.item.ModelVariant;
				this.ApplyChangeItem();
			}
		}
	}

	protected ref EquipmentModelId ItemEquip => ref this.DrawData.Equipment(this.Slot);

	public unsafe void ApplyChangeItem()
	{
		Threads.RunOnFrameworkThread(() =>
		{
			ActorWindow.GetTarget()->UpdateEquipment(this.Slot, this.ItemEquip, Actor.UpdateSource.Interface);
		});
	}
}