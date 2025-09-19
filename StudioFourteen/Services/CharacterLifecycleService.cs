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

using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.Game.Event;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using StudioFourteen.Appearance;
using StudioFourteen.Interop;
using StudioFourteen.Mvm;
using StudioFourteen.Plugin;
using StudioFourteen.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

using Character = StudioFourteen.Scene.GameObjects.Characters.Character;
using XivCharacter = FFXIVClientStructs.FFXIV.Client.Game.Character.Character;
using XivSetupContainer = FFXIVClientStructs.FFXIV.Client.Game.Character.CharacterSetupContainer;

[Service]
public class CharacterLifecycleService : ServiceBase
{
	private static readonly List<ushort> CreatedIndexes = new();

	public delegate void CharacterDelegate(int objectTableIndex);

	public event CharacterDelegate? CharacterCreated;
	public event CharacterDelegate? CharacterDestroyed;

	[AutoNotify]
	public bool CanSpawn => this.Services.GroupPose.IsGroupPosing || this.Services.Territory.IsInTitleScreen;

	public override async Task Initialize()
	{
		await base.Initialize();

		if (DalamudServices.ClientState != null)
		{
			DalamudServices.ClientState.TerritoryChanged += (s) => CreatedIndexes.Clear();
		}
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

		this.DestroyAllCreated();

		Hooks.CharacterInitialize.Disable();
		Hooks.CharacterFinalize.Disable();
		this.Services.Tick.Remove(TickService.Channels.GameTick, this.OnTick);
	}

	public async Task<Character?> CreateAsync(ICharacterAppearance? appearance, UpdateSource updateSource)
	{
		return await this.CreateAsync(Vector3.Zero, appearance, updateSource);
	}

	public async Task<Character?> CreateAsync(Vector3 position, ICharacterAppearance? appearance, UpdateSource updateSource)
	{
		await TickService.GameTick();

		if (!this.CanSpawn)
			return null;

		await TickService.GameTick();
		int index = this.Spawn(position);
		if (index < 0)
			return null;

		await Task.Delay(100);
		await TickService.NextGameTick();
		Character? character = this.Services.GameObjects.Get<Character>(index);
		if (character == null)
			return null;

		Stopwatch sw = new Stopwatch();
		sw.Start();
		bool canDraw = false;
		while (!canDraw && sw.ElapsedMilliseconds < 1000)
		{
			await Threads.NextFrame();
			unsafe
			{
				XivCharacter* pCharacter = character.GetXivCharacter();
				pCharacter->Alpha = 1.0f;
				canDraw = pCharacter->CanDraw();
			}
		}

		sw.Stop();
		if (sw.ElapsedMilliseconds >= 1000)
			throw new Exception("Failed to create character, timeout waiting for draw");

		await Threads.NextFrame();

		string name = $"Studio {character}";
		if (appearance != null)
		{
			if (appearance.Name != null)
				name = appearance.Name;

			await appearance.Apply(character, updateSource);
		}

		await TickService.GameTick();
		unsafe
		{
			XivCharacter* pCharacter = character.GetXivCharacter();

			Character? selectedCharacter = this.Services.Selection.GetLast<Character>();
			if (selectedCharacter != null)
			{
				// Move the spawned characters draw object to the current targets location.
				XivCharacter* pSelectedCharacter = selectedCharacter.GetXivCharacter();
				if (pCharacter->DrawObject != null && pSelectedCharacter != null && pSelectedCharacter->DrawObject != null)
				{
					pCharacter->DrawObject->Position = pSelectedCharacter->DrawObject->Position;
					pCharacter->DrawObject->Rotation = pSelectedCharacter->DrawObject->Rotation;
				}
			}
			else
			{
				// TODO: Raycast from camera?
			}
		}

		return character;
	}

	public void Destroy(int objectTableIndex)
	{
		this.DestroyAsync(objectTableIndex).RunAsynchronously();
	}

	public async Task<bool> DestroyAsync(int objectTableIndex)
	{
		await TickService.GameTick();

		if (!this.Services.GroupPose.IsGroupPosing)
			return false;

		unsafe
		{
			// Mare assumes no target = left gpose and will unload all mods, so don't
			// let us delete the target object (should be 0 (the player character) but users
			// might change targets with other tools or outside of Studio, so check to make sure.)
			if (TargetSystem.Instance()->GPoseTarget->ObjectIndex == objectTableIndex)
			{
				return false;
			}
		}

		await TickService.GameTick();

		unsafe
		{
			GameObject* character = this.Services.GameObjects.GetXivObject(objectTableIndex);

			ClientObjectManager* com = ClientObjectManager.Instance();
			uint idx = com->GetIndexByObject(character);
			if (idx == 0xFFFFFFFF)
				return false;

			com->DeleteObjectByIndex((ushort)idx, 0);
		}

		await Threads.NextFrame();
		return true;
	}

	public unsafe void DestroyAllCreated()
	{
		////TickService.VerifyGameTickThread();

		List<ushort> indexes = CreatedIndexes.ToList();
		ClientObjectManager* com = ClientObjectManager.Instance();
		foreach (ushort idx in indexes)
		{
			XivCharacter* deletingCharacter = (XivCharacter*)com->GetObjectByIndex(idx);
			if (deletingCharacter == null)
			{
				this.Log.Error($"Attempt to delete object by index {idx} was not a character");
				continue;
			}

			this.Log.Information($"Deleting object: {idx} - {deletingCharacter->NameString}");
			com->DeleteObjectByIndex((ushort)idx, 0);
		}

		CreatedIndexes.Clear();
	}

	protected void OnTick()
	{
		try
		{
			if (!this.Services.Territory.IsInTitleScreen
				&& !this.Services.GroupPose.IsGroupPosing
				&& CreatedIndexes.Count > 0)
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

	private unsafe nint CharacterInitializeDetour(XivCharacter* character)
	{
		nint result = Hooks.CharacterInitialize.Original.Invoke(character);
		this.CharacterCreated?.Invoke(character->ObjectIndex);
		return result;
	}

	private unsafe nint CharacterFinalizeDetour(XivCharacter* character)
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
		TickService.VerifyGameTickThread();

		ClientObjectManager* com = ClientObjectManager.Instance();
		uint idCheck = com->CreateBattleCharacter();
		if (idCheck == 0xffffffff)
			return -1;

		ushort spawnedCharacterId = (ushort)idCheck;

		XivCharacter* pSpawned = (XivCharacter*)com->GetObjectByIndex(spawnedCharacterId);
		if (pSpawned == null)
			return -1;

		pSpawned->Alpha = 0.01f;

		EventGPoseController* gposeController = &EventFramework.Instance()->EventSceneModule.EventGPoseController;
		gposeController->AddCharacterToGPose(pSpawned); // This is safe even if the list is full. The game will also cleanup for us.

		pSpawned->CharacterSetup.SetupBNpc(1);

		*((sbyte*)pSpawned + 0x95) &= ~2; // Disable selection just in case this somehow leaks out of GPose

		pSpawned->GameObject.Position = position;
		pSpawned->GameObject.DefaultPosition = position;

		// Generate a unique name.
		// This name must pass penumbra's naming validation.
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
		pSpawned->CharacterSetup.CopyFromCharacter(pSpawned, XivSetupContainer.CopyFlags.None);
		pSpawned->GameObject.EnableDraw();

		pSpawned->Alpha = 0.01f;

		CreatedIndexes.Add(spawnedCharacterId);

		this.Log.Information($"Spawning character {pSpawned->NameString} with id {spawnedCharacterId} and index {pSpawned->ObjectIndex}");

		return pSpawned->ObjectIndex;
	}
}
