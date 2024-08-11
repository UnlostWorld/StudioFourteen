//// Brio
//// https://github.com/AsgardXIV/Brio/
//// https://github.com/AsgardXIV/Brio/blob/main/Brio/Game/Character/CharacterSpawnService.cs
//// https://github.com/Etheirys/Brio/blob/main/Brio/Game/Core/ObjectMonitorService.cs

namespace ScreenshotStudio.Services;

using Dalamud.Hooking;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.Event;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using ScreenshotStudio;
using ScreenshotStudio.Plugin;
using ScreenshotStudio.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TerraFX.Interop.Windows;

public class CharacterLifecycleService : ServiceBase
{
	private static readonly List<ushort> CreatedIndexes = new();

	private Hook<CharacterEventDelegate>? characterInitializeHook;
	private Hook<CharacterEventDelegate>? characterFinalizeHook;

	public delegate void CharacterDelegate(int objectTableIndex);
	private unsafe delegate nint CharacterEventDelegate(Character* character);

	public event CharacterDelegate? CharacterCreated;
	public event CharacterDelegate? CharacterDestroyed;

	public bool CanSpawn => this.Services.GroupPose.IsGroupPosing;

	public override async Task Initialize()
	{
		await base.Initialize();

		if (DalamudServices.ClientState != null)
		{
			DalamudServices.ClientState.TerritoryChanged += (s) => CreatedIndexes.Clear();
		}
	}

	public override async Task Start()
	{
		await base.Start();
		this.Attach();
	}

	public override async Task Stop()
	{
		await base.Stop();
		this.Detach();
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

	public async Task<int> Create(ICharacterAppearance? appearance = null)
	{
		Threads.VerifyFrameworkThread();

		if (!this.CanSpawn)
			return -1;

		string name = "Character";
		if (appearance != null && !string.IsNullOrEmpty(appearance.Name))
			name = appearance.Name;

		await Threads.FrameworkThread();
		int index = this.Spawn(name);

		if (index != -1 && appearance != null)
		{
			await appearance.Apply(index);
		}

		return index;
	}

	public unsafe bool Destroy(Character* character)
	{
		ClientObjectManager* com = ClientObjectManager.Instance();
		uint idx = com->GetIndexByObject((GameObject*)character);
		if (idx != 0xFFFFFFFF)
		{
			Threads.RunOnFrameworkThread(() =>
			{
				com->DeleteObjectByIndex((ushort)idx, 0);
			});

			return true;
		}

		return false;
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
				this.Log.Information($"Deleting object: {idx} - {deletingCharacter->GetNameAsString()}");
				com->DeleteObjectByIndex(idx, 0);
			});
		}

		CreatedIndexes.Clear();
	}

	private unsafe void Attach()
	{
		this.characterInitializeHook = InteropService.HookFromSignature<CharacterEventDelegate>("E8 ?? ?? ?? ?? 8D 57 ?? C6 83", this.CharacterInitializeDetour);
		this.characterInitializeHook?.Enable();

		this.characterFinalizeHook = InteropService.HookFromSignature<CharacterEventDelegate>("48 89 5C 24 ?? 48 89 74 24 ?? 57 48 83 EC ?? 48 8D 05 ?? ?? ?? ?? 48 8B D9 48 89 01 48 8D 05 ?? ?? ?? ?? 48 89 81 ?? ?? ?? ?? 48 81 C1", this.CharacterFinalizeDetour);
		this.characterFinalizeHook?.Enable();
	}

	private void Detach()
	{
		this.characterInitializeHook?.Dispose();
		this.characterFinalizeHook?.Dispose();
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

	private unsafe int Spawn(string name)
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

		pSpawned->CharacterSetup.CopyFromCharacter(player, CharacterSetupContainer.CopyFlags.None); // We copy the Player as the created character is just blank

		*((sbyte*)pSpawned + 0x95) &= ~2; // Disable selection just incase this somehow leaks out of GPose

		pSpawned->GameObject.Position = player->GameObject.Position;
		pSpawned->GameObject.DefaultPosition = player->GameObject.Position;
		pSpawned->GameObject.Rotation = player->GameObject.Rotation;
		pSpawned->GameObject.DefaultRotation = player->GameObject.Rotation;

		// Set name
		for (int x = 0; x < name.Length; x++)
		{
			pSpawned->GameObject.Name[x] = (byte)name[x];
		}

		pSpawned->GameObject.Name[name.Length] = 0;

		pSpawned->GameObject.DisableDraw();
		pSpawned->CharacterSetup.CopyFromCharacter(pSpawned, CharacterSetupContainer.CopyFlags.None); // Some tools get confused (Like Penumbra) unless we copy onto ourselves after name change
		pSpawned->GameObject.EnableDraw();

		CreatedIndexes.Add(spawnedCharacterId);

		this.Log.Information($"Spawning character {name} with id {spawnedCharacterId}");

		return pSpawned->ObjectIndex;
	}
}
