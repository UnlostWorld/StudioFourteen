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

namespace StudioFourteen.Files;
using FontAwesome.Sharp;
using StudioFourteen.Library;
using StudioFourteen.Library.LibraryMenu;
using StudioFourteen.Posing;
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

		if (this.ReferenceRelativeBones == null)
		{
			tags.Add("LegacyPose");
		}
	}

	public async Task Save(int objectTableIndex, bool includeLegacyBones = true, HashSet<string>? includeBones = null, bool onlyEdits = false)
	{
		await Threads.FrameworkThread();

		this.Bones = new();
		this.ReferenceRelativeBones = new();
		this.MainHand = null;
		this.OffHand = null;

		List<BoneReference> references = await ServiceManager.Instance.Pose.GetOrCreateBoneReferences(objectTableIndex);

		// Wait one frame for all the bone references to populate with real transform data.
		await Threads.NextFrame();

		foreach (BoneReference boneReference in references)
		{
			if (boneReference.Name == null)
				continue;

			if (boneReference.Name == "n_root")
				continue;

			// We'll have duplicate bone names, since we support indexing all the duplicate
			// HkPose and PartialSkeleton bones, but we can fairly safely assume the first
			// bone will be the one we want (from the lowest HkPose and PartialSkeleton)
			if (this.Bones.ContainsKey(boneReference.Name))
				continue;

			if (this.ReferenceRelativeBones.ContainsKey(boneReference.Name))
				continue;

			if (onlyEdits && (boneReference.Transform == null || boneReference.IsBlendingOut))
				continue;

			while (boneReference.IsBlending)
				await Task.Delay(33);

			// Legacy bone format for backwards compatibility
			if (includeLegacyBones && boneReference.ModelSpaceTransform != null)
			{
				LegacyBoneTransform modelSpaceTransform = new();
				modelSpaceTransform.Position = boneReference.ModelSpaceTransform.Value.Translation;
				modelSpaceTransform.Rotation = boneReference.ModelSpaceTransform.Value.Rotation;
				modelSpaceTransform.Scale = boneReference.ModelSpaceTransform.Value.Scale;
				this.Bones.Add(boneReference.Name, modelSpaceTransform);
			}

			// New format bones
			if (boneReference.LocalSpaceTransform != null && boneReference.ReferenceTransform != null)
			{
				Transform? referenceRelative = boneReference.ReferenceRelativeTransform;
				if (referenceRelative == null)
					continue;

				if (includeBones == null || includeBones.Contains(boneReference.Name))
				{
					BoneTransform boneTransform = new();
					boneTransform.Locked = boneReference.Locked;

					// Null out components that are irrelevantly small
					if (!referenceRelative.Value.Translation.IsApproximately(Vector3.Zero, 0.001f))
						boneTransform.Translation = referenceRelative.Value.Translation;

					// If the rotation quat has no x,y, or z component, then ignore it, as 0,0,0,1 is identity, and
					// a W component without X,Y,Z components doesn't do anything afaik.
					if (!referenceRelative.Value.Rotation.X.IsApproximately(0, 0.001f)
						|| !referenceRelative.Value.Rotation.Y.IsApproximately(0, 0.001f)
						|| !referenceRelative.Value.Rotation.Z.IsApproximately(0, 0.001f))
					{
						boneTransform.Rotation = referenceRelative.Value.Rotation;
					}

					if (!referenceRelative.Value.Scale.IsApproximately(Vector3.One, 0.001f))
						boneTransform.Scale = referenceRelative.Value.Scale;

					this.ReferenceRelativeBones.Add(boneReference.Name, boneTransform);
				}
			}
		}
	}

	[LibraryMenuTarget(IconChar.Running, "LOC_AppearanceApplyTo")]
	public Task Apply(int objectTableIndex)
	{
		return this.Apply(objectTableIndex, true);
	}

	public async Task Apply(int objectTableIndex, bool blend)
	{
		await Threads.FrameworkThread();

		bool useReferenceRelativeBones = this.ReferenceRelativeBones != null;

		// Get bone references
		List<BoneReference> boneReferences = await ServiceManager.Instance.Pose.GetOrCreateBoneReferences(objectTableIndex);
		await Threads.NextFrame();

		foreach (BoneReference boneReference in boneReferences)
		{
			if (boneReference.Name == null)
				continue;

			if (boneReference.Name == "n_root")
				continue;

			if (useReferenceRelativeBones && this.ReferenceRelativeBones != null)
			{
				BoneTransform? val = null;
				if (this.ReferenceRelativeBones.TryGetValue(boneReference.Name, out val))
				{
					boneReference.SetReferenceRelativeTransform(val, blend);
					boneReference.Locked = val.Locked;
				}

				continue;
			}
			else
			{
				// no face bones for legacy poses
				if (boneReference.Name.StartsWith("j_f_"))
					continue;

				LegacyBoneTransform? val = null;
				if (this.Bones?.TryGetValue(boneReference.Name, out val) != true)
				{
					string? legacyName = LegacyBoneNameConverter.GetLegacyName(boneReference.Name);
					if (legacyName != null)
					{
						this.Bones?.TryGetValue(legacyName, out val);
					}
				}

				if (val != null)
				{
					// TODO: Allow a way for the user to explicitly include translation & scale
					// but disable them by default (Anamnesis style)
					val.Position = null;
					val.Scale = null;

					boneReference.SetModelSpaceTransform(val.ToBoneTransform());
					boneReference.Locked = true;
					continue;
				}
			}

			boneReference.Reset(false);
		}
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

	public class PosePreview(PoseFile file) : LibraryPreviewBase
	{
		private PoseFile? backupPose;

		protected override async Task Start(LibraryPreviewBase? other)
		{
			if (other is PosePreview otherPosePreview && otherPosePreview.backupPose != null)
			{
				this.backupPose = otherPosePreview.backupPose;
			}
			else
			{
				this.backupPose = new();
				await this.backupPose.Save(this.Services.Target.TargetObjectIndex, false, null, true);
				await Task.Delay(33);
			}

			await file.Apply(this.Services.Target.TargetObjectIndex);
		}

		protected override async Task Stop()
		{
			if (this.backupPose == null)
				throw new Exception("No backup pose in pose preview");

			await this.backupPose.Apply(this.Services.Target.TargetObjectIndex);
		}
	}
}