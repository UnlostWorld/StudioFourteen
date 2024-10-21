namespace StudioFourteen.Appearance.Equipment;

using DependencyPropertyGenerator;
using StudioFourteen.GameData.Excel;
using StudioFourteen.Library;
using StudioFourteen.Mvm;
using StudioFourteen.Tags;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using static FFXIVClientStructs.FFXIV.Client.Game.Character.DrawDataContainer;

[DependencyProperty<ItemViewModelBase>("Item")]
public partial class EquipmentView : View
{
	public Rect IconRect
	{
		get
		{
			if (this.Item is ItemEquipViewModel equipViewModel)
			{
				switch (equipViewModel.Slot)
				{
					case EquipmentSlot.Head: return new(64, 144, 64, 64);
					case EquipmentSlot.Body: return new(192, 144, 64, 64);
					case EquipmentSlot.Hands: return new(256, 144, 64, 64);
					case EquipmentSlot.Legs: return new(384, 144, 64, 64);
					case EquipmentSlot.Feet: return new(0, 208, 64, 64);
					case EquipmentSlot.Ears: return new(64, 208, 64, 64);
					case EquipmentSlot.Neck: return new(128, 208, 64, 64);
					case EquipmentSlot.Wrists: return new(192, 208, 64, 64);
					case EquipmentSlot.RFinger: return new(256, 208, 64, 64);
					case EquipmentSlot.LFinger: return new(256, 208, 64, 64);
				}
			}
			else if (this.Item is WeaponViewModel weaponViewModel)
			{
				switch (weaponViewModel.Slot)
				{
					case WeaponSlot.MainHand:
					case WeaponSlot.OffHand: return new(64, 144, 64, 64);
				}
			}

			return new(0, 0, 1, 1);
		}
	}

	private unsafe void OnChangeClicked(object sender, RoutedEventArgs e)
	{
		if (this.Item is GearViewModelBase gear)
		{
			gear.Change(this);
		}
	}

	private void OnMouseUp(object sender, MouseButtonEventArgs e)
	{
		if (e.ChangedButton != MouseButton.Middle && e.ChangedButton != MouseButton.Right)
			return;

		if (this.Item is GearViewModelBase gear)
		{
			gear.Clear();
		}
	}

	private void OnChangeDye1Clicked(object sender, RoutedEventArgs e)
	{
		if (this.Item is ItemEquipViewModel equip)
		{
			this.OnChangeDye(sender, equip, 0);
		}
	}

	private void OnChangeDye2Clicked(object sender, RoutedEventArgs e)
	{
		if (this.Item is ItemEquipViewModel equip)
		{
			this.OnChangeDye(sender, equip, 1);
		}
	}

	private void OnChangeDye(object sender, ItemEquipViewModel equip, int dyeChanel)
	{
		TagCollection defaultTags = new();
		defaultTags.Add("Named");

		string searchTitle = $"{equip.Slot} {StudioFourteen.Resources.Find("Dye", "Dye")}";

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
