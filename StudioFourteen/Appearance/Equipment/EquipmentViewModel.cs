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

namespace StudioFourteen.Appearance.Equipment;

using FFXIVClientStructs.FFXIV.Client.Game.Character;
using StudioFourteen.Mvm;
using WpfUtils.Extensions;

using static FFXIVClientStructs.FFXIV.Client.Game.Character.DrawDataContainer;

public class EquipmentViewModel : ViewModel
{
	public EquipmentViewModel()
	{
		this.Weapons.Add(new WeaponSlotViewModel(WeaponSlot.MainHand));
		this.Weapons.Add(new WeaponSlotViewModel(WeaponSlot.OffHand));

		this.Equipment.Add(new EquipmentSlotViewModel(EquipmentSlot.Head));
		this.Equipment.Add(new EquipmentSlotViewModel(EquipmentSlot.Body));
		this.Equipment.Add(new EquipmentSlotViewModel(EquipmentSlot.Hands));
		this.Equipment.Add(new EquipmentSlotViewModel(EquipmentSlot.Legs));
		this.Equipment.Add(new EquipmentSlotViewModel(EquipmentSlot.Feet));

		this.Accessories.Add(new EquipmentSlotViewModel(EquipmentSlot.Ears));
		this.Accessories.Add(new EquipmentSlotViewModel(EquipmentSlot.Neck));
		this.Accessories.Add(new EquipmentSlotViewModel(EquipmentSlot.Wrists));
		this.Accessories.Add(new EquipmentSlotViewModel(EquipmentSlot.RFinger));
		this.Accessories.Add(new EquipmentSlotViewModel(EquipmentSlot.LFinger));
	}

	public FastObservableCollection<GearViewModelBase> Weapons { get; init; } = new();
	public FastObservableCollection<GearViewModelBase> Equipment { get; init; } = new();
	public FastObservableCollection<GearViewModelBase> Accessories { get; init; } = new();
	public FastObservableCollection<GearViewModelBase> Fashion { get; init; } = new();

	public void OnTargetChanged()
	{
	}

	public unsafe void OnGameTick(Character* pCharacter)
	{
		foreach (GearViewModelBase gearViewModel in this.Weapons)
		{
			gearViewModel.OnFrameworkUpdate(pCharacter);
		}

		foreach (GearViewModelBase gearViewModel in this.Equipment)
		{
			gearViewModel.OnFrameworkUpdate(pCharacter);
		}

		foreach (GearViewModelBase gearViewModel in this.Accessories)
		{
			gearViewModel.OnFrameworkUpdate(pCharacter);
		}

		foreach (GearViewModelBase gearViewModel in this.Fashion)
		{
			gearViewModel.OnFrameworkUpdate(pCharacter);
		}
	}
}
