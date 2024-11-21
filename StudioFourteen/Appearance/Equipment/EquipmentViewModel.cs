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
		this.Gear.Add(new EquipmentSlotViewModel(EquipmentSlot.Head));
		this.Gear.Add(new EquipmentSlotViewModel(EquipmentSlot.Body));
		this.Gear.Add(new EquipmentSlotViewModel(EquipmentSlot.Legs));
		this.Gear.Add(new EquipmentSlotViewModel(EquipmentSlot.Feet));

		this.Gear.Add(new EquipmentSlotViewModel(EquipmentSlot.Ears));
		this.Gear.Add(new EquipmentSlotViewModel(EquipmentSlot.Neck));
		this.Gear.Add(new EquipmentSlotViewModel(EquipmentSlot.Wrists));
		this.Gear.Add(new EquipmentSlotViewModel(EquipmentSlot.RFinger));
		this.Gear.Add(new EquipmentSlotViewModel(EquipmentSlot.LFinger));
	}

	public FastObservableCollection<GearViewModelBase> Gear { get; init; } = new();

	public void OnTargetChanged()
	{
	}

	public unsafe void OnFrameworkUpdate(Character* pCharacter)
	{
		foreach(GearViewModelBase gearViewModel in this.Gear)
		{
			gearViewModel.OnFrameworkUpdate(pCharacter);
		}
	}
}
