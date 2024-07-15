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
	private readonly CurrentActorsLibraryProvider provider = new();
	private readonly Dictionary<ushort, Appearance> backup = new();

	public override Task Start()
	{
		GroupPoseService.OnStateChange += this.OnGroupPoseStateChange;
		this.Services.Library.AddSource(this.provider);

		if (GroupPoseService.IsGroupPosing)
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

	public unsafe bool CanRestore(Actor* actor)
	{
		ushort index = actor->GameObject.ObjectIndex;
		return this.backup.ContainsKey(index);
	}

	public unsafe void Backup(Actor actor)
	{
		ushort index = actor.GameObject.ObjectIndex;

		if (this.backup.ContainsKey(index))
			return;

		this.backup.Add(index, new(actor));
	}

	public unsafe void Backup(Actor* actor)
	{
		ushort index = actor->GameObject.ObjectIndex;

		if (this.backup.ContainsKey(index))
			return;

		this.backup.Add(index, new(actor));
	}

	public unsafe void Restore(Actor* actor)
	{
		ushort index = actor->GameObject.ObjectIndex;

		if (!this.backup.ContainsKey(index))
			return;

		Threads.RunOnFrameworkThread(() =>
		{
			this.backup[index].Apply(actor, Actor.UpdateSource.Restore);
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

	public class Appearance : IActorAppearance
	{
		public unsafe Appearance(Actor* actor)
		{
			this.Name = actor->Name;
			this.DrawData = actor->DrawData;
			this.ModelId = actor->ModelCharaRowId;

			this.Tags.Add("Named");
		}

		public Appearance(Actor actor)
		{
			this.Name = actor.Name;
			this.DrawData = actor.DrawData;
			this.ModelId = actor.ModelCharaRowId;

			this.Tags.Add("Named");
		}

		public DrawDataContainer DrawData { get; private set; }
		public uint ModelId { get; private set; }
		public string? Name { get; private set; }
		public TagCollection Tags { get; init; } = new();
		public SourceBase? Source { get; set; }
		public string Identifier => string.Empty;
		public bool IsValid => true;

		public unsafe void Apply(Actor* actor)
		{
			this.Apply(actor, Actor.UpdateSource.Library);
		}

		public unsafe void Apply(Actor* actor, Actor.UpdateSource source)
		{
			bool redraw = this.ModelId != actor->ModelCharaRowId;

			actor->UpdateModel(this.ModelId, source, false);
			actor->UpdateCustomize(this.DrawData.CustomizeData, redraw, source);
			actor->UpdateEquipment(this.DrawData.EquipmentModelIds, source);
		}

		public void Dispose()
		{
		}

		public bool Search(string[] query)
		{
			return SearchUtility.Matches(this.Name, query);
		}
	}

	public class CurrentActorsLibraryProvider : SourceBase
	{
		public ILogger Log { get; init; } = Logging.ForContext<CurrentActorsLibraryProvider>();
		public override string Name => "Current Actors";

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

				Actor* actor = (Actor*)address;

				Appearance appearance = new(actor);
				this.Add(appearance);
			}
		}
	}
}
