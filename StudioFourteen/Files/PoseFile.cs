// .                      @@             _____ _______ _    _ _____ _____ ____
//            @       @@@@@             / ____|__   __| |  | |  __ \_   _/ __ \
//           @@@  @@@@                 | (___    | |  | |  | | |  | || || |  | |
//           @@@@@@@@@  @    @          \___ \   | |  | |  | | |  | || || |  | |
//          @@@@       @@@@@@@          ____) |  | |  | |__| | |__| || || |__| |
//      @@@@@             @@@          |_____/   |_|   \____/|_____/_____\____/
//       @@@      @@@      @@        ___     _    _   _  __   _____  ___  ___  _  _
//        @@    @@@@@@@    @@       |  _|  / _ \ | | | || _ \|_   _|| __|| __|| \| |
//        @@    @@@@@@@    @   @    | __| | (_) || |_| ||   /  | |  | _| | _| | .` |
//      @@@@      @@@      @@@@     |_|    \___/  \___/ |_|_\  |_|  |___||___||_|\_|
//       @@@@             @@@
//         @@@@@      @@@@@               This software is licensed under the
//          @@@@@@@@@@@@@@                 GNU AFFERO GENERAL PUBLIC LICENSE
//              @@@@  @                       Version 3, 19 November 2007

namespace StudioFourteen.Files;

using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Graphics.Render;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using FFXIVClientStructs.Havok.Animation.Rig;
using FontAwesome.Sharp;
using Newtonsoft.Json;
using StudioFourteen.Library.LibraryMenu;
using StudioFourteen.Plugin;
using StudioFourteen.Posing;
using StudioFourteen.Structs.Extensions;
using StudioFourteen.Tags;
using StudioFourteen.Utilities;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using System.Windows.Input;

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

	public async Task Save(int objectTableIndex, bool includeLegacyBones = true, HashSet<string>? includeBones = null)
	{
		await Threads.FrameworkThread();

		this.Bones = new();
		this.ReferenceRelativeBones = new();
		this.MainHand = null;
		this.OffHand = null;

		List<BoneReference> references = new();

		unsafe
		{
			Character* character = ServiceManager.Instance.Target.GetCharacter(objectTableIndex);
			if (character == null)
				return;

			CharacterBase* characterBase = character->GetCharacterBase();
			if (characterBase == null)
				return;

			ushort partialCount = characterBase->Skeleton->PartialSkeletonCount;
			for (int partialIdx = 0; partialIdx < partialCount; partialIdx++)
			{
				PartialSkeleton* partialSkeleton = &characterBase->Skeleton->PartialSkeletons[partialIdx];

				byte poseCount = partialSkeleton->GetMaxPoses();
				for (byte poseIdx = 0; poseIdx < poseCount; poseIdx++)
				{
					hkaPose* pose = partialSkeleton->GetHavokPose(poseIdx);
					if (pose == null)
						continue;

					int boneCount = pose->Skeleton->Bones.Length;
					for (short boneIdx = 0; boneIdx < boneCount; boneIdx++)
					{
						hkaBone bone = pose->Skeleton->Bones[boneIdx];
						string? boneName = bone.Name.String;

						if (boneName == null)
							continue;

						if (includeBones != null && !includeBones.Contains(boneName))
							continue;

						BoneId boneId = new(character->ObjectIndex, partialIdx, poseIdx, boneIdx);
						BoneReference reference = ServiceManager.Instance.Pose.GetOrCreateBoneReference(boneId, boneName);
						references.Add(reference);
					}
				}
			}
		}

		// Wait one frame for all the bone references to populate with real transform data.
		await Threads.NextFrame();

		foreach(BoneReference reference in references)
		{
			if (reference.Name == null)
				continue;

			// We'll have duplicate bone names, since we support indexing all the duplicate
			// HkPose and PartialSkeleton bones, but we can fairly safely assume the first
			// bone will be the one we want (from the lowest HkPose and PartialSkeleton)
			if (this.Bones.ContainsKey(reference.Name))
				continue;

			if (this.ReferenceRelativeBones.ContainsKey(reference.Name))
				continue;

			// Legacy bone format for backwards compatibility
			if (includeLegacyBones && reference.ModelSpaceTransform != null)
			{
				Transform hkModelSpaceTransform = reference.ModelSpaceTransform.Value;
				if (reference.Transform != null)
					hkModelSpaceTransform *= (Transform)reference.Transform;

				LegacyBoneTransform modelSpaceTransform = new();
				modelSpaceTransform.Position = hkModelSpaceTransform.Translation;
				modelSpaceTransform.Rotation = hkModelSpaceTransform.Rotation;
				modelSpaceTransform.Scale = hkModelSpaceTransform.Scale;
				this.Bones.Add(reference.Name, modelSpaceTransform);
			}

			// New format bones
			if (reference.LocalSpaceTransform != null && reference.ReferenceTransform != null)
			{
				Transform hkReferenceRelativeTransform = reference.LocalSpaceTransform.Value;
				if (reference.Transform != null)
					hkReferenceRelativeTransform *= (Transform)reference.Transform;

				hkReferenceRelativeTransform /= (Transform)reference.ReferenceTransform;

				Transform? referenceRelative = reference.ReferenceRelativeTransform;
				if (referenceRelative == null)
					continue;

				BoneTransform boneTransform = new();
				if (includeBones == null)
				{
					// Null out components that are irrelevantly small
					if (!referenceRelative.Value.Translation.IsApproximately(Vector3.Zero, 0.001f))
						boneTransform.Translation = referenceRelative.Value.Translation;

					// If the rotation quat has no x,y, or z component, then ignore it, as 0,0,0,1 is identity, and
					// a W component without X,Y,Z components doesn't do anything afaik.
					if (referenceRelative.Value.Rotation.X.IsApproximately(0, 0.001f)
						&& referenceRelative.Value.Rotation.Y.IsApproximately(0, 0.001f)
						&& referenceRelative.Value.Rotation.Z.IsApproximately(0, 0.001f))
						boneTransform.Rotation = null;

					if (referenceRelative.Value.Scale.IsApproximately(Vector3.One, 0.001f))
						boneTransform.Scale = null;

					// If all the components were irrelevantly small, then return null
					if (boneTransform.Translation == null
						&& boneTransform.Rotation == null
						&& boneTransform.Scale == null)
					{
						continue;
					}
				}

				this.ReferenceRelativeBones.Add(reference.Name, boneTransform);
			}
		}
	}

	public List<BoneReference>? GetBoneReferences(int objectTableIndex, bool includeFace)
	{
		Threads.VerifyFrameworkThread();

		bool useReferenceRelativeBones = this.ReferenceRelativeBones != null;
		List<BoneReference> boneReferences = new();

		unsafe
		{
			Character* character = ServiceManager.Instance.Target.GetCharacter(objectTableIndex);
			if (character == null)
				return null;

			CharacterBase* characterBase = character->GetCharacterBase();
			if (characterBase == null)
				return null;

			ushort partialCount = characterBase->Skeleton->PartialSkeletonCount;
			for (int partialIdx = 0; partialIdx < partialCount; partialIdx++)
			{
				PartialSkeleton* partialSkeleton = &characterBase->Skeleton->PartialSkeletons[partialIdx];

				byte poseCount = partialSkeleton->GetMaxPoses();
				for (byte poseIdx = 0; poseIdx < poseCount; poseIdx++)
				{
					hkaPose* pose = partialSkeleton->GetHavokPose(poseIdx);
					if (pose == null)
						continue;

					int boneCount = pose->Skeleton->Bones.Length;
					for (short boneIdx = 0; boneIdx < boneCount; boneIdx++)
					{
						hkaBone bone = pose->Skeleton->Bones[boneIdx];
						string? boneName = bone.Name.String;

						if (boneName == null)
							continue;

						if (boneName == "n_root")
							continue;

						if (!includeFace && boneName.StartsWith("j_f_"))
						{
							continue;
						}

						BoneId boneId = new(character->ObjectIndex, partialIdx, poseIdx, boneIdx);

						if (useReferenceRelativeBones)
						{
							BoneTransform? val = null;
							this.ReferenceRelativeBones?.TryGetValue(boneName, out val);

							if (val != null)
							{
								BoneReference reference = ServiceManager.Instance.Pose.GetOrCreateBoneReference(boneId, boneName);
								boneReferences.Add(reference);
							}
						}
						else
						{
							LegacyBoneTransform? val = null;
							if (this.Bones?.TryGetValue(boneName, out val) != true)
							{
								string? legacyName = LegacyBoneNameConverter.GetLegacyName(boneName);
								if (legacyName != null)
								{
									this.Bones?.TryGetValue(legacyName, out val);
								}
							}

							if (val != null)
							{
								BoneReference reference = ServiceManager.Instance.Pose.GetOrCreateBoneReference(boneId, boneName);
								boneReferences.Add(reference);
							}
						}
					}
				}
			}
		}

		return boneReferences;
	}

	[LibraryMenuTarget(IconChar.Running, "LOC_AppearanceApplyTo")]
	public async Task Apply(int objectTableIndex)
	{
		await Threads.FrameworkThread();

		bool useReferenceRelativeBones = this.ReferenceRelativeBones != null;

		bool includeFace = false;
		if (useReferenceRelativeBones)
		{
			includeFace = true;
		}
		else
		{
			// TODO: check if all races have these bones or its just Hyur!
			includeFace = false; //// this.Bones?.ContainsKey("j_f_ulip_02_l") == true;
		}

		// Get bone references
		List<BoneReference>? boneReferences = this.GetBoneReferences(objectTableIndex, includeFace);
		if (boneReferences == null)
			return;

		// Wait for a tick to update all bone references
		await Threads.NextFrame();

		// Apply values
		foreach (BoneReference boneReference in boneReferences)
		{
			if (boneReference.Name == null)
				continue;

			if (useReferenceRelativeBones)
			{
				BoneTransform? val = null;
				this.ReferenceRelativeBones?.TryGetValue(boneReference.Name, out val);

				if (val != null)
				{
					boneReference.SetReferenceRelativeTransform(val);
					boneReference.Locked = true;
				}
			}
			else
			{
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
				}
			}
		}
	}

	public Task Revert(int objectTableIndex)
	{
		ServiceManager.Instance.Pose.FlushBoneReferences(objectTableIndex);
		return Task.CompletedTask;
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
}