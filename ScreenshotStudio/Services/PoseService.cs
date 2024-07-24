// Brio
// https://github.com/Etheirys/Brio/tree/main/Brio/Game/Posing/SkeletonService.cs

namespace ScreenshotStudio.Services;

using Dalamud.Hooking;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Graphics.Render;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using FFXIVClientStructs.Havok.Animation.Rig;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class PoseService : ServiceBase
{
	private SelectionBase? selectedBone;

	private Hook<UpdateBonePhysicsDelegate>? updateBonePhysicsHook;
	private Hook<FinalizeSkeletonsDelegate>? finalizeSkeletonsHook;

	public delegate void SelectionChangedDelegate(SelectionBase? newSelection);
	private delegate nint UpdateBonePhysicsDelegate(nint a1);
	private delegate void FinalizeSkeletonsDelegate(nint a1);

	public event SelectionChangedDelegate? SelectionChanged;

	public SelectionBase? Selection
	{
		get => this.selectedBone;
		set
		{
			this.selectedBone = value;
			this.SelectionChanged?.Invoke(value);
			this.RaisePropertyChanged();
		}
	}

	public override Task Start()
	{
		this.Attach();
		return base.Start();
	}

	public override Task Shutdown()
	{
		this.Detach();
		return base.Shutdown();
	}

	public unsafe BoneSelection? FindBone(ref Skeleton* skeleton, string name)
	{
		List<BoneReference> bones = new();

		ushort partialCount = skeleton->PartialSkeletonCount;
		for (int partialIdx = 0; partialIdx < partialCount; partialIdx++)
		{
			PartialSkeleton* partialSkeleton = &skeleton->PartialSkeletons[partialIdx];

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

					if (boneName == name)
					{
						BoneReference boneReference = new(boneName, partialIdx, poseIdx, boneIdx);
						bones.Add(boneReference);

						short parentIndex = pose->Skeleton->ParentIndices[boneIdx];
						hkaBone parentBone = pose->Skeleton->Bones[parentIndex];
						boneReference.Parent = new(parentBone.Name.String, partialIdx, poseIdx, parentIndex);
					}
				}
			}
		}

		if (bones.Count <= 0)
			return null;

		return new BoneSelection(bones, name);
	}

	private void Attach()
	{
		this.updateBonePhysicsHook = InteropService.HookFromSignature<UpdateBonePhysicsDelegate>("48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 41 56 41 57 48 83 EC ?? 48 8B 79 ?? 45 33 FF", this.UpdateBonePhysicsDetour);
		this.updateBonePhysicsHook?.Enable();

		// JMP in Framework.TaskRenderGraphicsRender
		this.finalizeSkeletonsHook = InteropService.HookFromSignature<FinalizeSkeletonsDelegate>("40 53 55 57 48 83 EC ?? 65 48 8B 04 25", this.FinalizeSkeletonsHook);
		this.finalizeSkeletonsHook?.Enable();
	}

	private void Detach()
	{
		this.updateBonePhysicsHook?.Dispose();
		this.finalizeSkeletonsHook?.Dispose();
	}

	private nint UpdateBonePhysicsDetour(nint a1)
	{
		if (this.updateBonePhysicsHook == null)
			return 0;

		nint result = this.updateBonePhysicsHook.Original(a1);

		try
		{
			this.BeginSkeletonUpdate();
		}
		catch (Exception ex)
		{
			this.Log.Error(ex, "Error during skeleton update");
		}

		return result;
	}

	private void FinalizeSkeletonsHook(nint a1)
	{
		if (this.finalizeSkeletonsHook == null)
			return;

		this.finalizeSkeletonsHook.Original(a1);

		try
		{
			this.FinalizeSkeletonUpdate();
		}
		catch (Exception e)
		{
			this.Log.Error(e, "Error during skeleton finalization");
		}
	}

	private void BeginSkeletonUpdate()
	{
		// This is a very hot path, be careful how much you do here.
		// All the main skeleton stuff like positions, IK and physics is done at this point.
		if (!this.Services.GroupPose.IsGroupPosing)
			return;
	}

	private void FinalizeSkeletonUpdate()
	{
		if (!this.Services.GroupPose.IsGroupPosing)
			return;
	}
}

public class BoneReference(string? name, int partialSkeletonIndex, byte poseIndex, short boneIndex)
	: IEquatable<BoneReference?>
{
	public readonly string? Name = name;
	public readonly int PartialSkeletonIndex = partialSkeletonIndex;
	public readonly byte PoseIndex = poseIndex;
	public readonly short BoneIndex = boneIndex;

	public BoneReference? Parent { get; set; }

	public static bool operator ==(BoneReference? left, BoneReference? right)
	{
		return EqualityComparer<BoneReference>.Default.Equals(left, right);
	}

	public static bool operator !=(BoneReference? left, BoneReference? right)
	{
		return !(left == right);
	}

	public override bool Equals(object? obj)
	{
		return this.Equals(obj as BoneReference);
	}

	public bool Equals(BoneReference? other)
	{
		return other is not null &&
			   this.PartialSkeletonIndex == other.PartialSkeletonIndex &&
			   this.PoseIndex == other.PoseIndex &&
			   this.BoneIndex == other.BoneIndex;
	}

	public unsafe object? GetBone(ref Skeleton* skeleton)
	{
		PartialSkeleton* partialSkeleton = &skeleton->PartialSkeletons[this.PartialSkeletonIndex];
		if (partialSkeleton == null)
			return null;

		hkaPose* pose = partialSkeleton->GetHavokPose(this.PoseIndex);
		if (pose == null)
			return null;

		// ...
		// hkaBone bone = pose->Skeleton->Bones[ this.BoneIndex];
		return null;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(this.PartialSkeletonIndex, this.PoseIndex, this.BoneIndex);
	}
}

public abstract class SelectionBase
{
	public abstract string Name { get; }
}

public class BoneSelection(List<BoneReference> bones, string name)
	: SelectionBase
{
	public override string Name => Resources.Find($"LOC_Bone_{name}", name);
	public string BoneName => name;
	public List<BoneReference> Bones { get; init; } = bones;
}