namespace ScreenshotStudio.Services;

using ScreenshotStudio.Library;
using ScreenshotStudio.Plugin;
using ScreenshotStudio.Tags;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using ScreenshotStudio.Structs;
using Serilog;

public class ActorAppearanceBackupService : ServiceBase
{
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
