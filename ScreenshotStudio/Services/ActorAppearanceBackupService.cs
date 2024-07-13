namespace ScreenshotStudio.Services;

using ScreenshotStudio.Structs;
using System.Collections.Generic;

public class ActorAppearanceBackupService : ServiceBase
{
	private readonly Dictionary<ushort, Appearance> backup = new();

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

		this.backup.Add(index, new(actor.DrawData, actor.ModelCharaRowId));
	}

	public unsafe void Backup(Actor* actor)
	{
		ushort index = actor->GameObject.ObjectIndex;

		if (this.backup.ContainsKey(index))
			return;

		this.backup.Add(index, new(actor->DrawData, actor->ModelCharaRowId));
	}

	public unsafe void Restore(Actor* actor)
	{
		ushort index = actor->GameObject.ObjectIndex;

		if (!this.backup.ContainsKey(index))
			return;

		this.backup[index].Apply(actor, Actor.UpdateSource.Restore);
		this.backup.Remove(index);
	}

	public class Appearance(ActorDrawData drawData, uint modelId)
	{
		public readonly ActorDrawData DrawData = drawData;
		public uint ModelId = modelId;

		public unsafe void Apply(Actor* actor, Actor.UpdateSource source)
		{
			bool redraw = this.ModelId != actor->ModelCharaRowId;

			actor->UpdateModel(this.ModelId, source, false);
			actor->UpdateCustomize(this.DrawData.Customize, source, redraw);
			actor->UpdateEquipment(this.DrawData.Equipment, source);
		}
	}

	/*private readonly CurrentActorsLibraryProvider provider = new();

	public override Task Start()
	{
		GroupPoseService.OnStateChange += this.OnGroupPoseStateChange;
		this.Services.Library.AddProvider(this.provider);

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

	private void OnGroupPoseStateChange(bool newState)
	{
		if (newState)
		{
			this.provider.OnEnterGroupPose();
		}
	}

	public class CurrentActorsLibraryProvider : LibraryProvider<IActorAppearance>
	{
		private readonly List<IActorAppearance> appearances = new();

		public ILogger Log { get; init; } = Logging.ForContext<CurrentActorsLibraryProvider>();

		public unsafe void OnEnterGroupPose()
		{
			this.appearances.Clear();

			DalamudServices.Framework?.RunOnFrameworkThread(() =>
			{
				// back up the appearance of every actor so that they are available in teh library while in group pose.
				for (int i = GroupPoseService.GPoseFirstActor; i < GroupPoseService.GPoseFirstActor + GroupPoseService.GPoseActorCount; ++i)
				{
					IntPtr? address = DalamudServices.ObjectTable?.GetObjectAddress(i);
					if (address == null || address == IntPtr.Zero)
						continue;

					Actor* actor = (Actor*)address;
					this.Log.Information($"Backup {actor->Name}!");
				}
			});
		}

		public override IEnumerator GetEnumerator()
		{
			return this.appearances.GetEnumerator();
		}

		protected override void GetAllTags(ref TagCollection tags)
		{
		}
	}*/
}
