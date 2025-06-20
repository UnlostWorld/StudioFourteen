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

namespace StudioFourteen.Scene.GameObjects.Characters.DrawData;

using StudioFourteen.Mvm;
using WpfUtils.Extensions;

using static FFXIVClientStructs.FFXIV.Client.Game.Character.DrawDataContainer;

public class DrawDataContainer : ViewModel
{
	public DrawDataContainer()
	{
		this.Weapons.Add(new Weapon(WeaponSlot.MainHand));
		this.Weapons.Add(new Weapon(WeaponSlot.OffHand));

		this.Equipment.Add(new Equipment(EquipmentSlot.Head));
		this.Equipment.Add(new Equipment(EquipmentSlot.Body));
		this.Equipment.Add(new Equipment(EquipmentSlot.Hands));
		this.Equipment.Add(new Equipment(EquipmentSlot.Legs));
		this.Equipment.Add(new Equipment(EquipmentSlot.Feet));

		this.Accessories.Add(new Equipment(EquipmentSlot.Ears));
		this.Accessories.Add(new Equipment(EquipmentSlot.Neck));
		this.Accessories.Add(new Equipment(EquipmentSlot.Wrists));
		this.Accessories.Add(new Equipment(EquipmentSlot.RFinger));
		this.Accessories.Add(new Equipment(EquipmentSlot.LFinger));
	}

	public FastObservableCollection<GearViewModelBase> Weapons { get; init; } = new();
	public FastObservableCollection<GearViewModelBase> Equipment { get; init; } = new();
	public FastObservableCollection<GearViewModelBase> Accessories { get; init; } = new();
	public FastObservableCollection<GearViewModelBase> Fashion { get; init; } = new();

	public unsafe void OnGameTick(Character character)
	{
		foreach (GearViewModelBase gear in this.Weapons)
		{
			gear.OnGameTick(character);
		}

		foreach (GearViewModelBase gear in this.Equipment)
		{
			gear.OnGameTick(character);
		}

		foreach (GearViewModelBase gear in this.Accessories)
		{
			gear.OnGameTick(character);
		}

		foreach (GearViewModelBase gear in this.Fashion)
		{
			gear.OnGameTick(character);
		}
	}
}
