// © XivTools.
// Licensed under the MIT license.

namespace ScreenshotStudio.Studio.Pose;

using FFXIVClientStructs.FFXIV.Client.Graphics.Render;
using FFXIVClientStructs.Havok;
using System.Collections.Generic;

public unsafe class BoneReference
{
	private readonly Skeleton* skeleton;
	private readonly int partialSkeletonIndex;
	private readonly int poseIndex;
	private readonly int boneIndex;

	public BoneReference(Skeleton* skeleton, int partialSkeletonIndex, int poseIndex, int boneIndex)
	{
		this.skeleton = skeleton;
		this.partialSkeletonIndex = partialSkeletonIndex;
		this.poseIndex = poseIndex;
		this.boneIndex = boneIndex;
	}

	public Skeleton* Skeleton => this.skeleton;
	public ref PartialSkeleton PartialSkeleton => ref Skeleton->PartialSkeletons[this.partialSkeletonIndex];
	public hkaPose* HkaPose => this.PartialSkeleton.GetHavokPose(this.poseIndex);

	public unsafe hkaBone HkaBone => HkaPose->Skeleton->Bones[this.boneIndex];
	public unsafe int ParentId => HkaPose->Skeleton->ParentIndices[this.boneIndex];

	public string? Name => this.HkaBone.Name.String;

	public hkQsTransformf Transform
	{
		get => HkaPose->ModelPose[this.boneIndex];
		set => HkaPose->ModelPose[this.boneIndex] = value;
	}
}

public class BoneReferences : List<BoneReference>
{
	public hkQsTransformf Transform
	{
		get
		{
			if (this.Count < 1)
				return default;

			return this[0].Transform;
		}

		set
		{
			foreach (BoneReference reference in this)
			{
				reference.Transform = value;
			}
		}
	}

	public string? Name => this.Count <= 0 ? null : this[0].Name;

	public static unsafe BoneReferences? Search(Skeleton* skeleton, string boneName)
	{
		BoneReferences results = new();
		for (int partialSkeletonIndex = 0; partialSkeletonIndex < skeleton->PartialSkeletonCount; partialSkeletonIndex++)
		{
			PartialSkeleton partialSkeleton = skeleton->PartialSkeletons[partialSkeletonIndex];

			for (int poseIndex = 0; poseIndex < 4; poseIndex++)
			{
				hkaPose* pose = partialSkeleton.GetHavokPose(poseIndex);
				if (pose == null)
					continue;

				for (int i = 0; i < pose->Skeleton->Bones.Length; i++)
				{
					string? testBoneName = pose->Skeleton->Bones[i].Name.String;
					if (testBoneName != boneName)
						continue;

					results.Add(new BoneReference(skeleton, partialSkeletonIndex, poseIndex, i));
				}
			}
		}

		if (results.Count == 0)
			return null;

		return results;
	}
}