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
using Dalamud.Hooking;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using FontAwesome.Sharp;
using Lumina.Excel.Sheets;
using StudioFourteen.Context;
using StudioFourteen.Files;
using StudioFourteen.Plugin;
using StudioFourteen.Services;
using StudioFourteen.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Media.TextFormatting;

using static FFXIVClientStructs.FFXIV.Client.Game.Character.CharacterExtensions;
using static FFXIVClientStructs.FFXIV.Client.Game.Character.DrawDataContainer;

public class CharacterAppearanceService : ServiceBase, WorldContextMenu.IProvider
{
	private readonly GroupPoseCharactersLibrarySource provider = new();
	private readonly ConcurrentDictionary<int, CharacterBackupAppearance> backup = new();
	private readonly HashSet<int> pendingRedraws = new();

	private Hook<EnforceKindRestrictionsDelegate>? enforceKindRestrictionsHook;

	private delegate byte EnforceKindRestrictionsDelegate(nint a1, nint a2);

	public override Task Start()
	{
		this.Services.GroupPose.StateChanged += this.OnGroupPoseStateChange;
		this.Services.Library.AddSource(this.provider);

		if (this.Services.GroupPose.IsGroupPosing)
		{
			this.provider.OnEnterGroupPose();
		}

		WorldContextMenu.AddProvider(this);

		return base.Start();
	}

	public override Task Stop()
	{
		WorldContextMenu.RemoveProvider(this);
		this.Services.GroupPose.StateChanged -= this.OnGroupPoseStateChange;
		return base.Stop();
	}

	public override void Attach()
	{
		base.Attach();

		this.enforceKindRestrictionsHook = InteropService.HookFromSignature<EnforceKindRestrictionsDelegate>("E8 ?? ?? ?? ?? 41 B0 ?? 48 8B D6", this.EnforceKindRestrictionsDetour);
		this.enforceKindRestrictionsHook?.Enable();
	}

	public override void Detach()
	{
		base.Detach();

		this.enforceKindRestrictionsHook?.Dispose();
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

		await this.backup[objectTableIndex].Apply(objectTableIndex, CharacterExtensions.UpdateSource.Restore);
		this.backup.TryRemove(objectTableIndex, out var _);
	}

	public async Task Save(int objectTableIndex)
	{
		await Threads.FrameworkThread();

		if (DalamudServices.ObjectTable == null)
			return;

		string name = $"#{objectTableIndex}";
		unsafe
		{
			Character* pCharacter = (Character*)DalamudServices.ObjectTable.GetObjectAddress(objectTableIndex);
			name = pCharacter->GetDisplayName();
		}

		AppearanceFile file = new();
		await file.Read(objectTableIndex);
		this.Services.Files.SaveFile(file, $"{name}'s Appearance");
	}

	Task WorldContextMenu.IProvider.GetMenu(WorldContextMenu menu)
	{
		if (menu.IsObject)
		{
			bool hasBackup = this.backup.ContainsKey(menu.ObjectTableIndex);
			menu.Add(IconChar.RotateLeft, "Restore Appearance", hasBackup, (h) => this.Restore(h.ObjectTableIndex));
			menu.Add(IconChar.Save, "Export Appearance", true, (h) => this.Save(h.ObjectTableIndex));
		}

		return Task.CompletedTask;
	}

	public unsafe void SetModelCharaId(int objectTableIndex, ModelChara modelChara, UpdateSource source)
	{
		this.SetModelCharaId(objectTableIndex, (int)modelChara.RowId, source);
	}

	public unsafe void SetModelCharaId(int objectTableIndex, int modelCharaId, UpdateSource source)
	{
		Threads.VerifyFrameworkThread();

		Character* pCharacter = this.Services.Target.GetCharacter(objectTableIndex);

		if (pCharacter->ModelContainer.ModelCharaId == modelCharaId)
			return;

		if (source != UpdateSource.Restore)
			this.Backup(pCharacter);

		pCharacter->ModelContainer.ModelCharaId = modelCharaId;

		this.pendingRedraws.Add(pCharacter->ObjectIndex);
	}

	public unsafe void SetCustomizeValue(int objectTableIndex, CustomizeIndex index, byte value, UpdateSource source)
	{
		Threads.VerifyFrameworkThread();

		Character* pCharacter = this.Services.Target.GetCharacter(objectTableIndex);

		byte oldValue = pCharacter->DrawData.CustomizeData.GetValue(index);
		if (oldValue == value)
			return;

		if (source != UpdateSource.Restore)
			this.Backup(pCharacter);

		pCharacter->DrawData.CustomizeData.SetValue(index, value);

		if (index == CustomizeIndex.Race
			|| index == CustomizeIndex.Tribe
			|| index == CustomizeIndex.ModelType
			|| index == CustomizeIndex.Gender)
		{
			this.pendingRedraws.Add(pCharacter->ObjectIndex);
		}

		this.UpdateCustomize(objectTableIndex, null, source);
	}

	public unsafe void SetWeapon(int objectTableIndex, WeaponSlot slot, WeaponModelId item, UpdateSource source)
	{
		Threads.VerifyFrameworkThread();

		Character* pCharacter = this.Services.Target.GetCharacter(objectTableIndex);

		if (source != UpdateSource.Restore)
			this.Backup(pCharacter);

		pCharacter->DrawData.LoadWeapon(slot, item, 1, 1, 0, 0);
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
		Threads.VerifyFrameworkThread();

		Character* pCharacter = this.Services.Target.GetCharacter(objectTableIndex);

		if (source != UpdateSource.Restore)
			this.Backup(pCharacter);

		pCharacter->DrawData.LoadEquipment(slot, &item, true);
	}

	public unsafe void SetCustomize(int objectTableIndex, CustomizeData customize, UpdateSource source)
	{
		Character* pCharacter = this.Services.Target.GetCharacter(objectTableIndex);

		if (pCharacter->DrawData.CustomizeData[(int)CustomizeIndex.Race] != customize[(int)CustomizeIndex.Race]
			|| pCharacter->DrawData.CustomizeData[(int)CustomizeIndex.Tribe] != customize[(int)CustomizeIndex.Tribe]
			|| pCharacter->DrawData.CustomizeData[(int)CustomizeIndex.ModelType] != customize[(int)CustomizeIndex.ModelType])
		{
			this.pendingRedraws.Add(pCharacter->ObjectIndex);
		}

		this.UpdateCustomize(objectTableIndex, customize, source);
	}

	protected unsafe override void OnFrameworkUpdate(IFramework framework)
	{
		base.OnFrameworkUpdate(framework);

		foreach (int objectTargetId in this.pendingRedraws)
		{
			Character* pCharacter = this.Services.Target.GetCharacter(objectTargetId);
			pCharacter->Redraw();
		}

		this.pendingRedraws.Clear();
	}

	private unsafe void UpdateCustomize(int objectTableIndex, CustomizeData? customize, UpdateSource source)
	{
		Threads.VerifyFrameworkThread();

		Character* pCharacter = this.Services.Target.GetCharacter(objectTableIndex);

		CustomizeData* custom = &pCharacter->DrawData.CustomizeData;

		if (customize != null)
			custom->Import(customize.Value);

		bool didLoad = ((Human*)pCharacter->DrawObject)->UpdateDrawData((byte*)custom, true);

		if (!didLoad)
		{
			this.pendingRedraws.Add(pCharacter->ObjectIndex);
		}
	}

	private void OnGroupPoseStateChange(bool newState)
	{
		this.Log.Information($"GPose {newState}");

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
