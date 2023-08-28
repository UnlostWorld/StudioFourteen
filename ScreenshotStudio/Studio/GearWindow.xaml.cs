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

public partial class GearWindow : PanelWindow
{
	[AutoNotify] public unsafe string? ActorName => this.TargetValid ? this.Target->Name : "Nobody";

	public ItemEquipViewModel Head { get; init; } = new(ItemSlots.Head);
	public ItemEquipViewModel Chest { get; init; } = new(ItemSlots.Chest);
	public ItemEquipViewModel Hands { get; init; } = new(ItemSlots.Hands);
	public ItemEquipViewModel Legs { get; init; } = new(ItemSlots.Legs);
	public ItemEquipViewModel Feet { get; init; } = new(ItemSlots.Feet);
	public ItemEquipViewModel Earring { get; init; } = new(ItemSlots.Earring);
	public ItemEquipViewModel Necklace { get; init; } = new(ItemSlots.Necklace);
	public ItemEquipViewModel Bracelet { get; init; } = new(ItemSlots.Bracelet);
	public ItemEquipViewModel RingRight { get; init; } = new(ItemSlots.RingRight);
	public ItemEquipViewModel RingLeft { get; init; } = new(ItemSlots.RingLeft);

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
				(item) =>
			{
				equip.Item = item;
			});
		}
	}
}

public class ItemEquipViewModel : ViewModel
{
	public readonly ItemSlots Slot;
	private Item? item;

	public ItemEquipViewModel(ItemSlots slot)
	{
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
		get => this.IsValid ? this.ItemEquip.Base : (ushort)0;
		set
		{
			this.ItemEquip.Base = value;
			this.ApplyChangeItem();
		}
	}

	[AutoNotify]
	public byte Variant
	{
		get => this.IsValid ? this.ItemEquip.Variant : (byte)0;
		set
		{
			this.ItemEquip.Variant = value;
			this.ApplyChangeItem();
		}
	}

	[AutoNotify]
	public byte Dye
	{
		get => this.IsValid ? this.ItemEquip.Dye : (byte)0;
		set => this.ItemEquip.Dye = value;
	}

	[AutoNotify]
	public Item? Item
	{
		get
		{
			if (!this.IsValid)
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

	protected unsafe Actor* Actor => this.Services.Targets.GPoseTarget;
	protected unsafe ref Equipment Equipment => ref this.Actor->DrawData.Equipment;

	protected bool IsValid => this.Services.Targets.HasTarget;

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

	public unsafe void ApplyChangeItem()
	{
		ActorDrawDataExtensions.ChangeEquip(&this.Actor->DrawData, this.Slot, this.ItemEquip);
	}
}