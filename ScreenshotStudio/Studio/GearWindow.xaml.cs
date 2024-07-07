namespace ScreenshotStudio.Studio;

using ScreenshotStudio.GameData;
using ScreenshotStudio.Services;
using ScreenshotStudio.Structs;
using ScreenshotStudio.Windows;
using ScreenshotStudio.GameData.Excel;
using System.Windows;
using ScreenshotStudio.Library;
using System.Windows.Controls;
using ScreenshotStudio.Tags;
using System;
using System.Windows.Input;
using ScreenshotStudio.GameData.Sheets;
using System.Text;

public partial class GearWindow : ActorWindow
{
	public GearWindow()
	{
		this.Head = new(ItemSlots.Head, this);
		this.Chest = new(ItemSlots.Chest, this);
		this.Hands = new(ItemSlots.Hands, this);
		this.Legs = new(ItemSlots.Legs, this);
		this.Feet = new(ItemSlots.Feet, this);
		this.Earring = new(ItemSlots.Earring, this);
		this.Necklace = new(ItemSlots.Necklace, this);
		this.Bracelet = new(ItemSlots.Bracelet, this);
		this.RingRight = new(ItemSlots.RingRight, this);
		this.RingLeft = new(ItemSlots.RingLeft, this);
	}

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

	private void OnChangeClicked(object sender, RoutedEventArgs e)
	{
		if (sender is Button btn
			&& btn.DataContext is ItemEquipViewModel equip)
		{
			TagCollection defaultTags = new();
			defaultTags.Add(equip.Slot.ToTag());

			string searchTitle = $"{equip.Slot.GetDisplayName()} {ScreenshotStudio.Resources.Find("Item", "Item")}";

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

		string searchTitle = $"{equip.Slot.GetDisplayName()} {ScreenshotStudio.Resources.Find("Dye", "Dye")}";

		QuickSearch.Show<Stain>(
			sender,
			searchTitle,
			defaultTags,
			equip.Stain1,
			(stain, isFinal) =>
			{
				if (dyeChanel == 0)
				{
					equip.Stain1 = stain;
				}
				else
				{
					equip.Stain2 = stain;
				}
			});
	}
}

public class ItemEquipViewModel : ViewModel
{
	public readonly ItemSlots Slot;
	private readonly GearWindow window;
	private Item? item;
	private Stain? stain1;
	private Stain? stain2;

	public ItemEquipViewModel(ItemSlots slot, GearWindow window)
	{
		this.window = window;
		this.Slot = slot;
	}

	public bool HasValidTarget => this.window.HasValidTarget;

	[AutoNotify]
	public ushort Set
	{
		get => 0;
		set { }
	}

	[AutoNotify]
	public ushort Base
	{
		get => this.HasValidTarget ? this.ItemEquip.Base : (ushort)0;
		set
		{
			this.ItemEquip.Base = value;
			this.ApplyChangeItem();
		}
	}

	[AutoNotify]
	public byte Variant
	{
		get => this.HasValidTarget ? this.ItemEquip.Variant : (byte)0;
		set
		{
			this.ItemEquip.Variant = value;
			this.ApplyChangeItem();
		}
	}

	[AutoNotify]
	public byte Dye1
	{
		get => this.HasValidTarget ? this.ItemEquip.Dye1 : (byte)0;
		set
		{
			this.ItemEquip.Dye1 = value;
			this.ApplyChangeItem();
		}
	}

	[AutoNotify]
	public byte Dye2
	{
		get => this.HasValidTarget ? this.ItemEquip.Dye2 : (byte)0;
		set
		{
			this.ItemEquip.Dye2 = value;
			this.ApplyChangeItem();
		}
	}

	[AutoNotify]
	public Item? Item
	{
		get
		{
			if (!this.HasValidTarget)
				return null;

			if (this.item == null || !this.item.IsItemEquip(this.ItemEquip))
			{
				this.item = GameDataService.Items?.Find(this.Slot, this.Set, this.Base, this.Variant);
			}

			return this.item;
		}

		set
		{
			this.item = value;

			if (this.item != null)
			{
				this.Base = this.item.ModelBase;
				this.Variant = (byte)this.item.ModelVariant;
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

			if (this.stain1 == null || this.stain1.RowId != this.Dye1)
			{
				this.stain1 = GameDataService.GetRow<Stain>(this.Dye1);
			}

			return this.stain1;
		}

		set
		{
			this.stain1 = value;

			if (this.stain1 != null)
			{
				this.Dye1 = (byte)this.stain1.RowId;
			}
		}
	}

	[AutoNotify]
	public Stain? Stain2
	{
		get
		{
			if (!this.HasValidTarget)
				return null;

			if (this.stain2 == null || this.stain2.RowId != this.Dye2)
			{
				this.stain2 = GameDataService.GetRow<Stain>(this.Dye2);
			}

			return this.stain2;
		}

		set
		{
			this.stain2 = value;

			if (this.stain2 != null)
			{
				this.Dye2 = (byte)this.stain2.RowId;
			}
		}
	}

	protected unsafe ref Equipment Equipment => ref this.window.Actor->DrawData.Equipment;

	protected ref ItemEquip ItemEquip
	{
		get
		{
			switch (this.Slot)
			{
				case ItemSlots.MainHand:
				case ItemSlots.OffHand:
				case ItemSlots.Head: return ref this.Equipment.Head;
				case ItemSlots.Chest: return ref this.Equipment.Chest;
				case ItemSlots.Hands: return ref this.Equipment.Hands;
				case ItemSlots.Legs: return ref this.Equipment.Legs;
				case ItemSlots.Feet: return ref this.Equipment.Feet;
				case ItemSlots.Earring: return ref this.Equipment.Earring;
				case ItemSlots.Necklace: return ref this.Equipment.Necklace;
				case ItemSlots.Bracelet: return ref this.Equipment.Bracelet;
				case ItemSlots.RingLeft: return ref this.Equipment.RingLeft;
				case ItemSlots.RingRight: return ref this.Equipment.RingRight;
			}

			return ref this.Equipment.Head;
		}
	}

	public override bool ShouldTickAutoProperties() => this.window.ShouldTickAutoProperties();

	public unsafe void ApplyChangeItem()
	{
		ActorDrawDataExtensions.ChangeEquip(&this.window.Actor->DrawData, this.Slot, this.ItemEquip, true);
	}
}