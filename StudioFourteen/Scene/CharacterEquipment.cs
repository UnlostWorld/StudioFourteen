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

namespace StudioFourteen.Scene;

using System;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using StudioFourteen.Scene.DrawData;
using StudioFourteen.Services.Library.GameData.Library;
using StudioFourteen.Services.Tick;

using static FFXIVClientStructs.FFXIV.Client.Game.Character.DrawDataContainer;
using XivCharacter = FFXIVClientStructs.FFXIV.Client.Game.Character.Character;

public partial class CharacterEquipment(int objectIndex)
	: CharacterCustomize(objectIndex)
{
	public Weapon MainHand { get; init; } = new Weapon(WeaponSlot.MainHand);
	public Weapon OffHand { get; init; } = new Weapon(WeaponSlot.OffHand);
	public Equipment Head { get; init; } = new Equipment(EquipmentSlot.Head);
	public Equipment Body { get; init; } = new Equipment(EquipmentSlot.Body);
	public Equipment Hands { get; init; } = new Equipment(EquipmentSlot.Hands);
	public Equipment Legs { get; init; } = new Equipment(EquipmentSlot.Legs);
	public Equipment Feet { get; init; } = new Equipment(EquipmentSlot.Feet);
	public Equipment Ears { get; init; } = new Equipment(EquipmentSlot.Ears);
	public Equipment Neck { get; init; } = new Equipment(EquipmentSlot.Neck);
	public Equipment Wrists { get; init; } = new Equipment(EquipmentSlot.Wrists);
	public Equipment RFinger { get; init; } = new Equipment(EquipmentSlot.RFinger);
	public Equipment LFinger { get; init; } = new Equipment(EquipmentSlot.LFinger);

	// ----------------------------------------------------------------------
	// Weapon
	// ----------------------------------------------------------------------
	public unsafe DrawObjectData GetWeapon(WeaponSlot slot)
	{
		TickService.VerifyGameTickThread();
		XivCharacter* pCharacter = this.GetXivCharacter();
		if (pCharacter == null)
			return default;

		return pCharacter->DrawData.Weapon(slot);
	}

	public unsafe void SetWeapon(WeaponSlot slot, WeaponModelId modelId, UpdateSource source)
	{
		TickService.VerifyGameTickThread();

		// We don't know what it does, lets not mess with it.
		if (slot == WeaponSlot.Unk)
			return;

		// Verify that the weapon is valid, or else the character will just vanish.
		if (modelId.Value != 0)
		{
			ItemLibraryEntry? item = Studio.Library.GameData.Items?.Find(slot, modelId);
			if (item == null)
			{
				Studio.Log.Warning($"Attempt to set invalid {slot} model: {modelId.Id}, {modelId.Type}, {modelId.Variant} to character {this}");
				modelId.Value = 0;
			}
		}

		XivCharacter* pCharacter = this.GetXivCharacter();

		if (source != UpdateSource.Restore && source != UpdateSource.Preview)
			this.BackupAppearance();

		pCharacter->DrawData.LoadWeapon(slot, modelId, 1, 1, 0, 0);
	}

	// ----------------------------------------------------------------------
	// Equipment
	// ----------------------------------------------------------------------
	public unsafe EquipmentModelId GetEquipment(EquipmentSlot slot)
	{
		TickService.VerifyGameTickThread();
		XivCharacter* pCharacter = this.GetXivCharacter();
		return pCharacter->DrawData.Equipment(slot);
	}

	public unsafe void SetEquipment(Span<EquipmentModelId> equipment, UpdateSource source)
	{
		for (int i = 0; i < equipment.Length; i++)
		{
			EquipmentSlot slot = (EquipmentSlot)i;
			this.SetEquipment(slot, equipment[i], source);
		}
	}

	public unsafe void SetEquipment(EquipmentSlot slot, EquipmentModelId item, UpdateSource source)
	{
		TickService.VerifyGameTickThread();
		XivCharacter* pCharacter = this.GetXivCharacter();

		if (source != UpdateSource.Restore && source != UpdateSource.Preview)
			this.BackupAppearance();

		pCharacter->DrawData.LoadEquipment(slot, &item, true);
	}

	public override void OnGameTick()
	{
		base.OnGameTick();

		this.MainHand.OnGameTick(this);
		this.OffHand.OnGameTick(this);
		this.Head.OnGameTick(this);
		this.Body.OnGameTick(this);
		this.Hands.OnGameTick(this);
		this.Legs.OnGameTick(this);
		this.Feet.OnGameTick(this);
		this.Ears.OnGameTick(this);
		this.Neck.OnGameTick(this);
		this.Wrists.OnGameTick(this);
		this.RFinger.OnGameTick(this);
		this.LFinger.OnGameTick(this);
	}
}
