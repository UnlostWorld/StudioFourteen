//// Brio
//// https://github.com/AsgardXIV/Brio/
//// https://github.com/AsgardXIV/Brio/blob/main/Brio/Game/Actor/ActorSpawnService.cs
//// https://github.com/Etheirys/Brio/blob/main/Brio/Game/Core/ObjectMonitorService.cs

namespace ScreenshotStudio.Services;

using Dalamud.Hooking;
using ScreenshotStudio.Library;
using ScreenshotStudio.Plugin;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using FFXIVClientStructs.FFXIV.Client.Game.Event;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using System.Linq;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using ScreenshotStudio.Structs;
using ScreenshotStudio.Utilities;
using Dalamud.Game.ClientState.Objects.Types;

public class ActorLifecycleService : ServiceBase
{
	private static readonly List<ushort> CreatedIndexes = new();

	private Hook<CharacterEventDelegate>? characterInitializeHook;
	private Hook<CharacterEventDelegate>? characterFinalizeHook;
	private unsafe delegate nint CharacterEventDelegate(Character* character);

	public bool CanSpawn => GroupPoseService.IsGroupPosing;

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
			if (!GroupPoseService.IsGroupPosing && CreatedIndexes.Count > 0)
			{
				this.DestroyAllCreated();
				this.Log.Warning("Left GPose with spawned actors. deleting...");
			}
		}
		catch (Exception ex)
		{
			this.Log.Error(ex, "Error checking group pose state");
		}

		return base.Tick();
	}

	public unsafe Actor* Create(IActorAppearance? appearance = null)
	{
		Threads.VerifyFrameworkThread();

		if (!this.CanSpawn)
			return null;

		string name = "Actor";
		if (appearance != null && !string.IsNullOrEmpty(appearance.Name))
			name = appearance.Name;

		Actor* actor = this.Spawn(name);

		if (actor != null && appearance != null)
		{
			appearance?.Apply(actor);
		}

		return actor;
	}

	public unsafe bool Destroy(Actor* actor)
	{
		ClientObjectManager* com = ClientObjectManager.Instance();
		uint idx = com->GetIndexByObject((GameObject*)actor);
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
			Actor* deletingCharacter = (Actor*)com->GetObjectByIndex(idx);
			if (deletingCharacter == null)
			{
				this.Log.Error($"Attempt to delete object by index {idx} was not a character");
				continue;
			}

			Threads.RunOnFrameworkThread(() =>
			{
				this.Log.Information($"Deleting object: {idx} - {deletingCharacter->Name}");
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

		return result;
	}

	private unsafe nint CharacterFinalizeDetour(Character* character)
	{
		uint idx = ClientObjectManager.Instance()->GetIndexByObject((GameObject*)character);
		if (idx < ushort.MaxValue && CreatedIndexes.Contains((ushort)idx))
		{
			CreatedIndexes.Remove((ushort)idx);
			this.Log.Information($"created actor was destroyed: {idx}");
		}

		if (this.characterFinalizeHook == null)
			return 0;

		return this.characterFinalizeHook.Original.Invoke(character);
	}

	private unsafe Actor* Spawn(string name)
	{
		if (DalamudServices.ClientState?.LocalPlayer == null)
			return null;

		Threads.VerifyFrameworkThread();

		Character* player = (Character*)DalamudServices.ClientState.LocalPlayer.Address;

		if (player == null)
			return null;

		ClientObjectManager* com = ClientObjectManager.Instance();
		uint idCheck = com->CreateBattleCharacter();
		if (idCheck == 0xffffffff)
			return null;

		ushort spawnedActorId = (ushort)idCheck;

		Character* pSpawned = (Character*)com->GetObjectByIndex(spawnedActorId);
		if (pSpawned == null)
			return null;

		EventGPoseController* gposeController = &EventFramework.Instance()->EventSceneModule.EventGPoseController;
		gposeController->AddCharacterToGPose(pSpawned); // This is safe even if the list is full. The game will also cleanup for us.

		pSpawned->CharacterSetup.CopyFromCharacter(player, CharacterSetupContainer.CopyFlags.None); // We copy the Player as the created actor is just blank

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

		CreatedIndexes.Add(spawnedActorId);

		this.Log.Information($"Spawning actor {name} with id {spawnedActorId}");

		return (Actor*)pSpawned;
	}
}
