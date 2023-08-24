// © XivTools.
// Licensed under the MIT license.

namespace ScreenshotStudio.Studio;

using ScreenshotStudio.GameData;
using ScreenshotStudio.Services;
using ScreenshotStudio.Structs;
using ScreenshotStudio.Windows;

public partial class GearWindow : PanelWindow
{
	[AutoNotify]
	public unsafe float ModelHeight
	{
		get => this.Target->Model->Height;
		set => this.Target->Model->Height = value;
	}

	public ItemEquipViewModel Head { get; init; } = new(EquipSlots.Head);
	public ItemEquipViewModel Chest { get; init; } = new(EquipSlots.Chest);
	public ItemEquipViewModel Hands { get; init; } = new(EquipSlots.Hands);
	public ItemEquipViewModel Legs { get; init; } = new(EquipSlots.Legs);
	public ItemEquipViewModel Feet { get; init; } = new(EquipSlots.Feet);
	public ItemEquipViewModel Earring { get; init; } = new(EquipSlots.Earring);
	public ItemEquipViewModel Necklace { get; init; } = new(EquipSlots.Necklace);
	public ItemEquipViewModel Bracelet { get; init; } = new(EquipSlots.Bracelet);
	public ItemEquipViewModel RingRight { get; init; } = new(EquipSlots.RingRight);
	public ItemEquipViewModel RingLeft { get; init; } = new(EquipSlots.RingLeft);
}

public class ItemEquipViewModel : ViewModel
{
	private readonly EquipSlots slot;
	private Item? item;

	public ItemEquipViewModel(EquipSlots slot)
	{
		this.slot = slot;
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
		set => this.ItemEquip.Base = value;
	}

	[AutoNotify]
	public byte Variant
	{
		get => this.ItemEquip.Variant;
		set => this.ItemEquip.Variant = value;
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
			if (this.item == null || !this.item.IsItemEquip(this.ItemEquip))
			{
				this.item = this.Services.Data.Items.Find(this.slot, this.Set, this.Base, this.Variant, false);

				if (this.item == null)
				{
					this.item = new DummyItem(this.slot, this.Set, this.Base, this.Variant);
				}
			}

			return this.item;
		}

		set => this.item = value;
	}

	protected unsafe ref Equipment Equipment => ref this.Services.Targets.GPoseTarget->DrawData.Equipment;

	protected ref ItemEquip ItemEquip
	{
		get
		{
			switch (this.slot)
			{
				case EquipSlots.MainHand:
				case EquipSlots.OffHand:
				case EquipSlots.Head: return ref this.Equipment.Head;
				case EquipSlots.Chest: return ref this.Equipment.Chest;
				case EquipSlots.Hands: return ref this.Equipment.Hands;
				case EquipSlots.Legs: return ref this.Equipment.Legs;
				case EquipSlots.Feet: return ref this.Equipment.Feet;
				case EquipSlots.Earring: return ref this.Equipment.Earring;
				case EquipSlots.Necklace: return ref this.Equipment.Necklace;
				case EquipSlots.Bracelet: return ref this.Equipment.Bracelet;
				case EquipSlots.RingLeft: return ref this.Equipment.RingLeft;
				case EquipSlots.RingRight: return ref this.Equipment.RingRight;
			}

			return ref this.Equipment.Head;
		}
	}

	public class DummyItem : Item
	{
		private readonly ushort modelSet;
		private readonly ushort modelBase;
		private readonly ushort modelVariant;

		public DummyItem(EquipSlots slot, ushort modelSet, ushort modelBase, ushort modelVariant)
		{
			this.modelSet = modelSet;
			this.modelBase = modelBase;
			this.modelVariant = modelVariant;
		}

		public override string DisplayName => "???";

		public override bool IsItemEquip(ItemEquip item)
		{
			return this.modelSet == 0 && this.modelBase == item.Base && this.modelVariant == item.Variant;
		}
	}
}