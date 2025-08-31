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

namespace StudioFourteen.Posing;

using StudioFourteen.Files;
using StudioFourteen.Library;
using StudioFourteen.Scene.GameObjects.Characters.Skeletons;
using StudioFourteen.Selection;
using StudioFourteen.Services;
using StudioFourteen.Structs.Extensions;
using StudioFourteen.Tags;
using StudioFourteen.Utilities;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;

public class PoseFileTypeInfo : JsonFileTypeInfoBase<PoseFile>
{
	public override string Extension => ".pose";
	public override string TypeName => "Pose";
	public override object? Icon => Resources.Find("ICON_Library_Entry_Pose");
}

[Serializable]
public class PoseFile : FileBase
{
	public LegacyBoneTransform? ModelDifference { get; set; }

	public Dictionary<string, LegacyBoneTransform>? Bones { get; set; }
	public Dictionary<string, LegacyBoneTransform>? MainHand { get; set; } = new();
	public Dictionary<string, LegacyBoneTransform>? OffHand { get; set; } = new();

	// New Studio format: Bones as relative transforms from reference pose values.
	// supports loading poses across races with full positions and scale support.
	public Dictionary<string, BoneTransform>? ReferenceRelativeBones { get; set; }

	public override void GetAutoTags(TagCollection tags)
	{
		base.GetAutoTags(tags);

		tags.Add("Pose");

		if (this.ReferenceRelativeBones == null)
		{
			tags.Add("LegacyPose");
		}
	}

	public override Task Execute()
	{
		if (ServiceManager.Instance.Selection.Current is Skeleton skeleton)
		{
			return skeleton.ImportPose(this, UpdateSource.Interface);
		}

		return Task.CompletedTask;
	}

	public override LibraryPreviewBase GetPreview()
	{
		return new PosePreview(this);
	}

	// This is really just here so it serializes the same
	public class LegacyBoneTransform
	{
		public Vector3? Position { get; set; }
		public Quaternion? Rotation { get; set; }
		public Vector3? Scale { get; set; }

		public BoneTransform ToBoneTransform()
		{
			BoneTransform transform = new();
			transform.Translation = this.Position;
			transform.Scale = this.Scale;
			transform.Rotation = this.Rotation;
			return transform;
		}
	}

	#pragma warning disable
	public class PosePreview(PoseFile file) : LibraryPreviewBase
	{
		private PoseFile? backupPose;

		protected override async Task Start(LibraryPreviewBase? other)
		{
			Skeleton? current = ServiceManager.Instance.Selection.GetLast<Skeleton>();
			if (current == null)
				return;

			if (other is PosePreview otherPosePreview && otherPosePreview.backupPose != null)
			{
				this.backupPose = otherPosePreview.backupPose;
			}
			else
			{
				this.backupPose = await current.ExportPoseAsync(false, null, true);
				await Task.Delay(33);
			}

			await current.ImportPose(file, UpdateSource.Preview);
		}

		protected override async Task Stop()
		{
			if (this.backupPose == null)
				return;

			Skeleton? current = ServiceManager.Instance.Selection.GetLast<Skeleton>();
			if (current == null)
				return;

			await current.ImportPose(this.backupPose, UpdateSource.Restore);
		}
	}
}