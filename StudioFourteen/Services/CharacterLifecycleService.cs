//// Brio
//// https://github.com/AsgardXIV/Brio/
//// https://github.com/AsgardXIV/Brio/blob/main/Brio/Game/Character/CharacterSpawnService.cs
//// https://github.com/Etheirys/Brio/blob/main/Brio/Game/Core/ObjectMonitorService.cs

namespace StudioFourteen.Services;

using Dalamud.Hooking;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.Event;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FontAwesome.Sharp;
using StudioFourteen.Appearance;
using StudioFourteen.Context;
using StudioFourteen.Mvm;
using StudioFourteen.Plugin;
using StudioFourteen.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using TerraFX.Interop.Windows;
using WpfUtils.Extensions;

public class CharacterLifecycleService : ServiceBase, WorldContextMenu.IProvider
{
	private static readonly List<ushort> CreatedIndexes = new();

	private Hook<CharacterEventDelegate>? characterInitializeHook;
	private Hook<CharacterEventDelegate>? characterFinalizeHook;

	public delegate void CharacterDelegate(int objectTableIndex);
	private unsafe delegate nint CharacterEventDelegate(Character* character);

	public event CharacterDelegate? CharacterCreated;
	public event CharacterDelegate? CharacterDestroyed;

	[AutoNotify]
	public bool CanSpawn => this.Services.GroupPose.IsGroupPosing;

	public override async Task Initialize()
	{
		await base.Initialize();

		if (DalamudServices.ClientState != null)
		{
			DalamudServices.ClientState.TerritoryChanged += (s) => CreatedIndexes.Clear();
		}
	}

	public override Task Start()
	{
		WorldContextMenu.AddProvider(this);
		return base.Start();
	}

	public override async Task Stop()
	{
		WorldContextMenu.RemoveProvider(this);
		await base.Stop();
		this.DestroyAllCreated();
	}

	public override Task Tick()
	{
		try
		{
			if (!this.Services.GroupPose.IsGroupPosing && CreatedIndexes.Count > 0)
			{
				this.DestroyAllCreated();
				this.Log.Warning("Left GPose with spawned characters. deleting...");
			}
		}
		catch (Exception ex)
		{
			this.Log.Error(ex, "Error checking group pose state");
		}

		return base.Tick();
	}

	public async Task<int> CreateAsync(ICharacterAppearance? appearance = null)
	{
		return await this.CreateAsync(Vector3.Zero, appearance);
	}

	public async Task<int> CreateAsync(Vector3 position, ICharacterAppearance? appearance = null)
	{
		await Threads.FrameworkThread();

		if (!this.CanSpawn)
			return -1;

		await Threads.FrameworkThread();
		int index = this.Spawn(position);

		if (DalamudServices.ObjectTable != null)
		{
			bool canDraw = false;
			while (!canDraw)
			{
				unsafe
				{
					Character* pCharacter = (Character*)DalamudServices.ObjectTable.GetObjectAddress(index);
					canDraw = pCharacter->CanDraw();
				}

				if (!canDraw)
				{
					await Task.Delay(10);
				}
			}
		}

		if (index != -1 && appearance != null)
		{
			await appearance.Apply(index);
		}

		return index;
	}

	public void Destroy(int objectTableIndex)
	{
		this.DestroyAsync(objectTableIndex).Run();
	}

	public async Task<bool> DestroyAsync(int objectTableIndex)
	{
		await Threads.FrameworkThread();

		if (DalamudServices.ObjectTable == null)
			return false;

		unsafe
		{
			GameObject* character = (GameObject*)DalamudServices.ObjectTable.GetObjectAddress(objectTableIndex);

			ClientObjectManager* com = ClientObjectManager.Instance();
			uint idx = com->GetIndexByObject((GameObject*)character);
			if (idx == 0xFFFFFFFF)
				return false;

			com->DeleteObjectByIndex((ushort)idx, 0);
			return true;
		}
	}

	public unsafe void DestroyAllCreated()
	{
		List<ushort> indexes = CreatedIndexes.ToList();
		ClientObjectManager* com = ClientObjectManager.Instance();
		foreach (ushort idx in indexes)
		{
			Character* deletingCharacter = (Character*)com->GetObjectByIndex(idx);
			if (deletingCharacter == null)
			{
				this.Log.Error($"Attempt to delete object by index {idx} was not a character");
				continue;
			}

			Threads.RunOnFrameworkThread(() =>
			{
				this.Log.Information($"Deleting object: {idx} - {deletingCharacter->GetDisplayName()}");
				com->DeleteObjectByIndex(idx, 0);
			});
		}

		CreatedIndexes.Clear();
	}

	public override unsafe void Attach()
	{
		base.Attach();

		this.characterInitializeHook = InteropService.HookFromSignature<CharacterEventDelegate>("E8 ?? ?? ?? ?? 8D 57 ?? C6 83", this.CharacterInitializeDetour);
		this.characterInitializeHook?.Enable();

		this.characterFinalizeHook = InteropService.HookFromSignature<CharacterEventDelegate>("48 89 5C 24 ?? 48 89 74 24 ?? 57 48 83 EC ?? 48 8D 05 ?? ?? ?? ?? 48 8B D9 48 89 01 48 8D 05 ?? ?? ?? ?? 48 89 81 ?? ?? ?? ?? 48 81 C1", this.CharacterFinalizeDetour);
		this.characterFinalizeHook?.Enable();
	}

	public override void Detach()
	{
		base.Detach();

		this.characterInitializeHook?.Dispose();
		this.characterFinalizeHook?.Dispose();
	}

	public Task GetMenu(WorldContextMenu menu)
	{
		if (menu.IsObject)
		{
			menu.AddIcon(IconChar.TrashCan, "Destroy Character", true, (h) => this.DestroyAsync(h.ObjectTableIndex));
		}
		else
		{
			menu.AddIcon(IconChar.UserPlus, "Create Character", true, (h) => this.CreateAsync(h.Position));
		}

		return Task.CompletedTask;
	}

	private unsafe nint CharacterInitializeDetour(Character* character)
	{
		if (this.characterInitializeHook == null)
			return 0;

		nint result = this.characterInitializeHook.Original.Invoke(character);

		this.CharacterCreated?.Invoke(character->ObjectIndex);

		return result;
	}

	private unsafe nint CharacterFinalizeDetour(Character* character)
	{
		ushort objectTableIndex = character->ObjectIndex;

		uint idx = ClientObjectManager.Instance()->GetIndexByObject((GameObject*)character);
		if (idx < ushort.MaxValue && CreatedIndexes.Contains((ushort)idx))
		{
			CreatedIndexes.Remove((ushort)idx);
			this.Log.Information($"created character was destroyed: {idx}");
		}

		if (this.characterFinalizeHook == null)
			return 0;

		nint result = this.characterFinalizeHook.Original.Invoke(character);

		this.CharacterDestroyed?.Invoke(objectTableIndex);

		return result;
	}

	private unsafe int Spawn(Vector3 position)
	{
		if (DalamudServices.ClientState?.LocalPlayer == null)
			return -1;

		Threads.VerifyFrameworkThread();

		Character* player = (Character*)DalamudServices.ClientState.LocalPlayer.Address;

		if (player == null)
			return -1;

		ClientObjectManager* com = ClientObjectManager.Instance();
		uint idCheck = com->CreateBattleCharacter();
		if (idCheck == 0xffffffff)
			return -1;

		ushort spawnedCharacterId = (ushort)idCheck;

		Character* pSpawned = (Character*)com->GetObjectByIndex(spawnedCharacterId);
		if (pSpawned == null)
			return -1;

		EventGPoseController* gposeController = &EventFramework.Instance()->EventSceneModule.EventGPoseController;
		gposeController->AddCharacterToGPose(pSpawned); // This is safe even if the list is full. The game will also cleanup for us.

		CharacterSetupContainer.CopyFlags flags = CharacterSetupContainer.CopyFlags.WeaponHiding | CharacterSetupContainer.CopyFlags.Position;
		pSpawned->CharacterSetup.CopyFromCharacter(player, flags);

		*((sbyte*)pSpawned + 0x95) &= ~2; // Disable selection just incase this somehow leaks out of GPose

		if (position == Vector3.Zero)
			position = player->GameObject.Position;

		pSpawned->GameObject.Position = position;
		pSpawned->GameObject.DefaultPosition = position;
		pSpawned->GameObject.Rotation = player->GameObject.Rotation;
		pSpawned->GameObject.DefaultRotation = player->GameObject.Rotation;

		// Generate a unique name.
		// This name must pass penumbra's naming validation to allow mcdf loading to wor.
		// Generate the name "Studio S" + the object table index as letters a = 0, b = 1, etc.
		char[] str = pSpawned->ObjectIndex.ToString("D3").ToArray();
		for (int j = 0; j < str.Length; j++)
		{
			str[j] = (char)(str[j] + ('a' - '0'));
		}

		string name = $"Studio S{new string(str)}";

		for (int x = 0; x < name.Length; x++)
		{
			pSpawned->GameObject.Name[x] = (byte)name[x];
		}

		pSpawned->GameObject.Name[name.Length] = 0;

		pSpawned->GameObject.DisableDraw();
		pSpawned->CharacterSetup.CopyFromCharacter(pSpawned, CharacterSetupContainer.CopyFlags.None);
		pSpawned->GameObject.EnableDraw();

		CreatedIndexes.Add(spawnedCharacterId);

		this.Log.Information($"Spawning character {pSpawned->NameString} with id {spawnedCharacterId} and index {pSpawned->ObjectIndex}");

		return pSpawned->ObjectIndex;
	}
}
