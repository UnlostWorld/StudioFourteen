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

using Dalamud.Game.ClientState.Objects.Enums;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using Lumina.Excel.Sheets;
using StudioFourteen.Context;
using StudioFourteen.Files;
using StudioFourteen.GameData.Library;
using StudioFourteen.Interop;
using StudioFourteen.Services;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

using static FFXIVClientStructs.FFXIV.Client.Game.Character.CharacterExtensions;
using static FFXIVClientStructs.FFXIV.Client.Game.Character.DrawDataContainer;

public class CharacterAppearanceService : ServiceBase
{
	private readonly GroupPoseCharactersLibrarySource provider = new();
	private readonly ConcurrentDictionary<int, CharacterBackupAppearance> backup = new();

	public delegate void AppearanceChangedDelegate(int objectTableIndex);
	public event AppearanceChangedDelegate? OnAppearanceChanged;

	public override Task Start()
	{
		this.Services.GroupPose.StateChanged += this.OnGroupPoseStateChange;
		this.Services.Library.AddSource(this.provider);

		if (this.Services.GroupPose.IsGroupPosing)
		{
			this.provider.OnEnterGroupPose();
		}

		return base.Start();
	}

	public override Task Stop()
	{
		this.Services.GroupPose.StateChanged -= this.OnGroupPoseStateChange;
		return base.Stop();
	}

	public override void Attach()
	{
		base.Attach();

		Hooks.EnforceKind.Enable(this.EnforceKindRestrictionsDetour);
	}

	public override void Detach()
	{
		base.Detach();

		Hooks.EnforceKind.Disable();
	}

	public unsafe bool CanRestore(Character* character)
	{
		if (character == null)
			return false;

		ushort index = character->GameObject.ObjectIndex;
		return this.CanRestore(index);
	}

	public unsafe bool CanRestore(int objectTableIndex)
	{
		return this.backup.ContainsKey(objectTableIndex);
	}

	public unsafe void Backup(Character character)
	{
		ushort index = character.GameObject.ObjectIndex;

		if (this.backup.ContainsKey(index))
			return;

		this.backup.TryAdd(index, new(character));
	}

	public unsafe void Backup(Character* character)
	{
		ushort index = character->GameObject.ObjectIndex;

		if (this.backup.ContainsKey(index))
			return;

		this.backup.TryAdd(index, new(character));
	}

	/*public async Task Restore(Character* character)
	{
		ushort index = character->GameObject.ObjectIndex;
		this.Restore(index);
	}*/

	public async Task Restore(int objectTableIndex)
	{
		if (!this.backup.ContainsKey(objectTableIndex))
			return;

		await this.backup[objectTableIndex].Apply(objectTableIndex, UpdateSource.Restore);
		this.backup.TryRemove(objectTableIndex, out var _);
	}

	public async Task Save(int objectTableIndex)
	{
		await TickService.GameTick();

		string name = $"#{objectTableIndex}";
		unsafe
		{
			Character* pCharacter = this.Services.GameObjects.Get<Character>(objectTableIndex);
			name = pCharacter->GetDisplayName();
		}

		AppearanceFile file = new();
		await file.Read(objectTableIndex);
		this.Services.Files.SaveFile(file, $"{name}'s Appearance");
	}

	public unsafe void SetModelCharaId(int objectTableIndex, ModelChara modelChara, UpdateSource source)
	{
		this.SetModelCharaId(objectTableIndex, (int)modelChara.RowId, source);
	}

	public unsafe void SetModelCharaId(int objectTableIndex, int modelCharaId, UpdateSource source)
	{
		TickService.VerifyGameTickThread();

		Character* pCharacter = this.Services.GameObjects.Get<Character>(objectTableIndex);

		if (pCharacter->ModelContainer.ModelCharaId == modelCharaId)
			return;

		if (source != UpdateSource.Restore && source != UpdateSource.Preview)
			this.Backup(pCharacter);

		pCharacter->ModelContainer.ModelCharaId = modelCharaId;

		this.Services.Redraw.Redraw(objectTableIndex);
		this.OnAppearanceChanged?.Invoke(objectTableIndex);
	}

	public unsafe void SetCustomizeValue(int objectTableIndex, CustomizeIndex index, byte value, UpdateSource source)
	{
		TickService.VerifyGameTickThread();

		Character* pCharacter = this.Services.GameObjects.Get<Character>(objectTableIndex);

		byte oldValue = pCharacter->DrawData.CustomizeData.GetValue(index);
		if (oldValue == value)
			return;

		if (source != UpdateSource.Restore && source != UpdateSource.Preview)
			this.Backup(pCharacter);

		pCharacter->DrawData.CustomizeData.SetValue(index, value);

		if (index == CustomizeIndex.Race
			|| index == CustomizeIndex.Tribe
			|| index == CustomizeIndex.ModelType
			|| index == CustomizeIndex.Gender)
		{
			this.Services.Redraw.Redraw(objectTableIndex);
		}

		this.UpdateCustomize(objectTableIndex, null, source);
	}

	public unsafe void SetWeapon(int objectTableIndex, WeaponSlot slot, WeaponModelId modelId, UpdateSource source)
	{
		TickService.VerifyGameTickThread();

		// We don't know what it does, lets not mess with it.
		if (slot == WeaponSlot.Unk)
			return;

		// Verify that the weapon is valid, or else the character will just vanish.
		if (modelId.Value != 0)
		{
			ItemLibraryEntry? item = this.Services.GameData.Items?.Find(slot, modelId);
			if (item == null)
			{
				this.Log.Warning($"Attempt to set invalid {slot} model: {modelId.Id}, {modelId.Type}, {modelId.Variant} to character {objectTableIndex}");
				modelId.Value = 0;
			}
		}

		Character* pCharacter = this.Services.GameObjects.Get<Character>(objectTableIndex);

		if (source != UpdateSource.Restore && source != UpdateSource.Preview)
			this.Backup(pCharacter);

		pCharacter->DrawData.LoadWeapon(slot, modelId, 1, 1, 0, 0);
		this.OnAppearanceChanged?.Invoke(objectTableIndex);
	}

	public unsafe void SetEquipment(int objectTableIndex, Span<EquipmentModelId> equipment, UpdateSource source)
	{
		for (int i = 0; i < equipment.Length; i++)
		{
			EquipmentSlot slot = (EquipmentSlot)i;
			this.SetEquipment(objectTableIndex, slot, equipment[i], source);
		}
	}

	public unsafe void SetEquipment(int objectTableIndex, EquipmentSlot slot, EquipmentModelId item, UpdateSource source)
	{
		TickService.VerifyGameTickThread();

		Character* pCharacter = this.Services.GameObjects.Get<Character>(objectTableIndex);

		if (source != UpdateSource.Restore && source != UpdateSource.Preview)
			this.Backup(pCharacter);

		pCharacter->DrawData.LoadEquipment(slot, &item, true);
		this.OnAppearanceChanged?.Invoke(objectTableIndex);
	}

	public unsafe void SetCustomize(int objectTableIndex, CustomizeData customize, UpdateSource source)
	{
		Character* pCharacter = this.Services.GameObjects.Get<Character>(objectTableIndex);

		if (pCharacter->DrawData.CustomizeData[(int)CustomizeIndex.Race] != customize[(int)CustomizeIndex.Race]
			|| pCharacter->DrawData.CustomizeData[(int)CustomizeIndex.Tribe] != customize[(int)CustomizeIndex.Tribe]
			|| pCharacter->DrawData.CustomizeData[(int)CustomizeIndex.ModelType] != customize[(int)CustomizeIndex.ModelType])
		{
			this.Services.Redraw.Redraw(objectTableIndex);
		}

		this.UpdateCustomize(objectTableIndex, customize, source);
	}

	private unsafe void UpdateCustomize(int objectTableIndex, CustomizeData? customize, UpdateSource source)
	{
		TickService.VerifyGameTickThread();

		Character* pCharacter = this.Services.GameObjects.Get<Character>(objectTableIndex);

		if (source != UpdateSource.Restore)
			this.Backup(pCharacter);

		CustomizeData* custom = &pCharacter->DrawData.CustomizeData;

		if (customize != null)
			custom->Import(customize.Value);

		bool didLoad = ((Human*)pCharacter->DrawObject)->UpdateDrawData((byte*)custom, true);
		if (!didLoad)
		{
			this.Services.Redraw.Redraw(objectTableIndex);
		}

		this.OnAppearanceChanged?.Invoke(objectTableIndex);
	}

	private void OnGroupPoseStateChange(bool newState)
	{
		if (newState)
		{
			this.provider.OnEnterGroupPose();
		}
	}

	private byte EnforceKindRestrictionsDetour(nint a1, nint a2)
	{
		// always allow npc values.
		////return this.enforceKindRestrictionsHook.Original(a1, a2);
		return 0;
	}
}
