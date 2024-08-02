namespace ScreenshotStudio.Files;

using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Graphics.Render;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using FFXIVClientStructs.Havok.Animation.Rig;
using ScreenshotStudio.Posing;
using ScreenshotStudio.Structs;
using ScreenshotStudio.Structs.Extensions;
using ScreenshotStudio.Utilities;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Xml.Linq;

public class PoseFileTypeInfo : JsonFileTypeInfoBase<PoseFile>
{
	public override string Extension => ".pose";
	public override string TypeName => "Pose";
}

[Serializable]
public class PoseFile : FileBase, IPose
{
	public BoneTransform? ModelDifference { get; set; }

	public Dictionary<string, BoneTransform>? Bones { get; set; } = [];
	public Dictionary<string, BoneTransform>? MainHand { get; set; } = [];
	public Dictionary<string, BoneTransform>? OffHand { get; set; } = [];

	public unsafe void Apply(Character* character)
	{
		Threads.VerifyFrameworkThread();

		if (this.Bones != null)
		{
			PoseService service = ServiceManager.Instance.Pose;

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

						if (boneName == "n_root")
							continue;

						if (this.Bones.TryGetValue(boneName, out BoneTransform? val))
						{
							BoneId boneId = new(character->ObjectIndex, partialIdx, poseIdx, boneIdx, boneName);
							BoneReference reference = service.GetOrCreateBoneReference(boneId, boneName);
							reference.Mode = BoneReference.Modes.Absolute;
							reference.CurrentTransform.Translation.FromVector3(val.Position);
							reference.CurrentTransform.Rotation.FromQuaternion(val.Rotation);
							reference.CurrentTransform.Scale.FromVector3(val.Scale);
						}
					}
				}
			}
		}
	}

	public unsafe void Revert(Character* character)
	{
		ServiceManager.Instance.Pose.FlushBoneReferences(character);
	}

	public class BoneTransform
	{
		public Vector3 Position { get; set; }
		public Quaternion Rotation { get; set; }
		public Vector3 Scale { get; set; }
	}
}