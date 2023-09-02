// © XivTools.
// Licensed under the MIT license.

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
		if (sender is Button btn && btn.DataContext is ItemEquipViewModel equip)
		{
			TagCollection defaultTags = new();
			defaultTags.Add(equip.Slot.ToTag());

			QuickSearch.Show<Item>(
				btn,
				equip.Slot.GetDisplayName(),
				defaultTags,
				equip.Item,
				(item, isFinal) =>
			{
				equip.Item = item;
			});
		}
	}
}

public class ItemEquipViewModel : ViewModel
{
	public readonly ItemSlots Slot;
	private readonly GearWindow window;
	private Item? item;

	public ItemEquipViewModel(ItemSlots slot, GearWindow window)
	{
		this.window = window;
		this.Slot = slot;
	}

	[AutoNotify]
	public ushort Set
	{
		get => 0;
		set { }
	}

	[AutoNotify]
	public ushort Base
	{
		get => this.ItemEquip.Base;
		set
		{
			this.ItemEquip.Base = value;
			this.ApplyChangeItem();
		}
	}

	[AutoNotify]
	public byte Variant
	{
		get => this.ItemEquip.Variant;
		set
		{
			this.ItemEquip.Variant = value;
			this.ApplyChangeItem();
		}
	}

	[AutoNotify]
	public byte Dye
	{
		get => this.ItemEquip.Dye;
		set => this.ItemEquip.Dye = value;
	}

	[AutoNotify]
	public Item? Item
	{
		get
		{
			if (!this.window.HasValidTarget)
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
		ActorDrawDataExtensions.ChangeEquip(&this.window.Actor->DrawData, this.Slot, this.ItemEquip);
	}
}