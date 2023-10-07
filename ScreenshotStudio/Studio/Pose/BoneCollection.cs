//// Ktisis
//// https://github.com/ktisis-tools/Ktisis/
//// https://github.com/ktisis-tools/Ktisis/blob/main/Ktisis/Structs/Bones/Bone.cs

namespace ScreenshotStudio.Studio.Pose;

using FFXIVClientStructs.FFXIV.Client.Graphics.Render;
using FFXIVClientStructs.Havok;
using System.Collections.Generic;

public class BoneCollection : HashSet<Bone>
{
	public hkQsTransformf Transform
	{
		get
		{
			foreach(Bone reference in this)
			{
				return reference.Transform;
			}

			return default;
		}

		set
		{
			foreach (Bone reference in this)
			{
				reference.Apply(value);
			}
		}
	}

	public string? DisplayName
	{
		get
		{
			// TODO: append these together?
			foreach (Bone reference in this)
			{
				return reference.Name;
			}

			return null;
		}
	}

	public static unsafe BoneCollection? Search(Skeleton* skeleton, string boneName)
	{
		BoneCollection results = new();
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

					results.Add(new Bone(skeleton, partialSkeletonIndex, poseIndex, i));
				}
			}
		}

		if (results.Count == 0)
			return null;

		return results;
	}

	public void Remove(IEnumerable<Bone> bones)
	{
		foreach (Bone bone in bones)
		{
			this.Remove(bone);
		}
	}

	public void Add(IEnumerable<Bone> childBones)
	{
		foreach (Bone bone in childBones)
		{
			this.Add(bone);
		}
	}

	public bool Contains(IEnumerable<Bone> bones)
	{
		bool contains = true;
		foreach (Bone bone in bones)
		{
			contains &= this.Contains(bone);
		}

		return contains;
	}

	public BoneCollection? GetParents()
	{
		BoneCollection parents = new();
		foreach (Bone bone in this)
		{
			Bone? parent = bone.GetParent();

			if (parent == null)
				continue;

			parents.Add(parent);
		}

		if (parents.Count <= 0)
			return null;

		return parents;
	}

	public BoneCollection? GetChildren()
	{
		BoneCollection allChildren = new();
		foreach (Bone bone in this)
		{
			BoneCollection? children = bone.GetChildren();

			if (children == null)
				continue;

			allChildren.Add(children);
		}

		if (allChildren.Count <= 0)
			return null;

		return allChildren;
	}
}