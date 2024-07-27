// Brio
// https://github.com/Etheirys/Brio/tree/main/Brio/Game/Actor/ActorAppearanceService.cs

namespace ScreenshotStudio.Services;

using Dalamud.Game;
using Dalamud.Hooking;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using ScreenshotStudio.Library;
using ScreenshotStudio.Library.Sources;
using ScreenshotStudio.Plugin;
using ScreenshotStudio.Utilities;
using Serilog;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class CharacterAppearanceService : ServiceBase
{
	private readonly GroupPoseCharactersLibrarySource provider = new();
	private readonly Dictionary<ushort, CharacterBackupAppearance> backup = new();

	private Hook<EnforceKindRestrictionsDelegate>? enforceKindRestrictionsHook;

	private delegate byte EnforceKindRestrictionsDelegate(nint a1, nint a2);

	public override Task Start()
	{
		this.Services.GroupPose.StateChange += this.OnGroupPoseStateChange;
		this.Services.Library.AddSource(this.provider);

		if (this.Services.GroupPose.IsGroupPosing)
		{
			this.provider.OnEnterGroupPose();
		}

		this.enforceKindRestrictionsHook = InteropService.HookFromSignature<EnforceKindRestrictionsDelegate>("E8 ?? ?? ?? ?? 41 B0 ?? 48 8B D6", this.EnforceKindRestrictionsDetour);
		this.enforceKindRestrictionsHook?.Enable();

		return base.Start();
	}

	public override Task Stop()
	{
		this.Services.GroupPose.StateChange -= this.OnGroupPoseStateChange;
		this.enforceKindRestrictionsHook?.Dispose();
		return base.Stop();
	}

	public unsafe bool CanRestore(Character* character)
	{
		if (character == null)
			return false;

		ushort index = character->GameObject.ObjectIndex;
		return this.backup.ContainsKey(index);
	}

	public unsafe void Backup(Character character)
	{
		ushort index = character.GameObject.ObjectIndex;

		if (this.backup.ContainsKey(index))
			return;

		this.backup.Add(index, new(character));
	}

	public unsafe void Backup(Character* character)
	{
		ushort index = character->GameObject.ObjectIndex;

		if (this.backup.ContainsKey(index))
			return;

		this.backup.Add(index, new(character));
	}

	public unsafe void Restore(Character* character)
	{
		ushort index = character->GameObject.ObjectIndex;

		if (!this.backup.ContainsKey(index))
			return;

		Threads.RunOnFrameworkThread(() =>
		{
			this.backup[index].Apply(character, CharacterExtensions.UpdateSource.Restore);
			this.backup.Remove(index);
		});
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

public class CharacterBackupAppearance : EntryBase, ICharacterAppearance
{
	private readonly string? name;

	public unsafe CharacterBackupAppearance(Character* character)
		: base(null)
	{
		this.name = character->GetNameAsString();
		this.DrawData = character->DrawData;
		this.ModelId = character->ModelCharaId;

		this.Tags.Add("Named");
	}

	public CharacterBackupAppearance(Character character)
		: base(null)
	{
		this.name = character.GetNameAsString();
		this.DrawData = character.DrawData;
		this.ModelId = character.ModelCharaId;

		this.Tags.Add("Named");
	}

	public DrawDataContainer DrawData { get; private set; }
	public int ModelId { get; private set; }
	public override string Name => this.name ?? string.Empty;

	public unsafe void Apply(Character* character)
	{
		this.Apply(character, CharacterExtensions.UpdateSource.Library);
	}

	public unsafe void Apply(Character* character, CharacterExtensions.UpdateSource source)
	{
		bool redraw = this.ModelId != character->ModelCharaId;

		character->UpdateModel(this.ModelId, source, false);
		character->UpdateCustomize(this.DrawData.CustomizeData, redraw, source);
		character->UpdateEquipment(this.DrawData.EquipmentModelIds, source);
	}

	protected override string GetInternalId() => this.Name;
}

public class GroupPoseCharactersLibrarySource : SourceBase
{
	public ILogger Log { get; init; } = Logging.ForContext<GroupPoseCharactersLibrarySource>();
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

	public override void Scan()
	{
	}

	protected override string GetInternalId() => "CurrentCharactersLibraryProvider";

	private unsafe void BackupAll()
	{
		// back up the appearance of every character in gpose
		for (int i = GroupPoseService.GPoseFirstCharacter; i < GroupPoseService.GPoseFirstCharacter + GroupPoseService.GPoseCharacterCount; ++i)
		{
			IntPtr? address = DalamudServices.ObjectTable?.GetObjectAddress(i);
			if (address == null || address == IntPtr.Zero)
				continue;

			Character* character = (Character*)address;

			CharacterBackupAppearance appearance = new(character);
			this.Add(appearance);
		}
	}
}