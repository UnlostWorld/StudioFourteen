// © XivTools.
// Licensed under the MIT license.

//// Brio
//// https://github.com/AsgardXIV/Brio/
//// https://github.com/AsgardXIV/Brio/blob/main/Brio/Game/Actor/ActorSpawnService.cs

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
using XivToolsWpf.Extensions;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using ScreenshotStudio.Structs;

public class ActorLifecycleService : ServiceBase
{
	private static readonly List<ushort> CreatedIndexes = new();
	private Hook<DestroyGameActorDelegate> destroyGameActorHook = null!;
	private delegate void DestroyGameActorDelegate(IntPtr addr);

	public bool CanSpawn => DalamudServices.PluginInterface.UiBuilder.GposeActive;

	public override async Task Initialize()
	{
		await base.Initialize();

		DalamudServices.ClientState.TerritoryChanged += (s, e) => CreatedIndexes.Clear();
	}

	public override async Task Start()
	{
		await base.Start();
		this.Attach();
	}

	public override async Task Stop()
	{
		await base.Stop();
		this.Detatch();
		this.DestroyAllCreated();
	}

	public override Task Tick()
	{
		try
		{
			if (!DalamudServices.PluginInterface.UiBuilder.GposeActive && CreatedIndexes.Count > 0)
			{
				this.DestroyAllCreated();
				this.Log.Warning("Left GPose with spawned actors. deleting...");
			}
		}
		catch (Exception ex)
		{
			this.Log.Error(ex, "Error checking gpose state");
		}

		return base.Tick();
	}

	public unsafe void Create(IActorAppearance? appearance = null)
	{
		if (!this.CanSpawn)
			return;

		Actor* pActor = this.Spawn("Test Actor");

		if (pActor != null && appearance != null)
		{
			appearance?.Apply(pActor);
		}
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

			DalamudServices.Framework.RunOnFrameworkThread(() =>
			{
				this.Log.Information($"Deleteing object: {idx} - {deletingCharacter->Name}");
				com->DeleteObjectByIndex(idx, 0);
			});
		}

		CreatedIndexes.Clear();
	}

	/*public unsafe bool DestroyGameObject(GameObject* gameObject)
	{
		ClientObjectManager* com = ClientObjectManager.Instance();
		uint idx = com->GetIndexByObject(gameObject);
		if (idx != 0xFFFFFFFF)
		{
			com->DeleteObjectByIndex((ushort)idx, 0);
			return true;
		}

		return false;
	}*/

	private void Attach()
	{
		var destroyAddress = DalamudServices.SigScanner.ScanText("48 89 5C 24 ?? 48 89 74 24 ?? 57 48 83 EC ?? 48 8D 05 ?? ?? ?? ?? 48 8B D9 48 89 01 48 8D 05 ?? ?? ?? ?? 48 89 81 ?? ?? ?? ?? 48 8D 05");
		this.destroyGameActorHook = Hook<DestroyGameActorDelegate>.FromAddress(destroyAddress, this.ActorDestructorDetour);
		this.destroyGameActorHook.Enable();
	}

	private void Detatch()
	{
		this.destroyGameActorHook.Dispose();
	}

	private unsafe void ActorDestructorDetour(IntPtr addr)
	{
		uint idx = ClientObjectManager.Instance()->GetIndexByObject((GameObject*)addr);
		if (idx < ushort.MaxValue && CreatedIndexes.Contains((ushort)idx))
		{
			CreatedIndexes.Remove((ushort)idx);
			this.Log.Information($"created actor was destroyed: {idx}");
		}

		this.destroyGameActorHook.Original.Invoke(addr);
	}

	private unsafe Actor* Spawn(string name)
	{
		if (DalamudServices.ClientState.LocalPlayer == null)
			return null;

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

		pSpawned->CopyFromCharacter(player, Character.CopyFlags.None); // We copy the Player as the created actor is just blank

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
		pSpawned->CopyFromCharacter(pSpawned, Character.CopyFlags.None); // Some tools get confused (Like Penumbra) unless we copy onto ourselves after name change
		pSpawned->GameObject.EnableDraw();

		CreatedIndexes.Add(spawnedActorId);

		this.Log.Information($"Spawning actor {name} with id {spawnedActorId}");

		return (Actor*)pSpawned;
	}
}
