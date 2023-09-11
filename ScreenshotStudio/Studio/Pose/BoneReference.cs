// © XivTools.
// Licensed under the MIT license.

namespace ScreenshotStudio.Studio.Pose;

using FFXIVClientStructs.FFXIV.Client.Graphics.Render;
using FFXIVClientStructs.Havok;
using System;
using System.Collections;
using System.Collections.Generic;

public unsafe class BoneReference
{
	private readonly Skeleton* skeleton;
	private readonly int partialSkeletonIndex;
	private readonly int poseIndex;
	private readonly int boneIndex;
	private BoneReferences? children = null;

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

	public static bool operator !=(BoneReference? left, BoneReference? right) => !(left == right);

	public static bool operator ==(BoneReference? left, BoneReference? right)
	{
		if (left is null && right is null)
			return true;

		if (left is null)
			return false;

		return left.Equals(right);
	}

	public override int GetHashCode() => HashCode.Combine(this.partialSkeletonIndex, this.poseIndex, this.boneIndex);

	public override bool Equals(object? obj)
	{
		if (ReferenceEquals(this, obj))
			return true;

		if (obj is null)
			return false;

		if (obj is not BoneReference right)
			return false;

		return this.partialSkeletonIndex == right.partialSkeletonIndex
			&& this.poseIndex == right.poseIndex
			&& this.boneIndex == right.boneIndex;
	}

	public BoneReference? GetParent()
	{
		int? parentIndex = this.HkaPose->Skeleton->ParentIndices[this.boneIndex];
		if (parentIndex == -1 || parentIndex == null)
			return null;

		return new BoneReference(this.skeleton, this.partialSkeletonIndex, this.poseIndex, (int)parentIndex);
	}

	public BoneReferences? GetChildren()
	{
		if (this.children != null)
			return this.children;

		this.children = new();

		for (int childIndex = 0; childIndex < this.HkaPose->Skeleton->ParentIndices.Length; childIndex++)
		{
			if (this.HkaPose->Skeleton->ParentIndices[childIndex] == this.boneIndex)
			{
				this.children.Add(new BoneReference(this.skeleton, this.partialSkeletonIndex, this.poseIndex, childIndex));
			}
		}

		return this.children;
	}
}

public class BoneReferences : HashSet<BoneReference>
{
	public hkQsTransformf Transform
	{
		get
		{
			foreach(BoneReference reference in this)
			{
				return reference.Transform;
			}

			return default;
		}

		set
		{
			foreach (BoneReference reference in this)
			{
				reference.Transform = value;
			}
		}
	}

	public string? DisplayName
	{
		get
		{
			// TODO: append these together?
			foreach (BoneReference reference in this)
			{
				return reference.Name;
			}

			return null;
		}
	}

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

	public void Remove(IEnumerable<BoneReference> bones)
	{
		foreach (BoneReference bone in bones)
		{
			this.Remove(bone);
		}
	}

	public void Add(IEnumerable<BoneReference> childBones)
	{
		foreach (BoneReference bone in childBones)
		{
			this.Add(bone);
		}
	}

	public bool Contains(IEnumerable<BoneReference> bones)
	{
		bool contains = true;
		foreach (BoneReference bone in bones)
		{
			contains &= this.Contains(bone);
		}

		return contains;
	}

	public BoneReferences? GetParents()
	{
		BoneReferences parents = new();
		foreach (BoneReference bone in this)
		{
			BoneReference? parent = bone.GetParent();

			if (parent == null)
				continue;

			parents.Add(parent);
		}

		if (parents.Count <= 0)
			return null;

		return parents;
	}

	public BoneReferences? Getchildren()
	{
		BoneReferences allChildren = new();
		foreach (BoneReference bone in this)
		{
			BoneReferences? children = bone.GetChildren();

			if (children == null)
				continue;

			allChildren.Add(children);
		}

		if (allChildren.Count <= 0)
			return null;

		return allChildren;
	}
}