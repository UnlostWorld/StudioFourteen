// Brio
// https://github.com/Etheirys/Brio/tree/main/Brio/Game/Actor/ActorAppearanceService.cs

namespace StudioFourteen.Appearance;

using Dalamud.Hooking;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using StudioFourteen;
using StudioFourteen.GameData;
using StudioFourteen.Library;
using StudioFourteen.Library.Sources;
using StudioFourteen.Plugin;
using StudioFourteen.Services;
using StudioFourteen.Utilities;
using Serilog;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Windows.Input;
using StudioFourteen.Mvm.Commands;

public class CharacterAppearanceService : ServiceBase
{
	private readonly GroupPoseCharactersLibrarySource provider = new();
	private readonly ConcurrentDictionary<int, CharacterBackupAppearance> backup = new();

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

public class CharacterBackupAppearance
	: LibraryEntryBase, ICharacterAppearance, ILibraryActions
{
	private readonly string? name;

	public unsafe CharacterBackupAppearance(Character* character)
		: base(null)
	{
		this.name = character->GetDisplayName();
		this.DrawData = character->DrawData;
		this.ModelId = character->ModelCharaId;

		this.Tags.Add("Named");

		CustomizeData customize = this.DrawData.CustomizeData;
		this.Icon = customize.GetIcon();

		this.ApplyCommand = new TargetCommand(this.Apply);
		this.RevertCommand = new RevertTargetAppearanceCommand();
		this.SpawnCommand = new TargetCommand(this.Spawn, true);
	}

	public CharacterBackupAppearance(Character character)
		: base(null)
	{
		this.name = character.GetDisplayName();
		this.DrawData = character.DrawData;
		this.ModelId = character.ModelCharaId;

		this.Tags.Add("Named");

		CustomizeData customize = this.DrawData.CustomizeData;
		this.Icon = customize.GetIcon();

		this.ApplyCommand = new TargetCommand(this.Apply);
		this.RevertCommand = new RevertTargetAppearanceCommand();
		this.SpawnCommand = new TargetCommand(this.Spawn, true);
	}

	public ICommand ApplyCommand { get; init; }
	public ICommand RevertCommand { get; init; }
	public ICommand SpawnCommand { get; init; }

	public DrawDataContainer DrawData { get; private set; }
	public int ModelId { get; private set; }
	public override string Name => this.name ?? string.Empty;
	public ImageReference? Icon { get; private set; }

	public Task Spawn(int objectTableIndex)
	{
		return ServiceManager.Instance.CharacterLifecycle.CreateAsync(this);
	}

	public Task Apply(int objectTableIndex)
	{
		return this.Apply(objectTableIndex, CharacterExtensions.UpdateSource.Library);
	}

	public async Task Apply(int objectTableIndex, CharacterExtensions.UpdateSource source)
	{
		await Threads.FrameworkThread();

		if (DalamudServices.ObjectTable == null)
			return;

		unsafe
		{
			Character* character = (Character*)DalamudServices.ObjectTable.GetObjectAddress(objectTableIndex);

			bool redraw = true; //// this.ModelId != character->ModelCharaId;

			character->UpdateModel(this.ModelId, source, false);
			character->UpdateEquipment(this.DrawData.EquipmentModelIds, source);
			character->UpdateCustomize(this.DrawData.CustomizeData, redraw, source);
		}
	}

	protected override string GetInternalId() => this.Name;
}

public class GroupPoseCharactersLibrarySource : SourceBase
{
	public override string Name => Resources.Find("LOC_Library_GroupPoseCharactersLibrarySource", "GPose Characters");

	public void OnEnterGroupPose()
	{
		this.Clear();

		Task.Run(async () =>
		{
			await Task.Delay(1500);
			await Threads.RunOnFrameworkThread(() => this.BackupAll());
		});
	}

	protected override void Scan()
	{
	}

	protected override string GetInternalId() => "CurrentCharactersLibraryProvider";

	private unsafe void BackupAll()
	{
		// back up the appearance of every character in gpose
		for (int i = GroupPoseService.GPoseFirstCharacter; i < GroupPoseService.GPoseFirstCharacter + GroupPoseService.GPoseCharacterCount; ++i)
		{
			nint? address = DalamudServices.ObjectTable?.GetObjectAddress(i);
			if (address == null || address == nint.Zero)
				continue;

			Character* character = (Character*)address;

			CharacterBackupAppearance appearance = new(character);
			this.Add(appearance);
		}
	}
}