// .                    @@             _____ _______ _    _ _____ _____ ____
//          @       @@@@@             / ____|__   __| |  | |  __ \_   _/ __ \
//         @@@  @@@@                 | (___    | |  | |  | | |  | || || |  | |
//         @@@@@@@@@  @    @          \___ \   | |  | |  | | |  | || || |  | |
//        @@@@       @@@@@@@          ____) |  | |  | |__| | |__| || || |__| |
//    @@@@@             @@@          |_____/   |_|   \____/|_____/_____\____/
//     @@@      @@@      @@        ___     _    _   _  __   _____  ___  ___  _  _
//      @@    @@@@@@@    @@       |  _|  / _ \ | | | || _ \|_   _|| __|| __|| \| |
//      @@    @@@@@@@    @   @    | __| | (_) || |_| ||   /  | |  | _| | _| | .` |
//    @@@@      @@@      @@@@     |_|    \___/  \___/ |_|_\  |_|  |___||___||_|\_|
//     @@@@             @@@        https://github.com/UnlostWorld/StudioFourteen
//       @@@@@      @@@@@
//        @@@@@@@@@@@@@@                This software is licensed under the
//            @@@@  @                  GNU AFFERO GENERAL PUBLIC LICENSE v3

namespace StudioFourteen.Appearance;

using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using StudioFourteen.Appearance.Customize;
using StudioFourteen.Appearance.Equipment;
using StudioFourteen.Files;
using StudioFourteen.Library;
using StudioFourteen.Mvm;
using StudioFourteen.Panels;
using System.Windows;
using WpfUtils.Extensions;

public partial class CharacterPanel : CharacterPanelBase
{
	public CharacterPanel()
	{
		this.Customize = new(this);
		this.Equipment = new();
	}

	public CustomizeViewModel Customize { get; init; }
	public EquipmentViewModel Equipment { get; init; }

	[AutoNotify] public bool UseTwoColumns => this.ActualWidth > 650;

	[AutoNotify] public unsafe bool CanRevert => this.Services.CharacterAppearance.CanRestore(this.TargetObjectIndex);
	[AutoNotify] public string ExportAppearanceToolTipText => string.Format(StudioFourteen.Resources.Find("LOC_Save_ExportAppearanceToolTip", string.Empty), this.CharacterName);

	[AutoNotify]
	public int SelectedTab
	{
		get => this.GetPersistence<int>();
		set => this.SetPersistence(value);
	}

	/*[AutoNotify] public WeaponViewModel MainHand { get; init; } = new(DrawDataContainer.WeaponSlot.MainHand);
	[AutoNotify] public WeaponViewModel OffHand { get; init; } = new(DrawDataContainer.WeaponSlot.OffHand);

	[AutoNotify] public ItemEquipViewModel Head { get; init; } = new(DrawDataContainer.EquipmentSlot.Head);
	[AutoNotify] public ItemEquipViewModel Chest { get; init; } = new(DrawDataContainer.EquipmentSlot.Body);
	[AutoNotify] public ItemEquipViewModel Hands { get; init; } = new(DrawDataContainer.EquipmentSlot.Hands);
	[AutoNotify] public ItemEquipViewModel Legs { get; init; } = new(DrawDataContainer.EquipmentSlot.Legs);
	[AutoNotify] public ItemEquipViewModel Feet { get; init; } = new(DrawDataContainer.EquipmentSlot.Feet);
	[AutoNotify] public ItemEquipViewModel Earring { get; init; } = new(DrawDataContainer.EquipmentSlot.Ears);
	[AutoNotify] public ItemEquipViewModel Necklace { get; init; } = new(DrawDataContainer.EquipmentSlot.Neck);
	[AutoNotify] public ItemEquipViewModel Bracelet { get; init; } = new(DrawDataContainer.EquipmentSlot.Wrists);
	[AutoNotify] public ItemEquipViewModel RingRight { get; init; } = new(DrawDataContainer.EquipmentSlot.RFinger);
	[AutoNotify] public ItemEquipViewModel RingLeft { get; init; } = new(DrawDataContainer.EquipmentSlot.LFinger);

	public AccessoryViewModel Glasses { get; init; } = new(AccessorySlots.Glasses);
	public OrnamentViewModel Ornament { get; init; } = new();*/

	/*
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
					case WeaponSlot.OffHand: return new(0, 144, 64, 64);
				}
			}

			return new(0, 0, 1, 1);
		}
	}*/

	protected unsafe override void OnFrameworkUpdate(IFramework framework)
	{
		base.OnFrameworkUpdate(framework);

		Character* pTarget = this.Services.Target.GetTarget();
		if (pTarget == null)
			return;

		this.Customize.OnFrameworkUpdate(pTarget);
		this.Equipment.OnFrameworkUpdate(pTarget);
	}

	protected override void OnTargetChanged()
	{
		base.OnTargetChanged();

		this.Customize.OnTargetChanged();
		this.Equipment.OnTargetChanged();
	}

	private unsafe void OnRevertClicked(object sender, RoutedEventArgs e)
	{
		this.Services.CharacterAppearance.Restore(this.TargetObjectIndex).Run();
	}

	private void OnImportClicked(object sender, RoutedEventArgs e)
	{
		LibraryWindow.Open();
	}

	private void OnExportClicked(object sender, RoutedEventArgs e)
	{
		AppearanceFile file = new();
		file.Read(this.TargetObjectIndex);
		this.Services.Files.SaveFile(file, $"{this.CharacterName}'s Appearance");
	}
}
