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

namespace StudioFourteen.Services;

using Dalamud.Hooking;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.Event;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FontAwesome.Sharp;
using StudioFourteen.Appearance;
using StudioFourteen.Context;
using StudioFourteen.Interop;
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

	public delegate void CharacterDelegate(int objectTableIndex);

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

		await TickService.GameTick();
		this.DestroyAllCreated();
	}

	public async Task<int> CreateAsync(ICharacterAppearance? appearance = null)
	{
		return await this.CreateAsync(Vector3.Zero, appearance);
	}

	public async Task<int> CreateAsync(Vector3 position, ICharacterAppearance? appearance = null)
	{
		await TickService.GameTick();

		if (!this.CanSpawn)
			return -1;

		await TickService.GameTick();
		int index = this.Spawn(position);

		if (index == -1)
			return index;

		bool canDraw = false;
		while (!canDraw)
		{
			await Threads.NextFrame();
			unsafe
			{
				Character* pCharacter = this.Services.GameObjects.GetCharacter(index);
				canDraw = pCharacter->CanDraw();
			}
		}

		await Threads.NextFrame();

		string name = $"Studio {index}";
		if (appearance != null)
		{
			if (appearance.Name != null)
				name = appearance.Name;

			await appearance.Apply(index);
		}

		await TickService.GameTick();
		unsafe
		{
			Character* pCharacter = this.Services.GameObjects.GetCharacter(index);
			pCharacter->SetDisplayName(name);
		}

		return index;
	}

	public void Destroy(int objectTableIndex)
	{
		this.DestroyAsync(objectTableIndex).Run();
	}

	public async Task<bool> DestroyAsync(int objectTableIndex)
	{
		await TickService.GameTick();

		if (DalamudServices.ObjectTable == null)
			return false;

		// change to a new target before deleting the actor as Mare assumes no target = left gpose
		// and will crash.
		bool success = await this.Services.Target.MoveTarget(objectTableIndex);
		if (!success)
			return false;

		unsafe
		{
			GameObject* character = (GameObject*)DalamudServices.ObjectTable.GetObjectAddress(objectTableIndex);

			ClientObjectManager* com = ClientObjectManager.Instance();
			uint idx = com->GetIndexByObject((GameObject*)character);
			if (idx == 0xFFFFFFFF)
				return false;

			com->DeleteObjectByIndex((ushort)idx, 0);
		}

		await Threads.NextFrame();
		return true;
	}

	public unsafe void DestroyAllCreated()
	{
		TickService.VerifyGameTickThread();

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

			this.Log.Information($"Deleting object: {idx} - {deletingCharacter->GetDisplayName()}");
			com->DeleteObjectByIndex(idx, 0);
		}

		CreatedIndexes.Clear();
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

	public override unsafe void Attach()
	{
		base.Attach();

		Hooks.CharacterInitialize.Enable(this.CharacterInitializeDetour);
		Hooks.CharacterFinalize.Enable(this.CharacterFinalizeDetour);
		this.Services.Tick.Add(TickService.Channels.GameTick, this.OnTick);
	}

	public override void Detach()
	{
		base.Detach();
		Hooks.CharacterInitialize.Disable();
		Hooks.CharacterFinalize.Disable();
		this.Services.Tick.Remove(TickService.Channels.GameTick, this.OnTick);
	}

	protected void OnTick()
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
	}

	private unsafe nint CharacterInitializeDetour(Character* character)
	{
		nint result = Hooks.CharacterInitialize.Original.Invoke(character);
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

		nint result = Hooks.CharacterFinalize.Original.Invoke(character);
		this.CharacterDestroyed?.Invoke(objectTableIndex);
		return result;
	}

	private unsafe int Spawn(Vector3 position)
	{
		if (DalamudServices.ClientState?.LocalPlayer == null)
			return -1;

		TickService.VerifyGameTickThread();

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
		// This name must pass penumbra's naming validation to allow mcdf loading to work.
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
