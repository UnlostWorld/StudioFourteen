// Brio
// https://github.com/Etheirys/Brio/tree/main/Brio/Game/Posing/SkeletonService.cs

namespace ScreenshotStudio.Services;

using Dalamud.Hooking;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Graphics.Render;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using FFXIVClientStructs.Havok.Animation.Rig;
using FFXIVClientStructs.Havok.Common.Base.Math.QsTransform;
using FFXIVClientStructs.Havok.Common.Base.Math.Quaternion;
using FFXIVClientStructs.Havok.Common.Base.Math.Vector;
using ScreenshotStudio.Structs;
using ScreenshotStudio.Structs.Extensions;
using ScreenshotStudio.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

public class PoseService : ServiceBase
{
	private readonly Dictionary<BoneId, BoneReference> boneReferences = new();

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
		this.updateBonePhysicsHook = InteropService.HookFromSignature<UpdateBonePhysicsDelegate>("48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 41 56 41 57 48 83 EC ?? 48 8B 79 ?? 45 33 FF", this.UpdateBonePhysicsDetour);
		this.updateBonePhysicsHook?.Enable();

		// JMP in Framework.TaskRenderGraphicsRender
		this.finalizeSkeletonsHook = InteropService.HookFromSignature<FinalizeSkeletonsDelegate>("40 53 55 57 48 83 EC ?? 65 48 8B 04 25", this.FinalizeSkeletonsHook);
		this.finalizeSkeletonsHook?.Enable();

		this.Services.CharacterLifecycle.CharacterDestroyed += this.OnCharacterDestroyed;
		this.Services.GroupPose.StateChange += this.OnGroupPoseStateChange;

		return base.Start();
	}

	public override Task Shutdown()
	{
		this.updateBonePhysicsHook?.Dispose();
		this.finalizeSkeletonsHook?.Dispose();

		this.Services.CharacterLifecycle.CharacterDestroyed -= this.OnCharacterDestroyed;
		this.Services.GroupPose.StateChange -= this.OnGroupPoseStateChange;

		this.FlushBoneReferences();

		return base.Shutdown();
	}

	public BoneReference GetOrCreateBoneReference(BoneId id, string? name = null)
	{
		BoneReference? reference = null;
		if (this.boneReferences.TryGetValue(id, out reference))
			return reference;

		reference = new(name, id);
		this.boneReferences.Add(id, reference);
		return reference;
	}

	public unsafe BoneSelection? FindBone(ref Character* character, string name)
	{
		Threads.VerifyFrameworkThread();

		CharacterBase* characterBase = character->GetCharacterBase();
		if (characterBase == null)
			return null;

		List<BoneReference> bones = new();
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

					if (boneName == name)
					{
						BoneId id = new(character->ObjectIndex, partialIdx, poseIdx, boneIdx);

						BoneReference boneReference = this.GetOrCreateBoneReference(id, name);
						bones.Add(boneReference);

						short parentIndex = pose->Skeleton->ParentIndices[boneIdx];

						if (parentIndex == -1)
							continue;

						hkaBone parentBone = pose->Skeleton->Bones[parentIndex];

						BoneId parentId = new(character->ObjectIndex, partialIdx, poseIdx, parentIndex);
						boneReference.Parent = this.GetOrCreateBoneReference(parentId, parentBone.Name.String);
					}
				}
			}
		}

		if (bones.Count <= 0)
			return null;

		return new BoneSelection(bones, name);
	}

	public void FlushBoneReferences()
	{
		foreach ((BoneId id, BoneReference reference) in this.boneReferences)
			reference.Dispose();

		this.boneReferences.Clear();
	}

	public void FlushBoneReferences(uint objectTableIndex)
	{
		HashSet<BoneId> toRemove = new();
		foreach((BoneId id, BoneReference reference) in this.boneReferences)
		{
			if (id.ObjectTableIndex == objectTableIndex)
			{
				toRemove.Add(id);
				reference.Dispose();
			}
		}

		foreach (BoneId id in toRemove)
		{
			this.boneReferences.Remove(id);
		}
	}

	private unsafe nint UpdateBonePhysicsDetour(nint a1)
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

	// This is a very hot path, be careful how much you do here.
	// All the main skeleton stuff like positions, IK and physics is done at this point.
	private void BeginSkeletonUpdate()
	{
		foreach ((BoneId id, BoneReference reference) in this.boneReferences)
		{
			if (!reference.IsValid)
				continue;

			reference.UpdateCachedTransform();
		}
	}

	private void FinalizeSkeletonUpdate()
	{
	}

	private void OnCharacterDestroyed(uint objectTableIndex)
	{
		this.FlushBoneReferences(objectTableIndex);
	}

	private void OnGroupPoseStateChange(bool newState)
	{
		this.FlushBoneReferences();
	}
}

public abstract class SelectionBase
{
	public abstract string Name { get; }

	public abstract hkVector4f Translation { get; }
	public abstract hkQuaternionf Rotation { get; }
	public abstract hkVector4f Scale { get; }
	public abstract hkVector4f EulerRotation { get; }
}

public class BoneSelection(List<BoneReference> bones, string name)
	: SelectionBase
{
	public override string Name => Resources.Find($"LOC_Bone_{name}", name);
	public string BoneName => name;
	public List<BoneReference> Bones { get; init; } = bones;

	public BoneReference? DefaultBone
	{
		get
		{
			if (this.Bones.Count > 0)
				return this.Bones[0];

			return null;
		}
	}

	public override hkVector4f Translation => this.DefaultBone?.LastTransform.Translation ?? HkVectorExtensions.Zero;
	public override hkQuaternionf Rotation => this.DefaultBone?.LastTransform.Rotation ?? HkQuaternionExtensions.Identity;
	public override hkVector4f Scale => this.DefaultBone?.LastTransform.Scale ?? HkVectorExtensions.Zero;

	public override hkVector4f EulerRotation
	{
		get
		{
			if (this.DefaultBone == null)
				return HkVectorExtensions.Zero;

			hkQsTransformf transform = this.DefaultBone.LastTransform;
			return transform.Rotation.ToEuler();
		}
	}
}