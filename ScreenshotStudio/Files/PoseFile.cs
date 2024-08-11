namespace ScreenshotStudio.Files;

using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Graphics.Render;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using FFXIVClientStructs.Havok.Animation.Rig;
using ScreenshotStudio.Plugin;
using ScreenshotStudio.Posing;
using ScreenshotStudio.Structs;
using ScreenshotStudio.Structs.Extensions;
using ScreenshotStudio.Utilities;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
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

	public async Task Apply(int objectTableIndex)
	{
		await Threads.FrameworkThread();

		Threads.VerifyFrameworkThread();

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

				CharacterBase* characterBase = character->GetCharacterBase();
				if (characterBase == null)
					return;

				// TODO: check if all races have these bones or its just Hyur!
				bool includeFace = this.Bones.ContainsKey("j_f_ulip_02_l");

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