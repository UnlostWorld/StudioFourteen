//// Ktisis
//// https://github.com/ktisis-tools/Ktisis/
//// https://github.com/ktisis-tools/Ktisis/blob/main/Ktisis/Structs/Bones/Bone.cs#L71

namespace ScreenshotStudio.Studio.Pose;

using FFXIVClientStructs.FFXIV.Client.Graphics.Render;
using FFXIVClientStructs.Havok;
using ScreenshotStudio.Plugin;
using ScreenshotStudio.Structs;
using ScreenshotStudio.Structs.Extensions;
using Serilog;
using System;
using System.Numerics;

using static FFXIVClientStructs.Havok.hkaPose;

public unsafe class Bone
{
	protected readonly ILogger Log = Logging.ForContext<Gizmo>();

	private readonly Skeleton* skeleton;
	private readonly int partialSkeletonIndex;
	private readonly int poseIndex;
	private readonly int boneIndex;

	public Bone(Skeleton* skeleton, int partialSkeletonIndex, int poseIndex, int boneIndex)
	{
		this.skeleton = skeleton;
		this.partialSkeletonIndex = partialSkeletonIndex;
		this.poseIndex = poseIndex;
		this.boneIndex = boneIndex;
	}

	public Skeleton* Skeleton => this.skeleton;
	public ref PartialSkeleton PartialSkeleton => ref Skeleton->PartialSkeletons[this.partialSkeletonIndex];
	public hkaPose* HkaPose => this.PartialSkeleton.GetHavokPose(this.poseIndex);

	public unsafe int ParentId => this.HkaPose->Skeleton->ParentIndices[this.boneIndex];

	public string? Name
	{
		get
		{
			if (this.HkaPose == null || this.HkaPose->Skeleton == null)
				return null;

			return this.HkaPose->Skeleton->Bones[this.boneIndex].Name.String;
		}
	}

	public hkQsTransformf Transform
	{
		get => this.HkaPose->ModelPose[this.boneIndex];
		protected set => this.HkaPose->ModelPose[this.boneIndex] = value;
	}

	public static bool operator !=(Bone? left, Bone? right) => !(left == right);

	public static bool operator ==(Bone? left, Bone? right)
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

		if (obj is not Bone right)
			return false;

		return this.partialSkeletonIndex == right.partialSkeletonIndex
			&& this.poseIndex == right.poseIndex
			&& this.boneIndex == right.boneIndex;
	}

	public unsafe hkQsTransformf* AccessModelSpace(PropagateOrNot propagate = PropagateOrNot.DontPropagate) => this.HkaPose->AccessBoneModelSpace(this.boneIndex, propagate);
	public unsafe hkQsTransformf* AccessLocalSpace() => this.HkaPose->AccessBoneLocalSpace(this.boneIndex);

	public Bone? GetParent()
	{
		int? parentIndex = this.HkaPose->Skeleton->ParentIndices[this.boneIndex];
		if (parentIndex == -1 || parentIndex == null)
			return null;

		return new Bone(this.skeleton, this.partialSkeletonIndex, this.poseIndex, (int)parentIndex);
	}

	public BoneCollection GetChildren(bool includePartials = true, bool usePartialRoot = false)
	{
		BoneCollection results = new();

		// Add child bones from same partial
		for (int childIndex = 0; childIndex < this.HkaPose->Skeleton->ParentIndices.Length; childIndex++)
		{
			if (this.HkaPose->Skeleton->ParentIndices[childIndex] == this.boneIndex)
			{
				results.Add(new Bone(this.skeleton, this.partialSkeletonIndex, this.poseIndex, childIndex));
			}
		}

		// Add child bones from connected partials
		if (includePartials && this.partialSkeletonIndex == 0)
		{
			for (int partialSkeletonIndex = 0; partialSkeletonIndex < Skeleton->PartialSkeletonCount; partialSkeletonIndex++)
			{
				if (partialSkeletonIndex == this.partialSkeletonIndex)
					continue;

				PartialSkeleton partial = Skeleton->PartialSkeletons[partialSkeletonIndex];
				if (partial.ConnectedParentBoneIndex == this.boneIndex)
				{
					Bone partialRoot = new Bone(this.skeleton, partialSkeletonIndex, this.poseIndex, partial.ConnectedBoneIndex);
					if (usePartialRoot)
					{
						results.Add(partialRoot);
					}
					else
					{
						BoneCollection? rootChildren = partialRoot.GetChildren();
						foreach (Bone child in rootChildren)
						{
							results.Add(child);
						}
					}
				}
			}
		}

		return results;
	}

	public BoneCollection GetDescendants(ref BoneCollection results, bool includePartials = true, bool usePartialRoot = false)
	{
		BoneCollection children = this.GetChildren(includePartials, usePartialRoot);
		results.Add(children);

		foreach (Bone bone in children)
		{
			bone.GetDescendants(ref results, includePartials, usePartialRoot);
		}

		return children;
	}

	public unsafe void Apply(hkQsTransformf transform)
	{
		DalamudServices.Framework.RunOnFrameworkThread(() =>
		{
			hkQsTransformf origin = transform;

			Vector3 deltaScale = transform.Scale.ToVector3() - this.Transform.Scale.ToVector3();
			Quaternion deltaRotation = transform.Rotation.ToQuaternion() / this.Transform.Rotation.ToQuaternion();
			Vector3 deltaTranslate = transform.Translation.ToVector3() - this.Transform.Translation.ToVector3();

			this.Transform = transform;

			// if enable parenting?
			this.PropagateChildren(origin, deltaTranslate, deltaRotation, deltaScale, true);
		});
	}

	public unsafe void PropagateChildren(hkQsTransformf origin, Vector3 deltaTranslate, Quaternion deltaRotation, Vector3 deltaScale, bool includePartials = true)
	{
		// Bone parenting
		// Adapted from Ktisis code shared by Chirp - thank you!
		BoneCollection descendants = new();
		this.GetDescendants(ref descendants, includePartials, true);

		DalamudServices.Framework.RunOnFrameworkThread(() =>
		{
			foreach (Bone descendant in descendants)
			{
				hkQsTransformf* access = descendant.AccessModelSpace();

				Vector3 offset = access->Translation.ToVector3() - origin.Translation.ToVector3();
				offset = Vector3.Transform(offset, deltaRotation);

				Matrix4x4 matrix = Alloc.GetMatrix(access);
				matrix *= Matrix4x4.CreateFromQuaternion(deltaRotation);
				matrix.Translation = origin.Translation.ToVector3() + offset + deltaTranslate;
				Alloc.SetMatrix(access, matrix);
			}
		});
	}
}