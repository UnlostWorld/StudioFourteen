namespace ScreenshotStudio.Files;

using Dalamud.Game.ClientState.Objects.Enums;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Graphics.Render;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using FFXIVClientStructs.Havok.Animation.Rig;
using ScreenshotStudio.Commands;
using ScreenshotStudio.GameData.Excel;
using ScreenshotStudio.Plugin;
using ScreenshotStudio.Posing;
using ScreenshotStudio.Structs;
using ScreenshotStudio.Structs.Extensions;
using ScreenshotStudio.Utilities;
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
	public PoseFile()
	{
		this.ApplyCommand = new TargetCommand(this.Apply);
		this.RevertCommand = new TargetCommand(this.Revert);
	}

	public ICommand ApplyCommand { get; init; }
	public ICommand RevertCommand { get; init; }

	public BoneTransform? ModelDifference { get; set; }

	public Race.RaceRows? Race { get; set; }
	public Tribe.TribeRows? Tribe { get; set; }
	public Genders? Gender { get; set; }

	public Dictionary<string, BoneTransform>? Bones { get; set; } = new();
	public Dictionary<string, BoneTransform>? MainHand { get; set; } = new();
	public Dictionary<string, BoneTransform>? OffHand { get; set; } = new();

	public async Task Save(int objectTableIndex)
	{
		await Threads.FrameworkThread();

		if (DalamudServices.ObjectTable == null)
			return;

		this.Bones = new();
		this.MainHand = null;
		this.OffHand = null;

		List<BoneReference> references = new();

		unsafe
		{
			Character* character = (Character*)DalamudServices.ObjectTable.GetObjectAddress(objectTableIndex);
			if (character == null)
				return;

			this.Race = (Race.RaceRows)character->GetCustomizeValue(CustomizeIndex.Race);
			this.Tribe = (Tribe.TribeRows)character->GetCustomizeValue(CustomizeIndex.Tribe);
			this.Gender = (Genders)character->GetCustomizeValue(CustomizeIndex.Gender);

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

						BoneId boneId = new(character->ObjectIndex, partialIdx, poseIdx, boneIdx, boneName);
						BoneReference reference = ServiceManager.Instance.Pose.GetOrCreateBoneReference(boneId, boneName);
						references.Add(reference);
					}
				}
			}
		}

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

			BoneTransform transform = new();
			transform.Position = reference.LastTransform.Translation.ToVector3();
			transform.Rotation = reference.LastTransform.Rotation.ToQuaternion();
			transform.Scale = reference.LastTransform.Scale.ToVector3();
			this.Bones.Add(reference.Name, transform);
		}
	}

	public async Task Apply(int objectTableIndex)
	{
		await Threads.FrameworkThread();

		if (DalamudServices.ObjectTable == null)
			return;

		if (this.Bones != null)
		{
			PoseService service = ServiceManager.Instance.Pose;

			unsafe
			{
				Character* character = (Character*)DalamudServices.ObjectTable.GetObjectAddress(objectTableIndex);
				if (character == null)
					return;

				// Only load translations if this pose was saved for the targets exact race, tribe, and gender,
				// otherwise the racial skeletal differences will warp the result too much.
				bool fullLoad = true;
				if (this.Race != null && this.Tribe != null && this.Gender != null)
				{
					fullLoad &= character->GetCustomizeValue(CustomizeIndex.Race) == (byte)this.Race;
					fullLoad &= character->GetCustomizeValue(CustomizeIndex.Tribe) == (byte)this.Tribe;
					fullLoad &= character->GetCustomizeValue(CustomizeIndex.Gender) == (byte)this.Gender;
				}
				else
				{
					fullLoad = false;
				}

				CharacterBase* characterBase = character->GetCharacterBase();
				if (characterBase == null)
					return;

				// TODO: check if all races have these bones or its just Hyur!
				bool includeFace = this.Bones.ContainsKey("j_f_ulip_02_l");

				Dictionary<string, BoneId> boneIds = new();

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

							if (!includeFace
								&& boneName.StartsWith("j_f_")
								&& !boneName.StartsWith("j_f_eye_"))
							{
								continue;
							}

							BoneTransform? val;
							if (!this.Bones.TryGetValue(boneName, out val))
							{
								string? legacyName = LegacyBoneNameConverter.GetLegacyName(boneName);
								if (legacyName == null || !this.Bones.TryGetValue(legacyName, out val))
								{
									continue;
								}
							}

							if (val == null)
								continue;

							BoneId boneId = new(character->ObjectIndex, partialIdx, poseIdx, boneIdx, boneName);
							if (boneIds.ContainsKey(boneName))
								continue;

							boneIds.Add(boneName, boneId);

							BoneReference reference = service.GetOrCreateBoneReference(boneId, boneName);
							reference.Mode = BoneReference.Modes.Absolute;

							if (fullLoad)
							{
								reference.NextAbsoluteTranslation = val.Position.ToHkVector();
								reference.NextAbsoluteScale = val.Scale.ToHkVector();
							}

							reference.NextAbsoluteRotation = val.Rotation.ToHkQuaternion();
						}
					}
				}
			}
		}
	}

	public Task Revert(int objectTableIndex)
	{
		ServiceManager.Instance.Pose.FlushBoneReferences(objectTableIndex);
		return Task.CompletedTask;
	}

	public class BoneTransform
	{
		public Vector3 Position { get; set; }
		public Quaternion Rotation { get; set; }
		public Vector3 Scale { get; set; }
	}
}