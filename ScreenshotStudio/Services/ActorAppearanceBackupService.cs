namespace ScreenshotStudio.Services;

using FFXIVClientStructs.FFXIV.Client.Game.Character;
using ScreenshotStudio.Library;
using ScreenshotStudio.Library.Sources;
using ScreenshotStudio.Plugin;
using ScreenshotStudio.Structs;
using ScreenshotStudio.Tags;
using ScreenshotStudio.Utilities;
using Serilog;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WpfUtils;

public class ActorAppearanceBackupService : ServiceBase
{
	private readonly GroupPoseCharactersLibrarySource provider = new();
	private readonly Dictionary<ushort, ActorBackupAppearance> backup = new();

	public override Task Start()
	{
		GroupPoseService.OnStateChange += this.OnGroupPoseStateChange;
		this.Services.Library.AddSource(this.provider);

		if (this.Services.GroupPose.IsGroupPosing)
		{
			this.provider.OnEnterGroupPose();
		}

		return base.Start();
	}

	public override Task Stop()
	{
		GroupPoseService.OnStateChange -= this.OnGroupPoseStateChange;
		return base.Stop();
	}

	public unsafe bool CanRestore(Character* actor)
	{
		if (actor == null)
			return false;

		ushort index = actor->GameObject.ObjectIndex;
		return this.backup.ContainsKey(index);
	}

	public unsafe void Backup(Character actor)
	{
		ushort index = actor.GameObject.ObjectIndex;

		if (this.backup.ContainsKey(index))
			return;

		this.backup.Add(index, new(actor));
	}

	public unsafe void Backup(Character* actor)
	{
		ushort index = actor->GameObject.ObjectIndex;

		if (this.backup.ContainsKey(index))
			return;

		this.backup.Add(index, new(actor));
	}

	public unsafe void Restore(Character* actor)
	{
		ushort index = actor->GameObject.ObjectIndex;

		if (!this.backup.ContainsKey(index))
			return;

		Threads.RunOnFrameworkThread(() =>
		{
			this.backup[index].Apply(actor, CharacterExtensions.UpdateSource.Restore);
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
}

public class ActorBackupAppearance : EntryBase, IActorAppearance
{
	private readonly string? name;

	public unsafe ActorBackupAppearance(Character* actor)
		: base(null)
	{
		this.name = actor->GetNameAsString();
		this.DrawData = actor->DrawData;
		this.ModelId = actor->ModelCharaId;

		this.Tags.Add("Named");
	}

	public ActorBackupAppearance(Character actor)
		: base(null)
	{
		this.name = actor.GetNameAsString();
		this.DrawData = actor.DrawData;
		this.ModelId = actor.ModelCharaId;

		this.Tags.Add("Named");
	}

	public DrawDataContainer DrawData { get; private set; }
	public int ModelId { get; private set; }
	public override string Name => this.name ?? string.Empty;

	public unsafe void Apply(Character* actor)
	{
		this.Apply(actor, CharacterExtensions.UpdateSource.Library);
	}

	public unsafe void Apply(Character* actor, CharacterExtensions.UpdateSource source)
	{
		bool redraw = this.ModelId != actor->ModelCharaId;

		actor->UpdateModel(this.ModelId, source, false);
		actor->UpdateCustomize(this.DrawData.CustomizeData, redraw, source);
		actor->UpdateEquipment(this.DrawData.EquipmentModelIds, source);
	}

	protected override string GetInternalId() => this.Name;
}

public class GroupPoseCharactersLibrarySource : SourceBase
{
	public ILogger Log { get; init; } = Logging.ForContext<GroupPoseCharactersLibrarySource>();
	public override string Name => Resources.Find("LOC_Library_GroupPoseCharactersLibrarySource", "GPose Actors");

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

	protected override string GetInternalId() => "CurrentActorsLibraryProvider";

	private unsafe void BackupAll()
	{
		// back up the appearance of every actor in gpose
		for (int i = GroupPoseService.GPoseFirstActor; i < GroupPoseService.GPoseFirstActor + GroupPoseService.GPoseActorCount; ++i)
		{
			IntPtr? address = DalamudServices.ObjectTable?.GetObjectAddress(i);
			if (address == null || address == IntPtr.Zero)
				continue;

			Character* actor = (Character*)address;

			ActorBackupAppearance appearance = new(actor);
			this.Add(appearance);
		}
	}
}